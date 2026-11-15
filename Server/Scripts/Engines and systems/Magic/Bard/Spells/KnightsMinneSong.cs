using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Spells;
using Server.Misc;
using Server.OneTime.Events;

namespace Server.Spells.Song
{
	public class KnightsMinneSong : ModifierSong
	{
		protected override BuffIcon BuffIconId { get { return BuffIcon.MassSleep; } }
		protected override int BuffTitleCliloc { get { return 1151418; } }
		protected override int BuffDescriptionCliloc { get { return 1151419; } }
		protected override string GetBuffArgs(SongEffect songEffect) { return songEffect.Strength.ToString(); }
		private static SpellInfo m_Info = new SpellInfo(
				"Knight's Minne", "*plays a knight's minne*",
				//SpellCircle.First,
				//212,9041
				-1
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 5 ); } }
		public override double RequiredSkill{ get{ return 50.0; } }
		public override int RequiredMana{ get{ return 20; } }
		public KnightsMinneSong( Mobile caster, Item scroll) : base( caster, scroll, m_Info )
		{
			ModType	= 1;
			ExpiryMessage = "The effect of the knight's minne wears off.";
		}

        public override void OnCast()
        {
			Spellbook book = Spellbook.Find(Caster, -1, SpellbookType.Song);		
			if (book != null) {
				SongBook = (SongBook)book;
				bool sings = false;
	        	base.GetElligbleBeneficials();
	        	if (this.CanCast()) {
	        		sings = true;
                                        Duration = 300; // 5 minutes
                                        int modAmount = MyServerSettings.PlayerLevelMod( (int)(MusicSkill( Caster ) / 25), Caster );

					if (Caster is PlayerMobile && ((PlayerMobile)Caster).Troubadour())
					{
						modAmount = MyServerSettings.PlayerLevelMod( (int)(MusicSkill( Caster ) / 15), Caster );
						Duration *= (int)((double)Duration * 1.3);
					}

                                        for ( int i = 0; i < Targets.Count; ++i )
                                        {
                                                Mobile mobile = (Mobile)Targets[i];
                                                int candidateAmount = GetResistanceCandidateAmount(mobile, ResistanceType.Physical, modAmount);
                                                if (!PrepareBeneficialTarget(mobile, candidateAmount, Duration))
                                                        continue;
                                                if (candidateAmount <= 0)
                                                        continue;

						mobile.SendMessage( "Your resistance to physical attacks has increased." );
						mobile.FixedParticles( 0x373A, 10, 15, 5012, 0x450, 3, EffectLayer.Waist );
                                                SongEffect songEffect = new SongEffect(mobile.Serial, this, candidateAmount, Duration);
                                                ResistanceMod mod = CreateResistanceMod( ResistanceType.Physical, candidateAmount );
                                                songEffect.ResistanceMod = mod;
                                                RegisterResistanceMod(mobile, mod);
                                                base.AddSongEffect(songEffect);
					}
					SubscribeSecondTimer();
	        	}
	        	BardFunctions.UseBardInstrument( SongBook.Instrument, sings, Caster );
				FinishSequence();
			}
		}
	}
}
