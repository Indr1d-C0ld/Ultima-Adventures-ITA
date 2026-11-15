using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Misc;
using Server.Items;
using Server.Network;
using Server.Commands;
using Server.Commands.Generic;
using Server.Mobiles;
using Server.Accounting;
using Server.Regions;
using System.Threading;
using Server.OneTime.Events;
using Server.Engines.PartySystem;

namespace Server.Spells.Song
{
        public abstract class ModifierSong : Song
        {
                private static readonly Dictionary<Serial, Dictionary<Type, SongEffect>> m_ActiveEffects = new Dictionary<Serial, Dictionary<Type, SongEffect>>();
                private ArrayList m_Targets;
                private SongBook m_Book;
		private bool m_isHkCheck;
		private bool m_DamageOverTime;
		private ResistanceMod m_Modification;
		private int m_Duration;
		private int m_ModType;
                private int m_ModLevel;
                private int m_ModAmount;
                private string m_ExpiryMessage;
                private int m_TargetOriginalResistance;
                private ResistanceMod m_mod;
                private Dictionary<Serial, ResistanceMod> m_ResistanceMods;
                private bool m_TimerSubscribed;
		
		public ResistanceMod Mod {
			get { return m_mod; }
			set { m_mod = value;}
		}
		
		public bool IsDamageOverTime {
			get { return m_DamageOverTime; }
			set { m_DamageOverTime = value;}
		}

		public bool IsHkCheck {
			get { return m_isHkCheck; }
			set { m_isHkCheck = value;}
		}

		public int ModAmount { // used in ModTypes 3
			get { return m_ModAmount; }
			set { m_ModAmount = value;}
		}

		public int ModLevel { // used in ModTypes 3
			get { return m_ModLevel; }
			set { m_ModLevel = value;}
		}
		public string ExpiryMessage {
			get { return m_ExpiryMessage; }
			set { m_ExpiryMessage = value;}
		}

		public int ModType { // 1 Buff; 2 Debuff; 3 Hit stacking; 4 StatMod
			get{ return m_ModType; }
            set{ m_ModType = value; }
		}

		public ArrayList Targets {
			get{ return m_Targets; }
            set{ m_Targets = value; }
		}

		public ResistanceMod DummyModification {
		 	get{ return m_Modification; }
            set{ m_Modification = value; }
		}


		public ResistanceMod Modification {
		 	get{ return m_Modification; }
            set{ m_Modification = value; }
		}

        public int Duration
        {
            get{ return m_Duration; }
            set{ m_Duration = value; }
        }

        public int TargetOriginalResistance
        {
            get{ return m_TargetOriginalResistance; }
            set{ m_TargetOriginalResistance = value; }
        }

        public SongBook SongBook
        {
            get{ return m_Book; }
            set{ m_Book = value; }
        }

        protected virtual BuffIcon BuffIconId
        {
            get { return (BuffIcon)(-1); }
        }

        protected virtual int BuffTitleCliloc
        {
            get { return 0; }
        }

        protected virtual int BuffDescriptionCliloc
        {
            get { return BuffTitleCliloc + 1; }
        }

        protected virtual string GetBuffArgs(SongEffect songEffect)
        {
            return null;
        }
        

                public ModifierSong( Mobile caster, Item scroll, SpellInfo info ) : base( caster, scroll, info )
                {
                        ExpiryMessage = "The effect of the musical spell wears off.";
                        Duration = 0;
                        Targets = new ArrayList();
                        IsHkCheck = false;
                        ModLevel = 6;
                        IsDamageOverTime = false;
                        m_ResistanceMods = new Dictionary<Serial, ResistanceMod>();
                }

                protected bool HasActiveResistanceSong(Mobile mobile)
                {
                        Dictionary<Type, SongEffect> effects;

                        if (mobile == null || !m_ActiveEffects.TryGetValue(mobile.Serial, out effects))
                        {
                                return false;
                        }

                        foreach (Type songType in effects.Keys)
                        {
                                if (IsResistanceSongType(songType))
                                {
                                        return true;
                                }
                        }

                        return false;
                }

