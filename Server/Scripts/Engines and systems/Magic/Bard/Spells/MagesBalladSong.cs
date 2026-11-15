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
using Server.OneTime.Events;

namespace Server.Spells.Song
{
    public class MagesBalladSong : ModifierSong
    {
        protected override BuffIcon BuffIconId { get { return BuffIcon.Invigorate; } }
        protected override int BuffTitleCliloc { get { return 1151402; } }
        protected override int BuffDescriptionCliloc { get { return 1151403; } }
        protected override string GetBuffArgs(SongEffect songEffect) { return songEffect.Strength.ToString(); }
        private static SpellInfo m_Info = new SpellInfo(
            "Mage's Ballad", "*plays a mage's ballad*",
            //SpellCircle.First,
            //212,9041
            -1);

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(6); } }
        public override double RequiredSkill { get { return 55; } }
        public override int RequiredMana { get { return 100; } }

        public MagesBalladSong(Mobile caster, Item scroll): base(caster, scroll, m_Info){
            ModType = 6;
            ExpiryMessage = "The effect of the mages's ballad wears off";
            ModLevel = 6;
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
                    Duration = (int)( Caster.Skills[SkillName.Musicianship].Value * .16 );
                    this.DetermineModLevelBySkill(MusicSkill( Caster ));
                    for ( int i = 0; i < Targets.Count; ++i )
                    {
                        Mobile mobile = (Mobile)Targets[i];
                        int candidateStrength = MyServerSettings.PlayerLevelMod(ModLevel, mobile);
                        if (!PrepareBeneficialTarget(mobile, candidateStrength, Duration))
                            continue;
                        mobile.SendMessage( "You feel a soothing peace come upon you." );   
                        mobile.FixedParticles(0x376A, 9, 32, 5030, 0x256, 3, EffectLayer.Waist);
                        mobile.PlaySound(0x1F2);
                        SongEffect songEffect = new SongEffect(mobile.Serial, this, candidateStrength, Duration);
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
