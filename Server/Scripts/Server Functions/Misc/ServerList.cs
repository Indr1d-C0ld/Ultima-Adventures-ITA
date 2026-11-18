using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Server;
using Server.Network;

namespace Server.Misc
{
	public class ServerList
	{
		/* 
		 * The default setting for Address, a value of 'null', will use your local IP address. If all of your local IP addresses
		 * are private network addresses and AutoDetect is 'true' then RunUO will attempt to discover your public IP address
		 * for you automatically.
		 *
		 * If you do not plan on allowing clients outside of your LAN to connect, you can set AutoDetect to 'false' and leave
		 * Address set to 'null'.
		 * 
		 * If your public IP address cannot be determined, you must change the value of Address to your public IP address
		 * manually to allow clients outside of your LAN to connect to your server. Address can be either an IP address or
		 * a hostname that will be resolved when RunUO starts.
		 * 
		 * If you want players outside your LAN to be able to connect to your server and you are behind a router, you must also
		 * forward TCP port 2593 to your private IP address. The procedure for doing this varies by manufacturer but generally
		 * involves configuration of the router through your web browser.
		 *
		 * ServerList will direct connecting clients depending on both the address they are connecting from and the address and
		 * port they are connecting to. If it is determined that both ends of a connection are private IP addresses, ServerList
		 * will direct the client to the local private IP address. If a client is connecting to a local public IP address, they
		 * will be directed to whichever address and port they initially connected to. This allows multihomed servers to function
		 * properly and fully supports listening on multiple ports. If a client with a public IP address is connecting to a
		 * locally private address, the server will direct the client to either the AutoDetected IP address or the manually entered
		 * IP address or hostname, whichever is applicable. Loopback clients will be directed to loopback.
		 * 
		 * If you would like to listen on additional ports (i.e. 22, 23, 80, for clients behind highly restrictive egress
		 * firewalls) or specific IP adddresses you can do so by modifying the file SocketOptions.cs found in this directory.
		 */

		// Indirizzo pubblico (IP o nome DNS/DynDNS) in MyServerSettings.ServerPublicAddress().
		// Un nome dinamico viene risolto di nuovo ogni 10 minuti, così un cambio di IP del router non richiede il riavvio.
		public static readonly string Address = MyServerSettings.ServerPublicAddress();
		private static DateTime m_LastResolve = DateTime.MinValue;
		public static readonly string ServerName = MyServerSettings.ServerName();

		public static readonly bool AutoDetect = true;

		public static void Initialize()
		{
			if ( Address == null ) {
				if ( AutoDetect )
					AutoDetection();
			}
			else {
				Resolve( Address, out m_PublicAddress );
				m_LastResolve = DateTime.UtcNow;
				Console.WriteLine( "ServerList: indirizzo pubblico {0} -> {1}", Address, m_PublicAddress != null ? m_PublicAddress.ToString() : "non risolto" );
			}

			EventSink.ServerList += new ServerListEventHandler( EventSink_ServerList );
		}

		private static IPAddress m_PublicAddress;

		private static void EventSink_ServerList( ServerListEventArgs e )
		{
			try
			{
				NetState ns = e.State;
				Socket s = ns.Socket;

				IPEndPoint ipep = (IPEndPoint)s.LocalEndPoint;

				IPAddress localAddress = ipep.Address;
				int localPort = ipep.Port;

				if ( IsPrivateNetwork( localAddress ) ) {
					ipep = (IPEndPoint)s.RemoteEndPoint;
					if ( !IsPrivateNetwork( ipep.Address ) && !IPAddress.IsLoopback( ipep.Address ) )
					{
						if ( Address != null && DateTime.UtcNow - m_LastResolve > TimeSpan.FromMinutes( 10 ) )
						{
							IPAddress fresh;
							Resolve( Address, out fresh );
							if ( fresh != null )
								m_PublicAddress = fresh;
							m_LastResolve = DateTime.UtcNow;
						}

						if ( m_PublicAddress != null )
							localAddress = m_PublicAddress;
					}
				}

				e.AddServer( ServerName, new IPEndPoint( localAddress, localPort ) );
			}
			catch
			{
				e.Rejected = true;
			}
		}

		private static void AutoDetection()
		{
			if ( !HasPublicIPAddress() ) {
				Console.Write( "ServerList: Auto-detecting public IP address..." );
				m_PublicAddress = FindPublicAddress();

				if ( m_PublicAddress != null )
					Console.WriteLine( "done ({0})", m_PublicAddress.ToString() );
				else
					Console.WriteLine( "failed" );
			}
		}

		private static void Resolve( string addr, out IPAddress outValue )
		{
			if ( IPAddress.TryParse( addr, out outValue ) )
				return;

			try {
				IPHostEntry iphe = Dns.GetHostEntry( addr );

				// preferisci un indirizzo IPv4 (il client UO non usa IPv6)
				foreach ( IPAddress a in iphe.AddressList )
					if ( a.AddressFamily == AddressFamily.InterNetwork ) { outValue = a; return; }

				if ( iphe.AddressList.Length > 0 )
					outValue = iphe.AddressList[iphe.AddressList.Length - 1];
			}
			catch {
			}
		}

		private static bool HasPublicIPAddress()
		{
			NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

			foreach ( NetworkInterface adapter in adapters ) {
				IPInterfaceProperties properties = adapter.GetIPProperties();

				foreach ( IPAddressInformation unicast in properties.UnicastAddresses ) {
					IPAddress ip = unicast.Address;

					if ( !IPAddress.IsLoopback( ip ) && ip.AddressFamily != AddressFamily.InterNetworkV6 && !IsPrivateNetwork( ip ) )
						return true;
				}
			}

			return false;
		}

		private static bool IsPrivateNetwork( IPAddress ip )
		{
			if ( ip.AddressFamily == AddressFamily.InterNetworkV6 )
				return false;

			if ( Utility.IPMatch( "192.168.*", ip ) )
				return true;
			else if ( Utility.IPMatch( "10.*", ip ) )
				return true;
			else if ( Utility.IPMatch( "127.0.0.1", ip ) )
				return true;
			else if ( Utility.IPMatch( "169.254.*", ip ) )
				return true;
			else if ( Utility.IPMatch( "172.16-31.*", ip ) )
				return true;
			else
				return false;
		}

		private static IPAddress FindPublicAddress()
		{
			try {
				WebRequest req = HttpWebRequest.Create( "http://icanhazip.com" );
				req.Timeout = 15000;

				WebResponse res = req.GetResponse();

				Stream s = res.GetResponseStream();

				StreamReader sr = new StreamReader( s );

				IPAddress ip = IPAddress.Parse( sr.ReadLine() );

				sr.Close();
				s.Close();
				res.Close();

				return ip;

			} catch {
				return null;
			}
		}
	}
}