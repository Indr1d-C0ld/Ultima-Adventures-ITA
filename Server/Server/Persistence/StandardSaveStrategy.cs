/***************************************************************************
 *                          StandardSaveStrategy.cs
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Server;
using Server.Guilds;
using Server.Diagnostics;

namespace Server {
	public class StandardSaveStrategy : SaveStrategy {
		public enum SaveOption
		{
			Normal,
			Threaded
		}

		public static SaveOption SaveType = SaveOption.Normal;

		public override string Name {
			get { return "Standard"; }
		}

		private Queue<Item> _decayQueue;
		private bool _permitBackgroundWrite;

		public StandardSaveStrategy() {
			_decayQueue = new Queue<Item>();
		}

		protected bool PermitBackgroundWrite { get { return _permitBackgroundWrite; } set { _permitBackgroundWrite = value; } }

		protected bool UseSequentialWriters { get { return (StandardSaveStrategy.SaveType == SaveOption.Normal || !_permitBackgroundWrite); } }

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			_permitBackgroundWrite = permitBackgroundWrite;

			SaveMobiles(metrics);
			SaveItems(metrics);
			SaveGuilds(metrics);

			if (permitBackgroundWrite && UseSequentialWriters)	//If we're permitted to write in the background, but we don't anyways, then notify.
				World.NotifyDiskWriteComplete();
		}

		protected void SaveMobiles(SaveMetrics metrics)
		{
			long methodStart = SaveDiagnostics.GetTimestamp();
			long phaseStart;
			Dictionary<Serial, Mobile> mobiles = World.Mobiles;

			GenericWriter idx;
			GenericWriter tdb;
			GenericWriter bin;

			phaseStart = SaveDiagnostics.GetTimestamp();

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter( World.MobileIndexPath, false );
				tdb = new BinaryFileWriter( World.MobileTypesPath, false );
				bin = new BinaryFileWriter( World.MobileDataPath, true );
			} else {
				idx = new AsyncWriter( World.MobileIndexPath, false );
				tdb = new AsyncWriter( World.MobileTypesPath, false );
				bin = new AsyncWriter( World.MobileDataPath, true );
			}

			SaveDiagnostics.RecordPhase( "mobilesOpen", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Write( ( int ) mobiles.Count );
			int savedCount = 0;
			long savedBytes = 0;
			bool deepProfiling = SaveDiagnostics.IsDeepProfilingActive;
			SaveDiagnostics.TypeCollector mobileTypeCollector = deepProfiling ? new SaveDiagnostics.TypeCollector( "mobiles" ) : null;

			foreach ( Mobile m in mobiles.Values ) {
				long start = bin.Position;

				idx.Write( ( int ) m.m_TypeRef );
				idx.Write( ( int ) m.Serial );
				idx.Write( ( long ) start );

				if ( deepProfiling )
				{
					long serializeStart = SaveDiagnostics.GetTimestamp();
					m.Serialize( bin );
					mobileTypeCollector.Record( m.GetType(), m.Name, m.Serial, SaveDiagnostics.GetElapsedMilliseconds( serializeStart ) );
				}
				else
				{
					m.Serialize( bin );
				}

				int length = ( int ) ( bin.Position - start );
				++savedCount;
				savedBytes += length;

				if ( metrics != null ) {
					metrics.OnMobileSaved( length );
				}

				idx.Write( length );

				m.FreeCache();
			}

			SaveDiagnostics.RecordEntityBatch( "mobiles", savedCount, savedBytes );
			SaveDiagnostics.RecordEntityProfilingCounts( deepProfiling ? savedCount : 0, deepProfiling ? 0 : savedCount );

			if ( deepProfiling )
				SaveDiagnostics.RecordTypeStats( mobileTypeCollector );

			SaveDiagnostics.RecordPhase( "mobilesSerialize", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			tdb.Write( ( int ) World.m_MobileTypes.Count );

			for ( int i = 0; i < World.m_MobileTypes.Count; ++i )
				tdb.Write( World.m_MobileTypes[i].FullName );

			SaveDiagnostics.RecordPhase( "mobilesTypeDb", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Close();
			tdb.Close();
			bin.Close();
			SaveDiagnostics.RecordPhase( "mobilesClose", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );
			SaveDiagnostics.RecordPhase( "mobiles", SaveDiagnostics.GetElapsedMilliseconds( methodStart ) );
		}

		protected void SaveItems(SaveMetrics metrics)
		{
			long methodStart = SaveDiagnostics.GetTimestamp();
			long phaseStart;
			Dictionary<Serial, Item> items = World.Items;

			GenericWriter idx;
			GenericWriter tdb;
			GenericWriter bin;

			phaseStart = SaveDiagnostics.GetTimestamp();

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter( World.ItemIndexPath, false );
				tdb = new BinaryFileWriter( World.ItemTypesPath, false );
				bin = new BinaryFileWriter( World.ItemDataPath, true );
			} else {
				idx = new AsyncWriter( World.ItemIndexPath, false );
				tdb = new AsyncWriter( World.ItemTypesPath, false );
				bin = new AsyncWriter( World.ItemDataPath, true );
			}

			SaveDiagnostics.RecordPhase( "itemsOpen", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Write( ( int ) items.Count );

			DateTime n = DateTime.UtcNow;
			int savedCount = 0;
			long savedBytes = 0;
			bool deepProfiling = SaveDiagnostics.IsDeepProfilingActive;
			SaveDiagnostics.TypeCollector itemTypeCollector = deepProfiling ? new SaveDiagnostics.TypeCollector( "items" ) : null;

			foreach ( Item item in items.Values ) {
				if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= n)
				{
					_decayQueue.Enqueue( item );
				}

				long start = bin.Position;

				idx.Write( ( int ) item.m_TypeRef );
				idx.Write( ( int ) item.Serial );
				idx.Write( ( long ) start );

				if ( deepProfiling )
				{
					long serializeStart = SaveDiagnostics.GetTimestamp();
					item.Serialize( bin );
					itemTypeCollector.Record( item.GetType(), item.Name, item.Serial, SaveDiagnostics.GetElapsedMilliseconds( serializeStart ) );
				}
				else
				{
					item.Serialize( bin );
				}

				int length = ( int ) ( bin.Position - start );
				++savedCount;
				savedBytes += length;

				if ( metrics != null ) {
					metrics.OnItemSaved( length );
				}

				idx.Write( length );

				item.FreeCache();
			}

			SaveDiagnostics.RecordEntityBatch( "items", savedCount, savedBytes );
			SaveDiagnostics.RecordEntityProfilingCounts( deepProfiling ? savedCount : 0, deepProfiling ? 0 : savedCount );

			if ( deepProfiling )
				SaveDiagnostics.RecordTypeStats( itemTypeCollector );

			SaveDiagnostics.RecordPhase( "itemsSerialize", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			tdb.Write( ( int ) World.m_ItemTypes.Count );
			for ( int i = 0; i < World.m_ItemTypes.Count; ++i )
				tdb.Write( World.m_ItemTypes[i].FullName );

			SaveDiagnostics.RecordPhase( "itemsTypeDb", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Close();
			tdb.Close();
			bin.Close();
			SaveDiagnostics.RecordPhase( "itemsClose", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );
			SaveDiagnostics.RecordPhase( "items", SaveDiagnostics.GetElapsedMilliseconds( methodStart ) );
		}

		protected void SaveGuilds(SaveMetrics metrics)
		{
			long methodStart = SaveDiagnostics.GetTimestamp();
			long phaseStart;
			GenericWriter idx;
			GenericWriter bin;

			phaseStart = SaveDiagnostics.GetTimestamp();

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter( World.GuildIndexPath, false );
				bin = new BinaryFileWriter( World.GuildDataPath, true );
			} else {
				idx = new AsyncWriter( World.GuildIndexPath, false );
				bin = new AsyncWriter( World.GuildDataPath, true );
			}

			SaveDiagnostics.RecordPhase( "guildsOpen", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Write( ( int ) BaseGuild.List.Count );
			int savedCount = 0;
			long savedBytes = 0;
			bool deepProfiling = SaveDiagnostics.IsDeepProfilingActive;
			SaveDiagnostics.TypeCollector guildTypeCollector = deepProfiling ? new SaveDiagnostics.TypeCollector( "guilds" ) : null;

			foreach ( BaseGuild guild in BaseGuild.List.Values ) {
				long start = bin.Position;

				idx.Write( ( int ) 0 );//guilds have no typeid
				idx.Write( ( int ) guild.Id );
				idx.Write( ( long ) start );

				if ( deepProfiling )
				{
					long serializeStart = SaveDiagnostics.GetTimestamp();
					guild.Serialize( bin );
					guildTypeCollector.Record( guild.GetType(), guild.Name, guild.Id, SaveDiagnostics.GetElapsedMilliseconds( serializeStart ) );
				}
				else
				{
					guild.Serialize( bin );
				}

				int length = ( int ) ( bin.Position - start );
				++savedCount;
				savedBytes += length;

				if ( metrics != null ) {
					metrics.OnGuildSaved( length );
				}

				idx.Write( length );
			}

			SaveDiagnostics.RecordEntityBatch( "guilds", savedCount, savedBytes );
			SaveDiagnostics.RecordEntityProfilingCounts( deepProfiling ? savedCount : 0, deepProfiling ? 0 : savedCount );

			if ( deepProfiling )
				SaveDiagnostics.RecordTypeStats( guildTypeCollector );

			SaveDiagnostics.RecordPhase( "guildsSerialize", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );

			phaseStart = SaveDiagnostics.GetTimestamp();
			idx.Close();
			bin.Close();
			SaveDiagnostics.RecordPhase( "guildsClose", SaveDiagnostics.GetElapsedMilliseconds( phaseStart ) );
			SaveDiagnostics.RecordPhase( "guilds", SaveDiagnostics.GetElapsedMilliseconds( methodStart ) );
		}

		public override void ProcessDecay() {
			while ( _decayQueue.Count > 0 ) {
				Item item = _decayQueue.Dequeue();

				if ( item.OnDecay() ) {
					item.Delete();
				}
			}
		}
	}
}
