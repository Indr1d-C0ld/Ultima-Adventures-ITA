using System;
using System.Collections;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Herbalist 
{ 
	public class BlendWithForestSpell : HerbalistSpell 
	{ 
		private static SpellInfo m_Info = new SpellInfo( "", "", 239, 9021 );
		private static Hashtable m_Table = new Hashtable();
		public override int HerbalistSpellCircle{ get{ return 4; } }
		public override double CastDelay{ get{ return 1.0; } } 
		public override double RequiredSkill{ get{ return 50.0; } } 
		public override int RequiredMana{ get{ return 0; } } 
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.0 ); } }
		public BlendWithForestSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				InternalItem oldItem = (InternalItem)m_Table[m];
				bool originalSquelched = ( oldItem != null ? oldItem.OriginalSquelched : m.Squelched );

				if ( oldItem != null )
					oldItem.StopAndDelete();

				m.PlaySound( 0x19 );
				m.Paralyze( TimeSpan.FromSeconds( 20.0 ) );
				m.FixedParticles( 0x375A, 2, 10, 5027, 0x3D, 2, EffectLayer.Waist );

				Point3D loc = new Point3D( m.X, m.Y, m.Z );
				InternalItem item = new InternalItem( loc, Caster.Map, Caster, m, originalSquelched );

				if ( !item.Deleted )
				{
					m_Table[m] = item;
					item.StartTimer();
					m.Hidden = true;
					m.Squelched = true;
					BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.AuraOfNausea, 1151462, 1151463, TimeSpan.FromSeconds( 20.0 ), m ) );
				}

				foreach ( Mobile pet in World.Mobiles.Values )
				{
					if ( pet is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)pet;
						if ( bc.Controlled && bc.ControlMaster == m )
							pet.Hidden = true;
					}
				}
			}

			FinishSequence();
		}

		private class InternalItem : Item
		{
			private Timer m_Timer;
			private DateTime m_End;
			private Mobile m_Owner;
			private bool m_OriginalSquelched;

			public bool OriginalSquelched { get { return m_OriginalSquelched; } }

			public InternalItem( Point3D loc, Map map, Mobile caster, Mobile m, bool talk ) : base( 0xC9E )
			{
				Visible = false;
				Movable = false;
				m_Owner = m;
				m_OriginalSquelched = talk;

				MoveToWorld( loc, map );

				if ( caster.InLOS( this ) )
					Visible = true;
				else
					Delete();

				if ( Deleted )
					return;

				m_End = DateTime.UtcNow + TimeSpan.FromSeconds( 30.0 );
			}

			public void StartTimer()
			{
				if ( Deleted )
					return;

				m_Timer = new InternalTimer( this, TimeSpan.FromSeconds( 20.0 ) );
				m_Timer.Start();
			}

			public void StopAndDelete()
			{
				if ( m_Timer != null )
				{
					m_Timer.Stop();
					m_Timer = null;
				}

				if ( m_Owner != null && m_Table[m_Owner] == this )
				{
					m_Table.Remove( m_Owner );
					BuffInfo.RemoveBuff( m_Owner, BuffIcon.AuraOfNausea );
				}

				Delete();
			}

			public void Expire()
			{
				if ( m_Owner == null || m_Table[m_Owner] != this )
					return;

				m_Table.Remove( m_Owner );
				BuffInfo.RemoveBuff( m_Owner, BuffIcon.AuraOfNausea );
				Delete();
				m_Owner.Squelched = m_OriginalSquelched;
				m_Owner.Hidden = false;
			}

			public InternalItem( Serial serial ) : base( serial )
			{
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int) 1 ); // version

				writer.Write( m_End - DateTime.UtcNow );
				writer.Write(m_Owner);
				writer.Write(m_OriginalSquelched);
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				int version = reader.ReadInt();
				reader.ReadTimeSpan();
				m_Owner = reader.ReadMobile();
				m_OriginalSquelched = reader.ReadBool();
				if(m_Owner!=null)
				{
					m_Owner.Hidden=false;
					m_Owner.Squelched=m_OriginalSquelched;
				}
				this.Delete();
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if ( m_Timer != null )
					m_Timer.Stop();

				if ( m_Owner != null && m_Table[m_Owner] == this )
				{
					m_Table.Remove( m_Owner );
					BuffInfo.RemoveBuff( m_Owner, BuffIcon.AuraOfNausea );
					m_Owner.Squelched = m_OriginalSquelched;
					m_Owner.Hidden = false;
				}
			}

			private class InternalTimer : Timer
			{
				private InternalItem m_Item;
				public InternalTimer( InternalItem item, TimeSpan duration ) : base( duration )
				{
					m_Item = item;
				}

				protected override void OnTick()
				{
					m_Item.Expire();
				}
			}
		}

		private class InternalTarget : Target
		{
			private BlendWithForestSpell m_Owner;

			public InternalTarget( BlendWithForestSpell owner ) : base( 12, true, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