		public void AddSongEffect(SongEffect songEffect) {
			if (songEffect == null)
				return;

			PlayerMobile player = Caster as PlayerMobile;
			if (player == null)
				return;

			List<SongEffect> songEffects = player.SongEffects;
			if (songEffects == null)
			{
				songEffects = new List<SongEffect>();
				player.SongEffects = songEffects;
			}

			songEffects.Add(songEffect);
			if (IsBeneficialSongType(songEffect.Song.GetType()))
				RegisterSongEffect(songEffect);

                        AddSongBuff(World.FindMobile(songEffect.MobileSerial), songEffect);
		}

                private void AddSongBuff(Mobile mobile, SongEffect songEffect)
                {
                        if (mobile == null || mobile.Deleted || !(mobile is PlayerMobile) || songEffect == null || songEffect.RemainingDuration <= 0 || BuffTitleCliloc <= 0 || BuffIconId == (BuffIcon)(-1))
                        {
                                return;
                        }

                        string args = GetBuffArgs(songEffect);
                        BuffInfo buff;
                        if (args == null)
                        {
                                buff = new BuffInfo(BuffIconId, BuffTitleCliloc, BuffDescriptionCliloc, TimeSpan.FromSeconds( songEffect.RemainingDuration ), mobile);
                        }
                        else
                        {
                                buff = new BuffInfo(BuffIconId, BuffTitleCliloc, BuffDescriptionCliloc, TimeSpan.FromSeconds( songEffect.RemainingDuration ), mobile, args);
                        }

                        BuffInfo.AddBuff(mobile, buff);
                }

                protected void RefreshSongBuff(Mobile mobile, SongEffect songEffect)
                {
                        AddSongBuff(mobile, songEffect);
                }

                private void RemoveSongBuff(Mobile mobile)
                {
                        if (mobile == null || !(mobile is PlayerMobile) || BuffIconId == (BuffIcon)(-1))
                        {
                                return;
                        }

                        BuffInfo.RemoveBuff(mobile, BuffIconId);
                }

		private static void RegisterSongEffect(SongEffect songEffect)
		{
			if (!IsBeneficialSongType(songEffect.Song.GetType()))
				return;

			Dictionary<Type, SongEffect> effects;

			if (!m_ActiveEffects.TryGetValue(songEffect.MobileSerial, out effects))
			{
				effects = new Dictionary<Type, SongEffect>();
				m_ActiveEffects[songEffect.MobileSerial] = effects;
			}

			effects[songEffect.Song.GetType()] = songEffect;
		}

		private static void UnregisterSongEffect(SongEffect songEffect)
		{
			if (!IsBeneficialSongType(songEffect.Song.GetType()))
				return;

			Dictionary<Type, SongEffect> effects;
			SongEffect registeredEffect;

			if (!m_ActiveEffects.TryGetValue(songEffect.MobileSerial, out effects) || !effects.TryGetValue(songEffect.Song.GetType(), out registeredEffect) || registeredEffect != songEffect)
				return;

			effects.Remove(songEffect.Song.GetType());

			if (effects.Count == 0)
			{
				m_ActiveEffects.Remove(songEffect.MobileSerial);
			}
		}

		private static SongEffect GetActiveSongEffect(Mobile mobile, Type songType)
		{
			Dictionary<Type, SongEffect> effects;
			SongEffect songEffect;

			if (mobile != null && IsBeneficialSongType(songType) && m_ActiveEffects.TryGetValue(mobile.Serial, out effects) && effects.TryGetValue(songType, out songEffect))
				return songEffect;

			return null;
		}

