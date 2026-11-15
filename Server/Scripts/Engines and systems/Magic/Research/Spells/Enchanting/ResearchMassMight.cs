using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Spells;
using Server.Misc;

namespace Server.Spells.Research
{
	public class ResearchMassMight : ResearchSpell
	{
		public override int spellIndex { get { return 11; } }
		public int CirclePower = 2;
		public static int spellID = 11;
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.75 ); } }
		public override double RequiredSkill{ get{ return (double)(Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ))); } }
		public override int RequiredMana{ get{ return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 )); } }

		private static SpellInfo m_Info = new SpellInfo(
				Server.Misc.Research.SpellInformation( spellID, 2 ),
				Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ),
				236,
				9011
			);
		
		public ResearchMassMight( Mobile caster, Item scroll) : base( caster, scroll, m_Info )
		{
		}

        public override void OnCast()
        {
			if( CheckSequence() )
			{
				bool success = false;
 
				ArrayList targets = new ArrayList();

				foreach ( Mobile m in Caster.GetMobilesInRange( 10 ) )
				{
					if ( Caster.CanBeBeneficial( m, false, true ) && !(m is Golem) )
						targets.Add( m );
				}

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];
					
					int amount = (int)(DamagingSkill( Caster ) / 16);
					double duration = (double)(DamagingSkill( Caster ) * 2);
					int appliedBonus;
					TimeSpan appliedDuration;

					SpellHelper.AddStatBonus( Caster, m, StatType.Str, amount, TimeSpan.FromSeconds( duration ), out appliedBonus, out appliedDuration );

					BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.DespairCaster, 1151430, 1151431, appliedDuration, m, appliedBonus.ToString() ) );
						
					m.FixedParticles( 0x375A, 10, 15, 5017, 0x224, 3, EffectLayer.Waist );
					success = true;
				}

				if ( success ){ Server.Misc.Research.ConsumeScroll( Caster, true, spellIndex, false ); }
			}
			FinishSequence();
		}
	}
}
