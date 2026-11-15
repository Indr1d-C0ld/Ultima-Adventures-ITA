using System;
using Server.Items;
using Server.Network;
using System.Collections.Generic;
using System.Collections;
using Server.Mobiles;
using Server.Misc;
using Server.Diagnostics;

namespace Server.Items
{
	public class TaskManager250Min : Item
	{

		private static DateTime lastrun;

		public static DateTime LastRun { get { return lastrun; } }

		public static DateTime NextRunTime
		{
			get { return lastrun + TimeSpan.FromMinutes( 250.0 ); }
		}
		
		[Constructable]
		public TaskManager250Min () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager 250 Minutes";
			Visible = false;
			lastrun = DateTime.UtcNow;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManager250Min(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
            writer.Write( (int) 1 ); // version
			writer.Write( (DateTime)lastrun );

		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
            int version = reader.ReadInt();
			if (version == 1)
				lastrun = reader.ReadDateTime();

			if (lastrun == null)
				lastrun = DateTime.UtcNow;

			if ( Server.Misc.MyServerSettings.RunRoutinesAtStartup() )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}


		}
		public class TaskTimer : Timer 
		{ 
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 250.0 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( false, "TaskManager250Min+TaskTimer", false );
			} 
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 5.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( true, "TaskManager250Min+FirstTimer", true );
			} 
		}

		public static void RunThis( bool DoAction, string timerName, bool startup )
		{
			RunThis( DoAction, timerName, startup, false );
		}

		public static void RunThis( bool DoAction, string timerName, bool startup, bool forceRun )
		{
			TaskManagerDiagnostics.Context diag = TaskManagerDiagnostics.BeginRun( "TaskManager250Min", timerName, startup );

			try
			{
				if ( forceRun || DateTime.UtcNow > ( lastrun + TimeSpan.FromMinutes( 250 )) )
				{
					
				Console.WriteLine( "Begin 250 min tasks" );
				///// ADD RANDOM CITIZENS IN SETTLEMENTS /////////////////////
				//World.Broadcast( 0x35, true, "Begin 250 min tasks" );

	//balancelevel increase

				Console.WriteLine( "Randomizing DungeonDoors" );

				ArrayList dungeondoors = new ArrayList();
				ArrayList spawns = new ArrayList();
				
				TaskManagerDiagnostics.StartPhase( diag, "itemScanDoorsSpawns" );
				TaskManagerDiagnostics.MarkRegistryCandidate( diag, "250-minute doors/spawners/coffers full item scan" );
				int firstItemScanCount = 0;
				foreach( Item item in World.Items.Values )
					{
						++firstItemScanCount;

						if ( item is BaseDoor )
						{
								dungeondoors.Add( item );
						}
						
						if ( item is PremiumSpawner )
						{
							PremiumSpawner spawner = (PremiumSpawner)item;

							if ( spawner.SpawnID == 8888 )
							{
								bool reconfigure = true;

								foreach ( NetState state in NetState.Instances )
								{
									Mobile m = state.Mobile;

									if ( m is PlayerMobile && m.InRange( spawner.Location, (spawner.HomeRange+20) ) )
									{
										reconfigure = false;
									}
								}

								if ( reconfigure ){ spawns.Add( item ); }
							}
						}
						else if ( item is Coffer )
						{
							Coffer coffer = (Coffer)item;
							Server.Items.Coffer.SetupCoffer( coffer );
						}
						else if ( item is HayCrate || item is HollowStump )
						{	
							if (item is HayCrate &&  ( ((HayCrate)item).HayTown == null || ((HayCrate)item).HayTown == "") )
								Server.Items.HayCrate.GetNearbyTown( (HayCrate)item );
							if (item is HollowStump && ( ((HollowStump)item).StumpTown == null || ((HollowStump)item).StumpTown == "" ))
								Server.Items.HollowStump.GetNearbyTown( (HollowStump)item );
							
							item.Stackable = false;
						}
					}
				TaskManagerDiagnostics.AddScanned( diag, firstItemScanCount );

				TaskManagerDiagnostics.StartPhase( diag, "randomizeDoors" );
				TaskManagerDiagnostics.MarkChunkCandidate( diag, "door randomization can be chunked after door registry" );
				foreach( BaseDoor door in dungeondoors )
				{
					if (  Server.Items.DoorType.IsDungeonDoor( door ) && !Server.Items.DoorType.IsSpaceshipDoor( door ) )
					{

						int difficulty = Server.Misc.MyServerSettings.GetDifficultyLevel( door.Location, door.Map );
		
						if (difficulty <2)
						{
							if (Utility.RandomDouble() < 0.90) //80% of all doors are open
								door.Locked = false;
							else if (Utility.RandomDouble() < 0.80) // 65% of the 20% of doors that are locked are easier
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(10, 75);
							}
							else // 35% of the 20% of doors that are locked are harder
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(65, 95);
							}
						}
						else if (difficulty <4)
						{
							if (Utility.RandomDouble() < 0.75) //80% of all doors are open
								door.Locked = false;
							else if (Utility.RandomDouble() < 0.65) // 65% of the 20% of doors that are locked are easier
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(10, 75);
							}
							else // 35% of the 20% of doors that are locked are harder
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(65, 95);
							}
						}
						else if (difficulty >3)
						{
							if (Utility.RandomDouble() < 0.55) //80% of all doors are open
								door.Locked = false;
							else if (Utility.RandomDouble() < 0.50) // 65% of the 20% of doors that are locked are easier
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(10, 75);
							}
							else // 35% of the 20% of doors that are locked are harder
							{
								door.Locked = true;
								door.KeyValue = (uint)Utility.RandomMinMax(65, 95);
							}
						}
					}		
				}
			

				TaskManagerDiagnostics.StartPhase( diag, "reconfigureSpawns" );
				TaskManagerDiagnostics.MarkChunkCandidate( diag, "premium spawner reconfigure pass can be staggered" );
				for ( int i = 0; i < spawns.Count; ++i )
				{
					PremiumSpawner spawners = ( PremiumSpawner )spawns[ i ];
					Server.Mobiles.PremiumSpawner.Reconfigure( spawners, DoAction );
				}
				TaskManagerDiagnostics.AddModified( diag, dungeondoors.Count + spawns.Count );

				TaskManagerDiagnostics.StartPhase( diag, "checkInfection" );
				AdventuresFunctions.CheckInfection();

				// Randomize Midlands dest
				Point3D[] m_MidlandLocations = new Point3D[]
				{
						new Point3D( 2759, 983, 1 ), // Central Town
						new Point3D( 3464, 1427, 0 ), // Broken Fortress Town
						new Point3D( 2730, 1814, 1 ), // Central Bridge
						new Point3D( 2663, 3850, 0 ), // South Eastern Road
						new Point3D( 739, 2730, 5 ), // Western Oasis
				};
				Point3D m_RandLocation = m_MidlandLocations[new Random().Next(m_MidlandLocations.Length)];
				bool tpFound = false;
				TaskManagerDiagnostics.StartPhase( diag, "midlandTeleportScan" );
				TaskManagerDiagnostics.MarkRegistryCandidate( diag, "Midland teleporter nested full item scan" );
				int midlandScanCount = 0;
				foreach (Item item in World.Items.Values)
				{
					++midlandScanCount;

					if (item is Teleporter && item.Name == "MidlandExitMoongate")
					{
						foreach (Item portal in World.Items.Values)
						{
							++midlandScanCount;

							if (portal != item && portal.X == item.X && portal.Y == item.Y && portal.Map == Map.Midland)
							{
								tpFound = true;
								portal.X = m_RandLocation.X;
								portal.Y = m_RandLocation.Y;
								portal.Z = m_RandLocation.Z;
								break;
							}
						}
						if (tpFound)
						{
							item.X = m_RandLocation.X;
							item.Y = m_RandLocation.Y;
							item.Z = m_RandLocation.Z;
						}
						break;
					}
				}
				TaskManagerDiagnostics.AddScanned( diag, midlandScanCount );
				LoggingFunctions.LogServer("Done - Randomizing Midland Exit Moongate");

				Console.WriteLine( "End 250 minute tasks" );
				World.Broadcast( 0x35, true, "Dungeon doors have been randomized!" );
				lastrun = DateTime.UtcNow;
				}
				else
				{
					TaskManagerDiagnostics.StartPhase( diag, "notDue" );
				}
			
				if (AetherGlobe.invasionstage == 2 && Utility.RandomBool() )
				{
					TaskManagerDiagnostics.StartPhase( diag, "invasionRoutine" );
					AdventuresFunctions.InvasionRoutine();
				}

				TaskManagerDiagnostics.StartPhase( diag, "vendorCurse" );
				AetherGlobe.VendorCurse = AetherGlobe.BalanceLevel;
			}
			finally
			{
				TaskManagerDiagnostics.EndRun( diag );
			}
			
		}
	}
}