		protected bool PrepareBeneficialTarget(Mobile target, int candidateStrength, int candidateDuration)
		{
			if (target == null || target.Deleted)
				return false;

			Dictionary<Type, SongEffect> effects;
			if (IsResistanceSongType(GetType()) && m_ActiveEffects.TryGetValue(target.Serial, out effects))
			{
				SongEffect conflictingEffect = null;

				foreach (KeyValuePair<Type, SongEffect> entry in effects)
				{
					if (entry.Key != GetType() && IsResistanceSongType(entry.Key))
					{
						conflictingEffect = entry.Value;
						break;
					}
				}

				ModifierSong conflictingSong = conflictingEffect == null ? null : conflictingEffect.Song as ModifierSong;
				if (conflictingSong != null)
					conflictingSong.RemoveSongEffect(target, conflictingEffect);
			}

			SongEffect existing = GetActiveSongEffect(target, GetType());
			if (existing == null)
				return true;

			if (candidateStrength < existing.Strength)
			{
				existing.ResetDuration();
				ModifierSong refreshSong = existing.Song as ModifierSong;
				if (refreshSong != null)
					refreshSong.RefreshSongBuff(target, existing);
				return false;
			}

			ModifierSong existingSong = existing.Song as ModifierSong;
			if (existingSong != null)
				existingSong.RemoveSongEffect(target, existing);

			return true;
		}

		private static bool IsResistanceSongType(Type songType)
		{
			return songType == typeof(EnergyCarolSong) || songType == typeof(FireCarolSong) || songType == typeof(IceCarolSong) || songType == typeof(PoisonCarolSong) || songType == typeof(KnightsMinneSong);
		}

		private static bool IsBeneficialSongType(Type songType)
		{
			return songType == typeof(ArmysPaeonSong) || songType == typeof(MagesBalladSong) || songType == typeof(EnchantingEtudeSong) || songType == typeof(SinewyEtudeSong) || songType == typeof(SheepfoeMamboSong) || IsResistanceSongType(songType);
		}

		protected int GetResistanceCandidateAmount(Mobile mobile, ResistanceType resistanceType, int amount)
		{
			if (mobile == null || amount <= 0)
				return 0;

			int resistance = mobile.GetResistance(resistanceType);
			SongEffect existing = GetActiveSongEffect(mobile, GetType());
			if (existing != null && existing.ResistanceMod != null)
				resistance -= existing.ResistanceMod.Offset;

			if (resistance >= 70)
				return 0;

			return Math.Min(amount, 70 - resistance);
		}

		public bool SongIsAlreadyOnTarget(Mobile target) {
			List<SongEffect> songEffects = ((PlayerMobile)Caster).SongEffects;
			SongEffect songEffect = songEffects.Find(effect => target.Serial == effect.MobileSerial && effect.Song.GetType() == this.GetType() );
			if (songEffect != null) {
				Caster.SendMessage("This song is already in effect on the target");
			}
			return (songEffect != null);
		}


		public void GetElligbleBeneficials() {
			PlayerMobile player = (PlayerMobile)Caster;
			int range = SkillScaledAreaRange.Calculate( Caster.Skills[SkillName.Musicianship].Value );
			Targets.Add(Caster);
			// only buff nearby companions
			foreach ( Mobile m in player.getNearbyPets( range ) )
			{
				Targets.Add( m );
			}
			Party party = (Party)player.Party;
			if (party != null) {
				// only buff nearby party members
				foreach ( Mobile m in player.GetMobilesInRange( range ) )
				{
					foreach (PartyMemberInfo member in party.Members) {
						if (member.Mobile.Serial == m.Serial && m.Serial != Caster.Serial) {
							Targets.Add( m );
						}	
					}
				}
			}

		}

		public void DetermineModLevelBySkill(int musicSkill) {
		 if (musicSkill > 120 && musicSkill < 240)
            {
                ModLevel = 7;
            }
            else if (musicSkill < 360)
            { 
                ModLevel = 8;
            }
            else if (musicSkill < 480)
            { 
                ModLevel = 9;
            }
            else if (musicSkill >= 480)
            {
                ModLevel = 10;
            }
		}

		public virtual bool CheckSlayer( BaseInstrument instrument, Mobile defender )
		{
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName( instrument.Slayer );
			SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName( instrument.Slayer2 );
			if ( atkSlayer != null && atkSlayer.Slays( defender )  || atkSlayer2 != null && atkSlayer2.Slays( defender ) )
				return true;

                        return false;
                }

		public bool HasSongBook() {
            return (SongBook != null);
		}

