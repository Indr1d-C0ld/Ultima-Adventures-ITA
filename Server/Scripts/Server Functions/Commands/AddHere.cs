using System;
using Server;
using Server.Items;

namespace Server.Commands
{
	public static class AddHereCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register( "AddHere", AccessLevel.GameMaster, new CommandEventHandler( OnCommand ) );
		}

		[Usage( "AddHere <mib|tmap> [level]" )]
		[Description( "Adds a deterministic message in a bottle or treasure map to your backpack." )]
		private static void OnCommand( CommandEventArgs e )
		{
			if ( e.Length < 1 || e.Length > 2 )
			{
				e.Mobile.SendMessage( "Usage: AddHere <mib|tmap> [level]" );
				return;
			}

			string kind = e.GetString( 0 );
			int level = 1;

			if ( e.Length == 2 && !Int32.TryParse( e.GetString( 1 ), out level ) )
			{
				e.Mobile.SendMessage( "The level must be a number." );
				return;
			}

			Mobile from = e.Mobile;

			if ( from.Map == null || from.Map == Map.Internal )
			{
				from.SendMessage( "You must be on a valid map to use AddHere." );
				return;
			}

			if ( Insensitive.Equals( kind, "mib" ) || Insensitive.Equals( kind, "messageinabottle" ) )
			{
				level = Math.Max( 1, Math.Min( level, 4 ) );

				MessageInABottle mib = new MessageInABottle( from.Map, level, from.Location, from.X, from.Y );
				mib.SetDeterministicLocation( from.Map, from.Location );

				from.AddToBackpack( mib );
				CommandLogging.WriteLine( from, "{0} {1} created a deterministic MIB at {2} in {3}.", from.AccessLevel, CommandLogging.Format( from ), from.Location, from.Map );
				from.SendMessage( "A deterministic message in a bottle was added to your backpack." );
			}
			else if ( Insensitive.Equals( kind, "tmap" ) || Insensitive.Equals( kind, "treasuremap" ) )
			{
				level = Math.Max( 1, Math.Min( level, 7 ) );

				TreasureMap treasureMap = new TreasureMap( level, from.Map, from.Location, from.X, from.Y );
				treasureMap.SetDeterministicLocation( from.Map, new Point2D( from.X, from.Y ) );

				from.AddToBackpack( treasureMap );
				CommandLogging.WriteLine( from, "{0} {1} created a deterministic treasure map at {2} in {3}.", from.AccessLevel, CommandLogging.Format( from ), from.Location, from.Map );
				from.SendMessage( "A deterministic treasure map was added to your backpack." );
			}
			else
			{
				from.SendMessage( "Usage: AddHere <mib|tmap> [level]" );
			}
		}
	}
}
