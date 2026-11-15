using System;
using Server;

namespace Server.Items
{
	public abstract class BaseLight : Item
	{
		private Timer m_Timer;
		private DateTime m_End;
		private bool m_BurntOut = false;
		private bool m_Burning = false;
		private bool m_Protected = false;
		private TimeSpan m_Duration = TimeSpan.Zero;
		private TimeSpan m_DefaultDuration = TimeSpan.Zero;

		public abstract int LitItemID{ get; }
		
		public virtual int UnlitItemID{ get { return 0; } }
		public virtual int BurntOutItemID{ get { return 0; } }
		
		public virtual int LitSound{ get { return 0x47; } }
		public virtual int UnlitSound{ get { return 0x3be; } }
		public virtual int BurntOutSound{ get { return 0x4b8; } }

		public static readonly bool Burnout = true;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Burning
		{
			get { return m_Burning; }
			set
			{
				if ( m_Burning != value )
				{
					m_Burning = true;
					DoTimer( m_Duration );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool BurntOut
		{
			get { return m_BurntOut; }
			set { m_BurntOut = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Protected
		{
			get { return m_Protected; }
			set { m_Protected = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan Duration
		{
			get
			{
				if ( m_Duration != TimeSpan.Zero && m_Burning )
				{
					return m_End - DateTime.UtcNow;
				}
				else
					return m_Duration;
			}

			set
			{
				m_Duration = value;

				if ( m_DefaultDuration == TimeSpan.Zero && value != TimeSpan.Zero )
					m_DefaultDuration = value;
			}
		}

		[Constructable]
		public BaseLight( int itemID ) : base( itemID )
		{
		}

		public BaseLight( Serial serial ) : base( serial )
		{
		}

		public virtual void PlayLitSound()
		{
			if ( LitSound != 0 )
			{
				Point3D loc = GetWorldLocation();
				Effects.PlaySound( loc, Map, LitSound );
			}					
		}
		
		public virtual void PlayUnlitSound()
		{
			int sound = UnlitSound;

			if ( m_BurntOut && BurntOutSound != 0 )
				sound = BurntOutSound;

			
			if ( sound != 0 )
			{
				Point3D loc = GetWorldLocation();
				Effects.PlaySound( loc, Map, sound );
			}					
		}

		public virtual void Ignite()
		{
			if ( !m_BurntOut )
			{
				PlayLitSound();

				m_Burning = true;
				ItemID = LitItemID;

				if ( m_Duration != TimeSpan.Zero && Movable )
					DoTimer( m_Duration );
			}
		}

		public virtual void Douse()
		{
			m_Burning = false;
			
			if ( m_BurntOut && BurntOutItemID != 0 )
				ItemID = BurntOutItemID;
			else
				ItemID = UnlitItemID;

			if ( m_BurntOut )
				m_Duration = TimeSpan.Zero;
			else if ( m_Duration != TimeSpan.Zero && m_Timer != null )
				m_Duration = m_End - DateTime.UtcNow;

			if ( m_Timer != null )
				m_Timer.Stop();

			PlayUnlitSound();
		}

		public virtual void Burn()
		{
			m_BurntOut = true;
			Douse();
		}

		private void DoTimer( TimeSpan delay )
		{
			m_Duration = delay;
			
			if ( m_Timer != null )
				m_Timer.Stop();

			if ( delay == TimeSpan.Zero )
				return;

			m_End = DateTime.UtcNow + delay;

			m_Timer = new InternalTimer( this, delay );
			m_Timer.Start();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( m_BurntOut )
			{
				if ( TryRefillWithOil( from ) )
					return;

				from.SendMessage( "That item is burnt out and needs oil." );
				return;
			}

			if ( m_Duration != TimeSpan.Zero )
			{
				TimeSpan remaining = Duration;

				if ( remaining < TimeSpan.Zero )
					remaining = TimeSpan.Zero;

				from.SendMessage( "Oil remaining: {0:%d}d {0:%h}h {0:%m}m.", remaining );
			}

			if ( m_Protected && from.AccessLevel == AccessLevel.Player )
				return;

			if ( !from.InRange( this.GetWorldLocation(), 2 ) )
				return;

			if ( m_Burning )
			{
				if ( UnlitItemID != 0 )
					Douse();
			}
			else
			{
				Ignite();
			}
		}

		private bool TryRefillWithOil( Mobile from )
		{
			if ( from == null || from.Backpack == null )
				return false;

			if ( !from.Backpack.ConsumeTotal( typeof( LampOil ), 1 ) )
				return false;

			m_BurntOut = false;
			m_Burning = false;

			if ( m_Timer != null )
			{
				m_Timer.Stop();
				m_Timer = null;
			}

			TimeSpan refillDuration = TimeSpan.FromDays( 30 );

			m_Duration = refillDuration;

			if ( UnlitItemID != 0 )
				ItemID = UnlitItemID;

			from.SendMessage( "You refill the lamp with oil." );
			return true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 );
			writer.Write( m_BurntOut );
			writer.Write( m_Burning );
			writer.Write( m_Duration );
			writer.Write( m_Protected );
			writer.Write( m_DefaultDuration );

			if ( m_Burning && m_Duration != TimeSpan.Zero )
				writer.WriteDeltaTime( m_End );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_BurntOut = reader.ReadBool();
					m_Burning = reader.ReadBool();
					m_Duration = reader.ReadTimeSpan();
					m_Protected = reader.ReadBool();
					m_DefaultDuration = reader.ReadTimeSpan();

					if ( m_Burning && m_Duration != TimeSpan.Zero )
						DoTimer( reader.ReadDeltaTime() - DateTime.UtcNow );

					break;
				}
				case 0:
				{
					m_BurntOut = reader.ReadBool();
					m_Burning = reader.ReadBool();
					m_Duration = reader.ReadTimeSpan();
					m_Protected = reader.ReadBool();
					m_DefaultDuration = m_Duration;

					if ( m_Burning && m_Duration != TimeSpan.Zero )
						DoTimer( reader.ReadDeltaTime() - DateTime.UtcNow );

					break;
				}
			}
		}

		private class InternalTimer : Timer
		{
			private BaseLight m_Light;

			public InternalTimer( BaseLight light, TimeSpan delay ) : base( delay )
			{
				m_Light = light;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				if ( m_Light != null && !m_Light.Deleted )
					m_Light.Burn();
			}
		}
	}
}