		public bool CanCast() {
		  //get songbook instrument
            
            if ( !BaseInstrument.CheckMusicianship( Caster ) )
			{
				Caster.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
			} else if (SongBook.Instrument == null || !(Caster.InRange(SongBook.Instrument.GetWorldLocation(), 1)))
            {
                Caster.SendMessage("Your instrument is missing! You can select another from your song book.");
            } else if (SongBook.Instrument == null || !(Caster.InRange(SongBook.Instrument.GetWorldLocation(), 1)))
            {
                Caster.SendMessage("You need an instrument to play that song!");
            } else if (Targets.Count == 0) {
            	Caster.SendMessage("There are no eligible targets for your song!");
            } else if (!IsHkCheck && CheckSequence())
			{
				return true;
			} else if (IsHkCheck) { // is a HkCheck being performed, which calls CheckSequence anyway
				return true;
			}
			return false;
		}

                public void RemoveSongEffect(Mobile mobile) {
                        if (mobile == null)
                        {
                                return;
                        }

						PlayerMobile player = Caster as PlayerMobile;
                        if (player == null || player.SongEffects == null)
                        {
                                return;
                        }

                        for (int i = player.SongEffects.Count - 1; i >= 0; --i)
                        {
                                SongEffect songEffect = player.SongEffects[i];
                                if (songEffect.MobileSerial == mobile.Serial && songEffect.Song == this)
                                {
                                        RemoveSongEffect(mobile, songEffect);
                                }
                        }
                }

                public void RemoveSongEffect(Mobile mobile, SongEffect songEffect)
                {
						if (songEffect == null || songEffect.Song != this)
						{
							return;
						}

						RemoveSongBuff(mobile);

						PlayerMobile player = Caster as PlayerMobile;
                        if (player != null && player.SongEffects != null)
                        {
                                player.SongEffects.Remove(songEffect);
                        }

                        RemoveEffectModifier(mobile, songEffect);
                        ResistanceMod registeredMod;
                        if (songEffect.ResistanceMod != null && m_ResistanceMods.TryGetValue(songEffect.MobileSerial, out registeredMod) && registeredMod == songEffect.ResistanceMod)
                        {
                                m_ResistanceMods.Remove(songEffect.MobileSerial);
                        }
                        UnregisterSongEffect(songEffect);
                        RemoveTarget(songEffect.MobileSerial);

                        if (!HasRegisteredTargets())
                        {
                                UnsubscribeSecondTimer();
                        }
                }

                private static void RemoveEffectModifier(Mobile mobile, SongEffect songEffect)
                {
                        if (mobile == null)
                        {
                                return;
                        }

                        if (songEffect.StatMod != null)
                        {
                                mobile.RemoveStatMod(songEffect.StatMod.Name);
                        }

                        if (songEffect.ResistanceMod != null)
                        {
                                mobile.RemoveResistanceMod(songEffect.ResistanceMod);
                        }
                }

                private void RemoveTarget(Serial serial)
                {
                        for (int i = Targets.Count - 1; i >= 0; --i)
                        {
                                Mobile target = Targets[i] as Mobile;
                                if (target == null || target.Serial == serial)
                                {
                                        Targets.RemoveAt(i);
                                }
                        }
                }

                private bool HasRegisteredTargets()
                {
                        PlayerMobile player = Caster as PlayerMobile;
                        List<SongEffect> songEffects = player == null ? null : player.SongEffects;

                        if (songEffects == null)
                        {
                                return false;
                        }

                        for (int i = 0; i < songEffects.Count; ++i)
                        {
                                if (songEffects[i].Song == this)
                                {
                                        return true;
                                }
                        }

                        return false;
                }

                protected void SubscribeSecondTimer()
                {
                        if (m_TimerSubscribed || !HasRegisteredTargets())
                        {
                                return;
                        }

                        OneTimeSecEvent.SecTimerTick += SecondTimerTick;
                        m_TimerSubscribed = true;
                }

                private void UnsubscribeSecondTimer()
                {
                        OneTimeSecEvent.SecTimerTick -= SecondTimerTick;
                        m_TimerSubscribed = false;
                }

