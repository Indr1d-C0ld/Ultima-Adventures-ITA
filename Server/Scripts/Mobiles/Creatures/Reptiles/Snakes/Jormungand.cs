using System;
using Server;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using System.Collections.Generic;
using Server.Misc;
using Server.Regions;

namespace Server.Mobiles
{
        [CorpseName( "a sea serpents corpse" )]
        public class Jormungandr : BaseCreature
        {
                private DateTime m_NextVenomFlood;
                private DateTime m_NextTidalCrash;
                private DateTime m_AbilityLock;
                private static readonly TimeSpan SharedCooldown = TimeSpan.FromSeconds( 8.0 );

                public override int BreathPhysicalDamage{ get{ return 0; } }
                public override int BreathFireDamage{ get{ return 0; } }
                public override int BreathColdDamage{ get{ return 0; } }
                public override int BreathPoisonDamage{ get{ return 100; } }
                public override int BreathEnergyDamage{ get{ return 0; } }
		public override int BreathEffectHue{ get{ return 0x3F; } }
		public override int BreathEffectSound{ get{ return 0x658; } }
		public override bool ReacquireOnMovement{ get{ return !Controlled; } }
                public override bool HasBreath{ get{ return true; } }
                public override bool CanBreath{ get{ return base.CanBreath && DateTime.UtcNow >= m_AbilityLock; } }
                public override double BreathEffectDelay{ get{ return 0.1; } }
                public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 10 ); }
                public override void BreathStart( Mobile target )
                {
                        base.BreathStart( target );
                        m_AbilityLock = DateTime.UtcNow + SharedCooldown;
                }

		[Constructable]
                public Jormungandr() : base( AIType.AI_Predator, FightMode.Closest, 20, 2, 0.2, 0.4 )
		{
                        Name = "Jormungandr";
                        Title = "the Midgard Serpent";
                        Body = 150;
                        Hue = 0xBAB;
                        BaseSoundID = 447;

                        SetStr( 1450, 1625 );
                        SetDex( 150, 220 );
                        SetInt( 950, 1200 );

                        SetHits( 6200, 6800 );

                        SetDamage( 55, 70 );

                        SetDamageType( ResistanceType.Physical, 50 );
                        SetDamageType( ResistanceType.Poison, 35 );
                        SetDamageType( ResistanceType.Energy, 15 );

                        RangeFight = 4;
                        BaseWeapon reach = new JormungandrReachFists
                        {
                                MaxRange = 4,
                                Layer = Layer.TwoHanded,
                                Movable = false,
                                Visible = false
                        };
                        AddItem( reach );

                        SetResistance( ResistanceType.Physical, 70, 85 );
                        SetResistance( ResistanceType.Fire, 70, 80 );
                        SetResistance( ResistanceType.Cold, 70, 85 );
                        SetResistance( ResistanceType.Poison, 90, 100 );
                        SetResistance( ResistanceType.Energy, 75, 85 );

                        SetSkill( SkillName.EvalInt, 110.0, 130.0 );
                        SetSkill( SkillName.Magery, 110.0, 130.0 );
                        SetSkill( SkillName.Meditation, 90.0, 110.0 );
                        SetSkill( SkillName.MagicResist, 125.0, 150.0 );
                        SetSkill( SkillName.Tactics, 110.0, 130.0 );
                        SetSkill( SkillName.Wrestling, 115.0, 135.0 );

                        Fame = 24000;
                        Karma = -24000;

                        VirtualArmor = 85;

                        Item Venom = new VenomSack();
                                Venom.Name = "venom sack";
                                AddItem( Venom );

                        CanSwim = true;
                        CantWalk = true;

                        m_NextVenomFlood = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 20 ) );
                        m_NextTidalCrash = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 18, 28 ) );
                        m_AbilityLock = DateTime.UtcNow;
                }

                public override void GenerateLoot()
                {
                        AddLoot( LootPack.FilthyRich, 10 );
		}

        public override void OnAfterSpawn()
        {
			base.OnAfterSpawn();
			Worlds.MoveToRandomOcean( this );
		}

		public override bool OnBeforeDeath()
		{
			Server.Misc.LoggingFunctions.LogSlayingLord( this.LastKiller, this.Name );
			return base.OnBeforeDeath();
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			Mobile killer = this.LastKiller;
			if ( killer != null )
			{
				if ( killer is BaseCreature )
					killer = ((BaseCreature)killer).GetMaster();

				if ( killer is PlayerMobile )
				{
					if ( GetPlayerInfo.LuckyKiller( killer.Luck, killer ) )
					{
						StatueJormungandr trophy = new StatueJormungandr();

						string waters = Server.Misc.Worlds.GetRegionName( killer.Map, killer.Location );

						if ( waters == "the Bottle World of Kuldar" ){ 		waters = "the Kuldar Sea"; }
						else if ( waters == "the Land of Ambrosia" ){ 		waters = "the Ambrosia Lakes"; }
						else if ( waters == "the Island of Umber Veil" ){ 	waters = "the Umber Sea"; }
						else if ( waters == "the Land of Lodoria" ){ 		waters = "the Lodoria Ocean"; }
						else if ( waters == "the Underworld" ){ 			waters = "Carthax Lake"; }
						else if ( waters == "the Serpent Island" ){ 		waters = "the Serpent Seas"; }
						else if ( waters == "the Isles of Dread" ){ 		waters = "the Dreadful Sea"; }
						else if ( waters == "the Savaged Empire" ){ 		waters = "the Savage Seas"; }
						else if ( waters == "the Land of Sosaria" ){ 		waters = "the Sosaria Ocean"; }

						trophy.AnimalWhere = "From " + waters;
						string trophyKiller = killer.Name + " the " + Server.Misc.GetPlayerInfo.GetSkillTitle( killer );
						trophy.AnimalKiller = "Killed by " + trophyKiller;
						c.DropItem( trophy );
					}
					if ( GetPlayerInfo.LuckyKiller( killer.Luck, killer ) && Utility.RandomMinMax( 1, 5 ) == 1 && !Server.Items.CharacterDatabase.GetSpecialsKilled( killer, "Jormungandr" ) )
					{
						Server.Items.CharacterDatabase.SetSpecialsKilled( killer, "Jormungandr", true );
						ManualOfItems book = new ManualOfItems();
							book.Hue = 0xB3D;
							book.Name = "Tome of Midgard Relics";
							book.m_Charges = 1;
							book.m_Skill_1 = 99;
							book.m_Skill_2 = 0;
							book.m_Skill_3 = 0;
							book.m_Skill_4 = 0;
							book.m_Skill_5 = 0;
							book.m_Value_1 = 20.0;
							book.m_Value_2 = 0.0;
							book.m_Value_3 = 0.0;
							book.m_Value_4 = 0.0;
							book.m_Value_5 = 0.0;
							book.m_Slayer_1 = 34;
							book.m_Slayer_2 = 8;
							book.m_Owner = null;
							book.m_Extra = "of Midgard";
							book.m_FromWho = "Taken from Jormungandr the Serpent of Midgard";
							book.m_HowGiven = "Acquired by";
							book.m_Points = 200;
							book.m_Hue = 0xB3D;
							c.DropItem( book );
					}
				}
			}

			LootChest MyChest = new LootChest( 10 );
			Server.Misc.ContainerFunctions.MakeDemonBox( MyChest, this );
			MyChest.ItemID = Utility.RandomList( 0x2823, 0x2824 );
			MyChest.Hue = Utility.RandomList( 0xB3D, 0xB3E, 0xB3F, 0xB40 );
			c.DropItem( MyChest );
		}

                public override int Meat{ get{ return 10; } }
                public override MeatType MeatType{ get{ return MeatType.Fish; } }
                public override int Hides{ get{ return 10; } }
                public override HideType HideType{ get{ return HideType.Spined; } }
                public override int Scales{ get{ return 8; } }
                public override ScaleType ScaleType{ get{ return ScaleType.Blue; } }
                public override bool BleedImmune{ get{ return true; } }
                public override bool AutoDispel{ get{ return true; } }
                public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
                public override Poison HitPoison{ get{ return Poison.Lethal; } }
                public override int TreasureMapLevel{ get{ return 6; } }

                public override void OnThink()
                {
                        base.OnThink();

                        Mobile combatant = Combatant;

                        if ( combatant == null || Map == null )
                                return;

                        if ( !Controlled && DateTime.UtcNow >= m_NextVenomFlood && DateTime.UtcNow >= m_AbilityLock && InRange( combatant, 20 ) && CanBeHarmful( combatant ) )
                        {
                                VenomFlood();
                                m_AbilityLock = DateTime.UtcNow + SharedCooldown;
                                m_NextVenomFlood = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 15, 25 ) );
                        }

                        if ( !Controlled && DateTime.UtcNow >= m_NextTidalCrash && DateTime.UtcNow >= m_AbilityLock && InRange( combatant, 20 ) )
                        {
                                TidalCrash();
                                m_AbilityLock = DateTime.UtcNow + SharedCooldown;
                                m_NextTidalCrash = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 25, 35 ) );
                        }
                }

                public override void OnGaveMeleeAttack( Mobile defender )
                {
                        base.OnGaveMeleeAttack( defender );

                        defender.Stam = Math.Max( 0, defender.Stam - Utility.RandomMinMax( 15, 30 ) );
                        defender.Mana = Math.Max( 0, defender.Mana - Utility.RandomMinMax( 10, 20 ) );

                        if ( Utility.RandomDouble() <= 0.6 )
                                defender.ApplyPoison( this, Poison.Lethal );
                }

                private void VenomFlood()
                {
                        PublicOverheadMessage( MessageType.Regular, 0x44, false, "The world serpent vomits a flood of venom!" );
                        Effects.PlaySound( Location, Map, 0x658 );
                        List<Mobile> targets = new List<Mobile>();

                        foreach ( Mobile m in GetMobilesInRange( 20 ) )
                        {
                                if ( m != this && CanBeHarmful( m ) && m.Alive )
                                        targets.Add( m );
                        }

                        IEntity source = new Entity( Serial.Zero, Location, Map );

                        for ( int i = 0; i < targets.Count; i++ )
                        {
                                Mobile m = targets[i];
                                DoHarmful( m );

                                int damage = Utility.RandomMinMax( 280, 320 );
                                AOS.Damage( m, this, damage, 0, 0, 0, 100, 0 );
                                m.ApplyPoison( this, Poison.Lethal );
                                m.SendMessage( "You are drenched in Midgard venom!" );

                                IEntity dest = new Entity( Serial.Zero, m.Location, Map );
                                Effects.SendMovingEffect( source, dest, 0x36D4, 7, 0, false, false, 0x44, 0 );
                        }
                }

                private void TidalCrash()
                {
                        PublicOverheadMessage( MessageType.Regular, 0x480, false, "Jormungandr thrashes, summoning a crushing tide!" );
                        Effects.SendLocationEffect( Location, Map, 0x3709, 30, 0xB3D, 0 );

                        List<Mobile> targets = new List<Mobile>();

                        foreach ( Mobile m in GetMobilesInRange( 20 ) )
                        {
                                if ( m != this && CanBeHarmful( m ) && m.Alive )
                                        targets.Add( m );
                        }

                        IEntity source = new Entity( Serial.Zero, Location, Map );

                        for ( int i = 0; i < targets.Count; i++ )
                        {
                                Mobile m = targets[i];
                                DoHarmful( m );

                                if ( Utility.RandomBool() )
                                        m.Paralyze( TimeSpan.FromSeconds( 2.5 ) );

                                IEntity dest = new Entity( Serial.Zero, m.Location, Map );
                                Effects.SendMovingEffect( source, dest, 0x36D4, 7, 0, false, false, 0xB3D, 0 );

                                AOS.Damage( m, this, Utility.RandomMinMax( 280, 320 ), 60, 0, 20, 0, 20 );
                                m.SendMessage( "You are battered by the serpent's tidal crash!" );
                        }
                }

                public Jormungandr( Serial serial ) : base( serial )
                {
                }

                private class JormungandrReachFists : Fists
                {
                        public override int AosSpeed { get { return 25; } }
                        public override float MlSpeed { get { return 4.25f; } }
                        public override int OldSpeed { get { return 25; } }

                        public JormungandrReachFists()
                        {
                        }

                        public JormungandrReachFists( Serial serial ) : base( serial )
                        {
                        }

                        public override void Serialize( GenericWriter writer )
                        {
                                base.Serialize( writer );
                                writer.Write( (int) 0 );
                        }

                        public override void Deserialize( GenericReader reader )
                        {
                                base.Deserialize( reader );

                                int version = 0;

                                if ( !reader.End() )
                                        version = reader.ReadInt();
                        }
                }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
namespace Server.Items
{
	[Furniture]
	[Flipable( 0x4D0A, 0x4D0B )]
	public class StatueJormungandr : BaseStatue
	{
		public string AnimalKiller;
		public string AnimalWhere;

		[CommandProperty(AccessLevel.Owner)]
		public string Animal_Killer { get { return AnimalKiller; } set { AnimalKiller = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Animal_Where { get { return AnimalWhere; } set { AnimalWhere = value; InvalidateProperties(); } }

		[Constructable]
		public StatueJormungandr() : base( 0x4D0A )
		{
			Name = "Statue of Jormungandr";
			Weight = 60;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( AnimalWhere != "" && AnimalWhere != null ){ list.Add( 1070722, AnimalWhere ); }
			if ( AnimalKiller != "" && AnimalKiller != null ){ list.Add( 1049644, AnimalKiller ); }
        }

        public StatueJormungandr( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( AnimalKiller );
            writer.Write( AnimalWhere );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            AnimalKiller = reader.ReadString();
            AnimalWhere = reader.ReadString();
	    }
	}
}