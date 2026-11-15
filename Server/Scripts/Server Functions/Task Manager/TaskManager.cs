using System;
using Server.Items;
using Server.Network;
using System.Collections.Generic;
using System.Collections;
using Server.Mobiles;
using Server.Misc;
using Server.SkillHandlers;
using Server.Diagnostics;

namespace Server.Items
{
	public class TaskManager : Item
	{

		private static DateTime m_LastRun = DateTime.UtcNow;

		public static DateTime LastRun { get { return m_LastRun; } }

		public static DateTime NextRunTime
		{
			get { return m_LastRun + TimeSpan.FromHours( 1.0 ); }
		}

		[Constructable]
		public TaskManager () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager 1 Hour";
			Visible = false;
			m_LastRun = DateTime.UtcNow;
			TaskTimer thisTimer = new TaskTimer( this );
			thisTimer.Start();
		}

		public TaskManager( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			writer.Write( (DateTime)m_LastRun );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version > 0 )
			{
				m_LastRun = reader.ReadDateTime();
			}
			else
			{
				m_LastRun = DateTime.UtcNow;
			}

			if ( Server.Misc.MyServerSettings.RunRoutinesAtStartup() )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}

			ColorTimer colorTimer = new ColorTimer( this ); 
			colorTimer.Start();
		}

		public class TaskTimer : Timer 
		{ 
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 60.0 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( "TaskManager+TaskTimer", false );
			} 
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 1.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( "TaskManager+FirstTimer", true );
			} 
		}

		public class ColorTimer : Timer 
		{ 
			private Item i_item; 
			public ColorTimer( Item task ) : base( TimeSpan.FromSeconds( 30.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				Server.Misc.ServerUpdate.UpdateMaterialColors();
			} 
		}

		public static void RunThis( string timerName, bool startup )
		{
			TaskManagerDiagnostics.Context diag = TaskManagerDiagnostics.BeginRun( "TaskManager", timerName, startup );

			try
			{
				m_LastRun = DateTime.UtcNow;

				Console.WriteLine( "Beginning Hourly Tasks" );
				LoggingFunctions.LogServer( "Start - Moving Vendors Back" );

				// SWITCH UP THE MAGIC MIRRORS
				TaskManagerDiagnostics.StartPhase( diag, "magicMirrors" );
				Server.Items.MagicMirror.SetMirrors( diag );

				// MOVE SHOPKEEPERS AND GUARDS TO WHERE THEY BELONG...IN CASE THEY MOVED FAR AWAY
				TaskManagerDiagnostics.StartPhase( diag, "mobileScan" );
				TaskManagerDiagnostics.MarkRegistryCandidate( diag, "vendor/citizen full mobile scan" );
				ArrayList vendors = new ArrayList();
				ArrayList citizens = new ArrayList();
				int mobilesScanned = 0;
				foreach ( Mobile vendor in World.Mobiles.Values )
				{
					++mobilesScanned;

					if ( vendor is BaseVendor && vendor.WhisperHue != 999 && !(vendor is PlayerVendor || vendor is PlayerBarkeeper || vendor is RoomAttendant) )
					{
						vendors.Add( vendor );
					}
					else if ( vendor is BlueGuard || vendor is MercenaryGuard )
					{
						vendors.Add( vendor );
					}
					else if ( vendor is Citizens && vendor.Fame > 0 )
					{
						citizens.Add( vendor );
					}
				}
				TaskManagerDiagnostics.AddScanned( diag, mobilesScanned );

				TaskManagerDiagnostics.StartPhase( diag, "moveVendors" );
				for ( int i = 0; i < vendors.Count; ++i )
				{
					Mobile vendor = ( Mobile )vendors[ i ];
					BaseCreature vendur = ( BaseCreature )vendors[ i ];
					vendor.Location = vendur.Home;
				}
				TaskManagerDiagnostics.AddModified( diag, vendors.Count );

				TaskManagerDiagnostics.StartPhase( diag, "resetCitizens" );
				for ( int i = 0; i < citizens.Count; ++i )
				{
					Mobile citizen = ( Mobile )citizens[ i ];
					citizen.Fame = 0;
				}
				TaskManagerDiagnostics.AddModified( diag, citizens.Count );


				LoggingFunctions.LogServer( "Done - Moving Vendors Back" );

				TaskManagerDiagnostics.StartPhase( diag, "wipeStealing" );
				Stealing.WipeStealingList();

				TaskManagerDiagnostics.StartPhase( diag, "wipeSnooping" );
				Snooping.WipeSnoopingList();

				Console.WriteLine( "Done Hourly Tasks" );
			}
			finally
			{
				TaskManagerDiagnostics.EndRun( diag );
			}
		}
	}
}