                protected string GetStatModName(Mobile mobile)
                {
                        return GetType().FullName + ":" + Caster.Serial + ":" + mobile.Serial;
                }

                protected ResistanceMod CreateResistanceMod(ResistanceType type, int offset)
                {
                        return new ResistanceMod(type, offset, true);
                }

                protected void RegisterResistanceMod(Mobile mobile, ResistanceMod mod)
                {
                        if (mobile == null || mod == null)
                        {
                                return;
                        }

                        ResistanceMod existingMod;
                        if (m_ResistanceMods.TryGetValue(mobile.Serial, out existingMod))
                        {
                                mobile.RemoveResistanceMod(existingMod);
                        }

                        m_ResistanceMods[mobile.Serial] = mod;
                        mobile.AddResistanceMod(mod);
                        Modification = mod;
                        Mod = mod;
                }

                protected void RemoveResistanceMod(Mobile mobile)
                {
                        if (mobile == null)
                        {
                                return;
                        }

                        ResistanceMod existingMod;
                        if (m_ResistanceMods.TryGetValue(mobile.Serial, out existingMod))
                        {
                                mobile.RemoveResistanceMod(existingMod);
                                m_ResistanceMods.Remove(mobile.Serial);
                        }
                        else if (Modification != null)
                        {
                                mobile.RemoveResistanceMod(Modification);
                        }
                }

		public int CalculateDurationByFame(Mobile mobile, bool inverse) {
			int fameModifier = this.CalculateFameProbability(mobile);
			int duration = (int)MyServerSettings.PlayerLevelMod(100-fameModifier, mobile);
			if (inverse) {
				duration = (int)MyServerSettings.PlayerLevelMod(0+fameModifier, mobile);
			}

			if (duration < MyServerSettings.BardSongMinDurationCap()) {
				duration = MyServerSettings.BardSongMinDurationCap(); 
			} 
			return duration;
		}

		public int CalculateDurationByMagicResist(Mobile mobile) {
			int magicResist = (int)mobile.Skills[SkillName.MagicResist].Value;
			int duration =  (int)MyServerSettings.PlayerLevelMod((MusicSkill(Caster)/10), mobile);
			if (magicResist > 0) {
				duration = (int)MyServerSettings.PlayerLevelMod(MusicSkill(Caster)/(int)mobile.Skills[SkillName.MagicResist].Value, mobile);
			}
			if (duration < MyServerSettings.BardSongMinDurationCap()) {
				duration = MyServerSettings.BardSongMinDurationCap(); 
			} 
			return duration;
		}

		public bool FailedFameCheck(Mobile mobile) {
			Random rand = new Random(); 
			// get dynamic fame check success modifier
			// the higher the probability the greater chance of failure
			int probabilityToFail = this.CalculateFameProbability(mobile);
			return (rand.Next(1,101) <= probabilityToFail);
		} 

		public int CalculateFameProbability(Mobile mobile) {
			Random rand = new Random(); 
			int totalSkill = MusicSkill( Caster );
			// for a maxed character with any mob that has 20k+ fame, add a random difficulty modifier
			int fameModifier = (int)(mobile.Fame/totalSkill);
			int max = 20;
			fameModifier += rand.Next(1,max); 	
			return fameModifier;
		}

		// public void LowerMobileResistance(EditableMobile mobile, int amount) {
		// 	if (ModResistanceType != null) {
		// 		TargetOriginalResistance = mobile.GetResistance(ModResistanceType);
		// 		if (amount > TargetOriginalResistance) {
		// 			amount = TargetOriginalResistance;
		// 		}
		// 		int newAmount = TargetOriginalResistance - amount;
		//     	mobile.SetResistance(ModResistanceType, newAmount);
		//     	if (mobile is BaseCreature) {
		//     		Caster.SendMessage("Your song reduces " + mobile.Name + "'s resistances by " + amount.ToString() );	
		//     	} else {
		//     		Caster.SendMessage("Your song reduces the target's resistances by " + amount.ToString() );	
		//     	}
		    	
