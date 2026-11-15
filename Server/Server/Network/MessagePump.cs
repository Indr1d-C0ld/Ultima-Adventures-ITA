/***************************************************************************
 *                               MessagePump.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server;
using Server.Diagnostics;
using Server.Network;

namespace Server.Network
{
	public class MessagePump
	{
		private Listener[] m_Listeners;
		private Queue<NetState> m_Queue;
		private Queue<NetState> m_WorkingQueue;
		private Queue<NetState> m_Throttled;
		private const long SlowPacketThreshold = 50;
		private const long MovementPacketThreshold = 250;
		private const long SlowThrottleThreshold = 25;
		private const long StatsLogInterval = 30000;
		private const int MovementPacketID = 0x02;

		private static readonly object m_StatsLock = new object();
		private static readonly PacketStat[] m_PacketStats = new PacketStat[0x100];
		private static readonly ThrottleStat[] m_ThrottleStats = new ThrottleStat[0x100];
		private static long m_NextPacketStatsLog;
		private static long m_NextThrottleStatsLog;

		private sealed class PacketStat
		{
			public string Name;
			public int Count;
			public int SlowCount;
			public long TotalElapsed;
			public long MaxElapsed;
			public long LastElapsed;

			public void Reset()
			{
				Count = 0;
				SlowCount = 0;
				TotalElapsed = 0;
				MaxElapsed = 0;
				LastElapsed = 0;
			}
		}

		private sealed class ThrottleStat
		{
			public string Name;
			public int Checks;
			public int Requeued;
			public long TotalElapsed;
			public long MaxElapsed;
			public long LastElapsed;
			public int QueueMax;
			public int QueueLast;

			public void Reset()
			{
				Checks = 0;
				Requeued = 0;
				TotalElapsed = 0;
				MaxElapsed = 0;
				LastElapsed = 0;
				QueueMax = 0;
				QueueLast = 0;
			}
		}

		private static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		private static string FormatPacketHandler( PacketHandler handler )
		{
			if ( handler == null || handler.OnReceive == null || handler.OnReceive.Method == null )
				return "unknown";

			Type declaringType = handler.OnReceive.Method.DeclaringType;
			string typeName = declaringType != null ? declaringType.FullName : "unknown";

			return String.Format( "{0}.{1}", typeName, handler.OnReceive.Method.Name );
		}

		private static void LogSlowPacket( NetState ns, int packetID, PacketHandler handler, long elapsed )
		{
			string account = ( ns != null && ns.Account != null ) ? ns.Account.Username : "null";
			string mobile = ( ns != null && ns.Mobile != null ) ? ns.Mobile.ToString() : "null";
			string address = ns != null ? ns.ToString() : "null";

			DiagnosticConsole.WriteParsedLine( "[Lag] Packet 0x{0:X2} handler {1} took {2}ms for account={3} ip={4} mobile={5}", packetID, FormatPacketHandler( handler ), elapsed, account, address, mobile );
		}

		private static void LogSlowThrottle( NetState ns, int packetID, PacketHandler handler, long elapsed )
		{
			string account = ( ns != null && ns.Account != null ) ? ns.Account.Username : "null";
			string mobile = ( ns != null && ns.Mobile != null ) ? ns.Mobile.ToString() : "null";
			string address = ns != null ? ns.ToString() : "null";

			DiagnosticConsole.WriteParsedLine( "[Lag] Packet throttle 0x{0:X2} {1} took {2}ms for account={3} ip={4} mobile={5}", packetID, FormatPacketHandler( handler ), elapsed, account, address, mobile );
		}

		private static bool ShouldLogIndividualPacket( int packetID, long elapsed )
		{
			if ( packetID == MovementPacketID )
				return elapsed >= MovementPacketThreshold;

			return elapsed >= SlowPacketThreshold;
		}

		private static string GetPacketStatName( int packetID, PacketHandler handler )
		{
			if ( packetID == MovementPacketID )
				return "MovementReq";

			return FormatPacketHandler( handler );
		}

		private static void RecordPacketStats( int packetID, PacketHandler handler, long elapsed )
		{
			if ( packetID < 0 || packetID >= m_PacketStats.Length )
				return;

			lock ( m_StatsLock )
			{
				PacketStat stat = m_PacketStats[packetID];

				if ( stat == null )
					m_PacketStats[packetID] = stat = new PacketStat();

				if ( stat.Name == null )
					stat.Name = GetPacketStatName( packetID, handler );

				++stat.Count;
				stat.TotalElapsed += elapsed;
				stat.LastElapsed = elapsed;

				if ( elapsed > stat.MaxElapsed )
					stat.MaxElapsed = elapsed;

				if ( elapsed >= SlowPacketThreshold )
					++stat.SlowCount;

				long now = Core.TickCount;

				if ( m_NextPacketStatsLog == 0 )
				{
					m_NextPacketStatsLog = now + StatsLogInterval;
				}
				else if ( now >= m_NextPacketStatsLog )
				{
					m_NextPacketStatsLog = now + StatsLogInterval;
					LogPacketStatsLocked();
				}
			}
		}

		private static void RecordThrottleStats( int packetID, PacketHandler handler, long elapsed, bool requeued, int queueCount )
		{
			if ( packetID < 0 || packetID >= m_ThrottleStats.Length )
				return;

			lock ( m_StatsLock )
			{
				ThrottleStat stat = m_ThrottleStats[packetID];

				if ( stat == null )
					m_ThrottleStats[packetID] = stat = new ThrottleStat();

				if ( stat.Name == null )
					stat.Name = GetPacketStatName( packetID, handler );

				++stat.Checks;
				stat.TotalElapsed += elapsed;
				stat.LastElapsed = elapsed;
				stat.QueueLast = queueCount;

				if ( elapsed > stat.MaxElapsed )
					stat.MaxElapsed = elapsed;

				if ( queueCount > stat.QueueMax )
					stat.QueueMax = queueCount;

				if ( requeued )
					++stat.Requeued;

				long now = Core.TickCount;

				if ( m_NextThrottleStatsLog == 0 )
				{
					m_NextThrottleStatsLog = now + StatsLogInterval;
				}
				else if ( now >= m_NextThrottleStatsLog )
				{
					m_NextThrottleStatsLog = now + StatsLogInterval;
					LogThrottleStatsLocked();
				}
			}
		}

		private static void LogPacketStatsLocked()
		{
			for ( int i = 0; i < m_PacketStats.Length; ++i )
			{
				PacketStat stat = m_PacketStats[i];

				if ( stat == null || stat.Count == 0 )
					continue;

				if ( stat.SlowCount > 0 || stat.MaxElapsed >= SlowPacketThreshold )
				{
					long avg = stat.Count > 0 ? stat.TotalElapsed / stat.Count : 0;

					DiagnosticConsole.WriteParsedLine( "[Lag] PacketStats 30s: 0x{0:X2} {1} count={2} slow={3} avg={4}ms max={5}ms total={6}ms", i, stat.Name, stat.Count, stat.SlowCount, avg, stat.MaxElapsed, stat.TotalElapsed );
				}

				stat.Reset();
			}
		}

		private static void LogThrottleStatsLocked()
		{
			for ( int i = 0; i < m_ThrottleStats.Length; ++i )
			{
				ThrottleStat stat = m_ThrottleStats[i];

				if ( stat == null || stat.Checks == 0 )
					continue;

				if ( stat.Requeued > 0 || stat.MaxElapsed >= SlowThrottleThreshold )
				{
					DiagnosticConsole.WriteParsedLine( "[Lag] ThrottleStats 30s: 0x{0:X2} {1} checks={2} requeued={3} maxCallback={4}ms queue={5}", i, stat.Name, stat.Checks, stat.Requeued, stat.MaxElapsed, stat.QueueMax );
				}

				stat.Reset();
			}
		}

		public static void WritePacketStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_StatsLock )
			{
				writer.WriteLine( "Packet stats since last summary:" );

				for ( int i = 0; i < m_PacketStats.Length; ++i )
				{
					PacketStat stat = m_PacketStats[i];

					if ( stat == null || stat.Count == 0 )
						continue;

					long avg = stat.Count > 0 ? stat.TotalElapsed / stat.Count : 0;
					writer.WriteLine( "0x{0:X2} {1}: count={2} slow={3} avg={4}ms max={5}ms last={6}ms total={7}ms", i, stat.Name, stat.Count, stat.SlowCount, avg, stat.MaxElapsed, stat.LastElapsed, stat.TotalElapsed );
				}
			}
		}

		public static void WriteThrottleStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_StatsLock )
			{
				writer.WriteLine( "Throttle stats since last summary:" );

				for ( int i = 0; i < m_ThrottleStats.Length; ++i )
				{
					ThrottleStat stat = m_ThrottleStats[i];

					if ( stat == null || stat.Checks == 0 )
						continue;

					long avg = stat.Checks > 0 ? stat.TotalElapsed / stat.Checks : 0;
					writer.WriteLine( "0x{0:X2} {1}: checks={2} requeued={3} avg={4}ms max={5}ms last={6}ms queueMax={7} queueLast={8}", i, stat.Name, stat.Checks, stat.Requeued, avg, stat.MaxElapsed, stat.LastElapsed, stat.QueueMax, stat.QueueLast );
				}
			}
		}

		public MessagePump()
		{
			IPEndPoint[] ipep = Listener.EndPoints;

			m_Listeners = new Listener[ipep.Length];

			bool success = false;

			do {
				for ( int i = 0; i < ipep.Length; i++ ) {
					Listener l = new Listener( ipep[i] );
					if ( !success && l != null )
						success = true;
					m_Listeners[i] = l;
				}

				if ( !success ) {
					Console.WriteLine( "Retrying..." );
					Thread.Sleep( 10000 );
				}
			} while ( !success );

			m_Queue = new Queue<NetState>();
			m_WorkingQueue = new Queue<NetState>();
			m_Throttled = new Queue<NetState>();
		}

		public Listener[] Listeners
		{
			get{ return m_Listeners; }
			set{ m_Listeners = value; }
		}

		public void AddListener( Listener l )
		{
			Listener[] old = m_Listeners;

			m_Listeners = new Listener[old.Length + 1];

			for ( int i = 0; i < old.Length; ++i )
				m_Listeners[i] = old[i];

			m_Listeners[old.Length] = l;
		}

		private void CheckListener()
		{
			for ( int j = 0; j < m_Listeners.Length; ++j )
			{
				Socket[] accepted = m_Listeners[j].Slice();

				for ( int i = 0; i < accepted.Length; ++i )
				{
					NetState ns = new NetState( accepted[i], this );
					ns.Start();

					if ( ns.Running )
						Console.WriteLine( "Client: {0}: Connected. [{1} Online]", ns, NetState.Instances.Count );
				}
			}
		}

		public void OnReceive( NetState ns )
		{
			lock ( this )
				m_Queue.Enqueue( ns );

			Core.Set();
		}

		public void Slice()
		{
			CheckListener();

			lock ( this )
			{
				Queue<NetState> temp = m_WorkingQueue;
				m_WorkingQueue = m_Queue;
				m_Queue = temp;
			}

			while ( m_WorkingQueue.Count > 0 )
			{
				NetState ns = m_WorkingQueue.Dequeue();

				if ( ns.Running )
					HandleReceive( ns );
			}

			lock ( this )
			{
				while ( m_Throttled.Count > 0 )
					m_Queue.Enqueue( m_Throttled.Dequeue() );
			}
		}

		private const int BufferSize = 4096;
		private BufferPool m_Buffers = new BufferPool( "Processor", 4, BufferSize );

		private bool HandleSeed(NetState ns, ByteQueue buffer) {
			if (buffer.GetPacketID() == 0xEF) {
				// new packet in client	6.0.5.0	replaces the traditional seed method with a	seed packet
				// 0xEF	= 239 =	multicast IP, so this should never appear in a normal seed.	 So	this is	backwards compatible with older	clients.
				ns.Seeded = true;
				return true;
			} else if (buffer.Length >= 4) {
				byte[] m_Peek = new byte[4];

				buffer.Dequeue(m_Peek, 0, 4);

				int seed = (m_Peek[0] << 24) | (m_Peek[1] << 16) | (m_Peek[2] << 8) | m_Peek[3];

				if (seed == 0) {
					Console.WriteLine("Login: {0}: Invalid client detected, disconnecting", ns);
					ns.Dispose();
					return false;
				}

				ns.m_Seed = seed;
				ns.Seeded = true;
				return true;
			} else {
				return false;
			}
		}

		private bool CheckEncrypted(NetState ns, int packetID) {
			if (!ns.SentFirstPacket && packetID != 0xF0 && packetID != 0xF1 && packetID != 0xCF && packetID != 0x80 && packetID != 0x91 && packetID != 0xA4 && packetID != 0xEF) {
				Console.WriteLine("Client: {0}: Encrypted client detected, disconnecting", ns);
				ns.Dispose();
				return true;
			}
			return false;
		}

                public void HandleReceive( NetState ns )
                {
                        ByteQueue buffer = ns.Buffer;

                        if ( buffer == null || buffer.Length <= 0 )
				return;

			lock ( buffer )
			{
				if ( !ns.Seeded ) {
					if (!HandleSeed(ns, buffer))
						return;
				}

				int length = buffer.Length;

				while ( length > 0 && ns.Running ) {
					int packetID = buffer.GetPacketID();

					if (CheckEncrypted(ns, packetID))
						break;

                                        PacketHandler handler = ns.GetHandler( packetID );

                                        if ( handler == null ) {
#if NETFLIGHT_DIAGNOSTICS
                                                ns.FlightRecorder.RecordIntegrity(NetFlightInfoCode.UnknownPacket, (byte)packetID, length, "unknown packet");
#endif
                                                byte[] data = new byte[length];
                                                length = buffer.Dequeue( data, 0, length );
                                                new PacketReader( data, length, false ).Trace( ns );
                                                break;
                                        }

					int packetLength = handler.Length;

					if ( packetLength <= 0 ) 	{
                                                if ( length >= 3 ) {
                                                        packetLength = buffer.GetPacketLength();

                                                        if ( packetLength < 3 ) {
#if NETFLIGHT_DIAGNOSTICS
                                                                ns.FlightRecorder.RecordIntegrity(NetFlightInfoCode.InvalidLength, (byte)packetID, length, "invalid length");
#endif
                                                                ns.Dispose();
                                                                break;
                                                        }
                                                } else {
                                                        break;
                                                }
                                        }

                                        if ( length >= packetLength ) {
                                                if (handler.Ingame) {
                                                        if (ns.Mobile == null ) {
								Console.WriteLine( "Client: {0}: Sent ingame packet (0x{1:X2}) before having been attached to a mobile", ns, packetID );
								ns.Dispose();
								break;
							} else if (ns.Mobile.Deleted) {
								ns.Dispose();
								break;
							}
						}

						ThrottlePacketCallback throttler = handler.ThrottleCallback;

						if ( throttler != null )
						{
							long throttleStart = Stopwatch.GetTimestamp();
							bool allowed = throttler( ns );
							long throttleElapsed = GetElapsedMilliseconds( throttleStart );
							int throttledCount = m_Throttled.Count;

							if ( !allowed )
							{
								m_Throttled.Enqueue( ns );
								throttledCount = m_Throttled.Count;
							}

							RecordThrottleStats( packetID, handler, throttleElapsed, !allowed, throttledCount );

							if ( throttleElapsed >= SlowThrottleThreshold )
								LogSlowThrottle( ns, packetID, handler, throttleElapsed );

							if ( !allowed )
								return;
						}

#if NETFLIGHT_DIAGNOSTICS
                                                ns.FlightRecorder.RecordReceive((byte)packetID, packetLength);
#endif

                                                PacketReceiveProfile prof = null;

                                                if (Core.Profiling) prof = PacketReceiveProfile.Acquire( packetID );

						if ( prof != null ) {
							prof.Start();
						}

						byte[] packetBuffer;

						if ( BufferSize >= packetLength )
							packetBuffer = m_Buffers.AcquireBuffer();
						else
							packetBuffer = new byte[packetLength];

                                                packetLength = buffer.Dequeue( packetBuffer, 0, packetLength );

                                                PacketReader r =  new PacketReader( packetBuffer, packetLength, handler.Length != 0 );

                                                long packetStart = Stopwatch.GetTimestamp();

                                                try
                                                {
                                                        handler.OnReceive( ns, r );
                                                }
#if NETFLIGHT_DIAGNOSTICS
                                                catch ( Exception ex )
#else
                                                catch
#endif
                                                {
#if NETFLIGHT_DIAGNOSTICS
                                                        ns.FlightRecorder.RecordIntegrity(NetFlightInfoCode.HandlerException, (byte)packetID, packetLength, ex.Message);
#endif
                                                        throw;
                                                }

						long packetElapsed = GetElapsedMilliseconds( packetStart );

						RecordPacketStats( packetID, handler, packetElapsed );

						if ( ShouldLogIndividualPacket( packetID, packetElapsed ) )
							LogSlowPacket( ns, packetID, handler, packetElapsed );

                                                length = buffer.Length;

						if ( BufferSize >= packetLength )
							m_Buffers.ReleaseBuffer( packetBuffer );

						if ( prof != null ) {
							prof.Finish( packetLength );
						}
					} else {
						break;
					}
				}
			}
		}
	}
}
