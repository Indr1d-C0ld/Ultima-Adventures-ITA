using System;
using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an avenger corpse")]
    public class AnimalAvenger : BaseCreature
    {
        private DateTime m_LastCombatTime;

        public AnimalAvenger(int body, int hue, int sound) : base( AIType.AI_Melee, FightMode.Closest, 25, 1, 0.4, 0.3 )
        {
            Name = "spirit of the wild";
            Body = body;
            Hue = hue;
            BaseSoundID = sound;

            SetStr(500, 550);
            SetDex(200, 250);
            SetInt(100, 150);
            ActiveSpeed = 0.2;
	        PassiveSpeed = 0.1;

            SetHits(600, 700);
            SetStam(200, 250);
            SetMana(0);

            SetDamage(20, 30);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 50, 60);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.MagicResist, 100.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 100.0);

            if (Utility.Random(1, 2) == 2) // 50% chance to have an OmniAI skill set
		    OmniAI.SetRandomSkillSet(this, 70.0, 110.0);

            Fame = 15000;
            Karma = -15000;
            Criminal = true;
		    Kills = Utility.RandomMinMax(5, 20);

            VirtualArmor = 40;

            m_LastCombatTime = DateTime.UtcNow;
        }

	    // + OmniAI support +
	    protected override BaseAI ForcedAI
	    {
	        get
	        {
	    	return new OmniAI(this);
	        }
	    }
	    // - OmniAI support -
        public override bool AlwaysMurderer { get { return true; } }
        public override bool AlwaysAttackable { get { return true; } }

        public override bool IsEnemy(Mobile m)
        {
            if (m is PlayerMobile)
                return true;

            return base.IsEnemy(m);
        }

        public override void OnCombatantChange()
        {
            base.OnCombatantChange();

            if (Combatant != null)
                m_LastCombatTime = DateTime.UtcNow;
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant == null && DateTime.UtcNow - m_LastCombatTime > TimeSpan.FromMinutes(5))
                Delete();
        }

        public override bool DeleteCorpseOnDeath { get { return true; } }

        [Constructable]
        public AnimalAvenger() : this(0xD6, 0, 0xA3)
        {
        }

        public AnimalAvenger(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_LastCombatTime = DateTime.UtcNow;
        }
    }
}