		// 	}	
		// }

        public void SecondTimerTick(object sender, EventArgs e)
        {
			if (ModType == 5)
			{
				TickLegacyDominate();
				return;
			}

			if (ModType == 2)
			{
				TickLegacyHarmful();
				return;
			}

			PlayerMobile player = Caster as PlayerMobile;
			List<SongEffect> songEffects = player == null ? null : player.SongEffects;

			if (songEffects == null)
			{
				UnsubscribeSecondTimer();
				return;
			}

			for (int i = songEffects.Count - 1; i >= 0; --i)
			{
				SongEffect effect = songEffects[i];
				if (effect.Song != this)
					continue;

				Mobile mobile = World.FindMobile(effect.MobileSerial);
				if (mobile == null || mobile.Deleted)
				{
					RemoveSongEffect(mobile, effect);
					continue;
				}

				if (effect.RemainingDuration <= 0)
				{
					ExpireSongEffect(mobile, effect);
					continue;
				}

				if (ModType == 3)
				{
					mobile.Hits += MyServerSettings.PlayerLevelMod(ModLevel, mobile);
				}
				else if (ModType == 6 && Caster.Serial != mobile.Serial)
				{
					mobile.Mana += MyServerSettings.PlayerLevelMod(ModLevel, mobile);
				}

				if (!effect.TickDuration())
				{
					ExpireSongEffect(mobile, effect);
				}
			}

			if (!HasRegisteredTargets())
			{
				UnsubscribeSecondTimer();
			}
        }

		private void TickLegacyDominate()
		{
			if (Duration > 0)
			{
				Duration--;
				return;
			}

			for (int i = Targets.Count - 1; i >= 0; --i)
			{
				BaseCreature creature = Targets[i] as BaseCreature;
				if (creature != null)
				{
					creature.Owners.Remove(Caster);
					creature.Summoned = false;
					creature.SetControlMaster(null);
					if (Caster.Serial != creature.Serial)
						creature.SendMessage(ExpiryMessage);
				}
			}

			Targets.Clear();
			Caster.SendMessage(ExpiryMessage);
			UnsubscribeSecondTimer();
		}

		private void TickLegacyHarmful()
		{
			if (Duration > 0)
			{
				Duration--;
				return;
			}

			for (int i = Targets.Count - 1; i >= 0; --i)
			{
				Mobile mobile = Targets[i] as Mobile;
				if (mobile == null)
				{
					Targets.RemoveAt(i);
					continue;
				}

				Serial serial = mobile.Serial;
				RemoveResistanceMod(mobile);
				RemoveLegacySongEffect(serial);
				if (!mobile.Deleted && Caster.Serial != serial)
					mobile.SendMessage(ExpiryMessage);
			}

			Caster.SendMessage(ExpiryMessage);
			UnsubscribeSecondTimer();
		}

		private void RemoveLegacySongEffect(Serial serial)
		{
			PlayerMobile player = Caster as PlayerMobile;
			if (player != null && player.SongEffects != null)
			{
				for (int i = player.SongEffects.Count - 1; i >= 0; --i)
				{
					SongEffect songEffect = player.SongEffects[i];
					if (songEffect.MobileSerial == serial && songEffect.Song == this)
					{
						player.SongEffects.RemoveAt(i);
						UnregisterSongEffect(songEffect);
					}
				}
			}

			RemoveTarget(serial);
		}

		private void ExpireSongEffect(Mobile mobile, SongEffect effect)
		{
			if ((ModType == 1 || ModType == 2) && effect.ResistanceMod == null)
			{
				RemoveResistanceMod(mobile);
			}

			if (ModType == 5 && mobile is BaseCreature)
			{
				BaseCreature creature = (BaseCreature)mobile;
				creature.Owners.Remove(Caster);
				creature.Summoned = false;
				creature.SetControlMaster(null);
			}

			RemoveSongEffect(mobile, effect);
			mobile.SendMessage(ExpiryMessage);
			if (ModType == 2 && Caster.Serial != mobile.Serial)
				Caster.SendMessage(ExpiryMessage);
		}
    }
}
