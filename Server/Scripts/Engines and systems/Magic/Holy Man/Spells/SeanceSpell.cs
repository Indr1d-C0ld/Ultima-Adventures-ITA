using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Spells;

namespace Server.Spells.HolyMan
{
	public class SeanceSpell : HolyManSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Seance", "Spiritus Mundi",
				266,
				9040
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return 120; } }
		public override double RequiredSkill{ get{ return 60.0; } }
		public override int RequiredMana{ get{ return 30; } }
		public override bool CanCastWhileBlessed { get { return CanRefreshEffect(Caster, GetState(Caster)); } }

        private int m_NewBody;
        private int m_OldBody;
        private int m_NewHue;
        private int m_OldHue;
        private TimedBuffState m_RefreshState;

        public SeanceSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast(Mobile caster)
        {
			m_RefreshState = null;

            if (!base.CheckCast(caster))
                return false;

            TimedBuffState state = GetState(Caster);

            if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1042561); //Please dismount first.
                return false;
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendMessage("You cannot enter the realm of the dead while in that form.");
                return false;
            }
			else if ( DisguiseTimers.IsDisguised( Caster ) )
			{
                Caster.SendMessage("You cannot enter the realm of the dead while disguised.");
                return false;
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendMessage("You cannot enter the realm of the dead without removing your paint.");
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(SeanceSpell)))
            {
				if (CanRefreshEffect(Caster, state))
				{
					m_RefreshState = state;
					return true;
				}

                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else
			{
				EndEffect(Caster, state, true);
				m_NewBody = 970;
				m_NewHue = 0x9C4;
            }
            m_OldBody = Caster.Body;
            m_OldHue = Caster.Hue;
            return true;
        }

        public override void OnCast()
        {
			if (m_RefreshState != null)
			{
				TimedBuffState refreshState = m_RefreshState;
				m_RefreshState = null;

				if (CanRefreshEffect(Caster, refreshState))
				{
					if (CheckSequence())
					{
						refreshState.Refresh();
						BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.FanDancerFanFire, 1151452, 1151453, refreshState.FullDuration, Caster ) );
					}
				}

				FinishSequence();
				return;
			}

            TimedBuffState state = GetState(Caster);

            if (CanRefreshEffect(Caster, state))
            {
                if (CheckSequence())
                {
                    state.Refresh();
					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.FanDancerFanFire, 1151452, 1151453, state.FullDuration, Caster ) );
                }
            }
            else if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1042561); //Please dismount first.
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendMessage("You cannot enter the realm of the dead while in that form.");
            }
			else if ( DisguiseTimers.IsDisguised( Caster ) )
			{
                Caster.SendMessage("You cannot enter the realm of the dead while disguised.");
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendMessage("You cannot enter the realm of the dead without removing your paint.");
            }
            else if (!Caster.CanBeginAction(typeof(Server.Spells.Fifth.IncognitoSpell)) || Caster.IsBodyMod)
            {
                DoFizzle();
            }
            else if (CheckSequence())
            {
				EndEffect(Caster, state, true);

                if (Caster.BeginAction(typeof(SeanceSpell)))
                {
                    if (m_NewBody != 0)
                    {
                        if (this.Scroll != null)
                            Scroll.Consume();

                        Caster.PlaySound(0x379);
                        Caster.BodyValue = m_NewBody;
                        Caster.Hue = m_NewHue;
                        Caster.SendMessage("You enter the realm of the dead.");
						bool wasBlessed = Caster.Blessed;
                        Caster.Blessed = true;

                        state = new TimedBuffState(
						new RestorationState(Caster, m_OldBody, m_OldHue, wasBlessed),
						Caster,
						0.0,
						GetDuration(Caster),
						new TimedBuffExpireCallback(ExpireEffect));

                        m_Timers[Caster.Serial] = state;
						BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.FanDancerFanFire, 1151452, 1151453, state.FullDuration, Caster ) );
                    }
                }
                else
                {
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
            }

            FinishSequence();
        }

        private static Hashtable m_Timers = new Hashtable();

        public static bool StopTimer(Mobile m)
        {
            TimedBuffState state = GetState(m);

            if (state != null)
            {
                EndEffect(m, state, true);
            }

            return (state != null);
        }

        private static TimedBuffState GetState(Mobile mobile)
        {
            if (mobile == null)
                return null;

            return m_Timers[mobile.Serial] as TimedBuffState;
        }

        private static bool CanRefreshEffect(Mobile mobile, TimedBuffState state)
        {
            return mobile != null
				&& state != null
				&& m_Timers[mobile.Serial] == state
				&& !mobile.Deleted
				&& mobile.Blessed
				&& !mobile.Mounted
				&& !TransformationSpellHelper.UnderTransformation(mobile)
				&& !DisguiseTimers.IsDisguised(mobile)
				&& mobile.BodyMod != 183
				&& mobile.BodyMod != 184
				&& !mobile.IsBodyMod
				&& mobile.CanBeginAction(typeof(Server.Spells.Fifth.IncognitoSpell))
				&& !mobile.CanBeginAction(typeof(SeanceSpell))
				&& mobile.Body == 970
				&& mobile.Hue == 0x9C4;
        }

        private static bool EndEffect(Mobile mobile, TimedBuffState state, bool stopTimer)
        {
            if (mobile == null || state == null || m_Timers[mobile.Serial] != state)
                return false;

            RestorationState restoration = state.Owner as RestorationState;

            if (stopTimer)
                state.Stop();

            m_Timers.Remove(mobile.Serial);
			BuffInfo.RemoveBuff( mobile, BuffIcon.FanDancerFanFire );

            if (restoration != null && !mobile.Deleted)
            {
				RestoreBody(mobile, restoration.OldBody);

				if (mobile.Hue == 0x9C4)
					mobile.Hue = restoration.OldHue;

				mobile.Blessed = restoration.WasBlessed;
			}

            mobile.EndAction(typeof(SeanceSpell));
			return true;
        }

		private static void RestoreBody(Mobile mobile, int oldBody)
		{
			Body bodyMod = mobile.BodyMod;

			if (mobile.IsBodyMod)
			{
				mobile.BodyMod = 0;

				if (mobile.BodyValue == 970)
					mobile.BodyValue = oldBody;

				mobile.BodyMod = bodyMod;
			}
			else if (mobile.BodyValue == 970)
			{
				mobile.BodyValue = oldBody;
			}
		}

        private static TimeSpan GetDuration(Mobile mobile)
        {
            int val = (int)(mobile.Skills[SkillName.Healing].Value + (mobile.Skills[SkillName.SpiritSpeak].Value / 2));

            if (val > 100)
                val = 100;

            return TimeSpan.FromSeconds(val);
        }

        private static void ExpireEffect(TimedBuffState state)
        {
            RestorationState restoration = state.Owner as RestorationState;

            if (restoration != null)
				EndEffect(restoration.Mobile, state, false);
        }

        private sealed class RestorationState
        {
            private Mobile m_Mobile;
            private int m_OldBody;
            private int m_OldHue;
            private bool m_WasBlessed;

            public Mobile Mobile { get { return m_Mobile; } }
            public int OldBody { get { return m_OldBody; } }
            public int OldHue { get { return m_OldHue; } }
            public bool WasBlessed { get { return m_WasBlessed; } }

            public RestorationState(Mobile mobile, int oldBody, int oldHue, bool wasBlessed)
            {
                m_Mobile = mobile;
                m_OldBody = oldBody;
                m_OldHue = oldHue;
                m_WasBlessed = wasBlessed;
            }
        }
    }
}
