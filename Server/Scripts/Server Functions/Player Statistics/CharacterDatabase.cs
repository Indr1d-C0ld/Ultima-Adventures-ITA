using System;
using Server; 
using Server.Network;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using Server.Commands;
using Server.Commands.Generic;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Server.Items
{
	public class CharacterDatabase : Item
	{
		public Mobile CharacterOwner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Character_Owner { get{ return CharacterOwner; } set{ CharacterOwner = value; } }

		public int CharacterMOTD;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_MOTD { get { return CharacterMOTD; } set { CharacterMOTD = value; InvalidateProperties(); } }

		public int CharacterSkill;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Skill { get { return CharacterSkill; } set { CharacterSkill = value; InvalidateProperties(); } }

		public string CharacterKeys;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_Keys { get { return CharacterKeys; } set { CharacterKeys = value; InvalidateProperties(); } }

		public string CharacterDiscovered;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_Discovered { get { return CharacterDiscovered; } set { CharacterDiscovered = value; InvalidateProperties(); } }

		public int CharacterSheath;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Sheath { get { return CharacterSheath; } set { CharacterSheath = value; InvalidateProperties(); } }

		public int CharacterGuilds;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Guilds { get { return CharacterGuilds; } set { CharacterGuilds = value; InvalidateProperties(); } }

		public string CharacterBoatDoor;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_BoatDoor { get { return CharacterBoatDoor; } set { CharacterBoatDoor = value; InvalidateProperties(); } }

		public string CharacterPublicDoor;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_PublicDoor { get { return CharacterPublicDoor; } set { CharacterPublicDoor = value; InvalidateProperties(); } }

		public int CharacterBegging;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Begging { get { return CharacterBegging; } set { CharacterBegging = value; InvalidateProperties(); } }

		public int CharacterWepAbNames;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_WepAbNames { get { return CharacterWepAbNames; } set { CharacterWepAbNames = value; InvalidateProperties(); } }

		public int CharHue;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Char_Hue { get { return CharHue; } set { CharHue = value; InvalidateProperties(); } }

		public int CharHairHue;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Char_HairHue { get { return CharHairHue; } set { CharHairHue = value; InvalidateProperties(); } }

		public string CharMusical;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Char_Musical { get{ return CharMusical; } set{ CharMusical = value; } }

		public string CharacterLoot;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_Loot { get{ return CharacterLoot; } set{ CharacterLoot = value; } }

		public string CharacterLootB;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_LootB { get{ return CharacterLootB; } set{ CharacterLootB = value; } }

		public string CharacterLootC;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_LootC { get{ return CharacterLootC; } set{ CharacterLootC = value; } }

		public string CharacterWanted;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Character_Wanted { get{ return CharacterWanted; } set{ CharacterWanted = value; } }

		public int CharacterOriental;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Oriental { get { return CharacterOriental; } set { CharacterOriental = value; InvalidateProperties(); } }

		public int CharacterEvil;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Evil { get { return CharacterEvil; } set { CharacterEvil = value; InvalidateProperties(); } }

		public Serial CharacterAutolootContainer;
		[CommandProperty( AccessLevel.GameMaster )]
		public Serial Character_AutolootContainer { get { return CharacterAutolootContainer; } set { CharacterAutolootContainer = value; InvalidateProperties(); } }

		public Serial CharacterAutolootContainerB;
		[CommandProperty( AccessLevel.GameMaster )]
		public Serial Character_AutolootContainerB { get { return CharacterAutolootContainerB; } set { CharacterAutolootContainerB = value; InvalidateProperties(); } }

		public Serial CharacterAutolootContainerC;
		[CommandProperty( AccessLevel.GameMaster )]
		public Serial Character_AutolootContainerC { get { return CharacterAutolootContainerC; } set { CharacterAutolootContainerC = value; InvalidateProperties(); } }

		public Serial CharacterSkinningAutolootContainer;
		[CommandProperty( AccessLevel.GameMaster )]
		public Serial Character_SkinningAutolootContainer { get { return CharacterSkinningAutolootContainer; } set { CharacterSkinningAutolootContainer = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string MessageQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Message_Quest { get { return MessageQuest; } set { MessageQuest = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string ArtifactQuestTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Artifact_QuestTime { get { return ArtifactQuestTime; } set { ArtifactQuestTime = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string StandardQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Standard_Quest { get { return StandardQuest; } set { StandardQuest = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string FishingQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Fishing_Quest { get { return FishingQuest; } set { FishingQuest = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string AssassinQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Assassin_Quest { get { return AssassinQuest; } set { AssassinQuest = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string BardsTaleQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string BardsTale_Quest { get { return BardsTaleQuest; } set { BardsTaleQuest = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string EpicQuestName;
		[CommandProperty( AccessLevel.GameMaster )]
		public string EpicQuest_Name { get{ return EpicQuestName; } set{ EpicQuestName = value; } }

		public int EpicQuestNumber;
		[CommandProperty( AccessLevel.GameMaster )]
		public int EpicQuest_Number { get { return EpicQuestNumber; } set { EpicQuestNumber = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string SpellBarsMage1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Mage1 { get { return SpellBarsMage1; } set { SpellBarsMage1 = value; InvalidateProperties(); } }

		public string SpellBarsMage2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Mage2 { get { return SpellBarsMage2; } set { SpellBarsMage2 = value; InvalidateProperties(); } }

		public string SpellBarsMage3;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Mage3 { get { return SpellBarsMage3; } set { SpellBarsMage3 = value; InvalidateProperties(); } }

		public string SpellBarsMage4;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Mage4 { get { return SpellBarsMage4; } set { SpellBarsMage4 = value; InvalidateProperties(); } }

		public string SpellBarsNecro1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Necro1 { get { return SpellBarsNecro1; } set { SpellBarsNecro1 = value; InvalidateProperties(); } }

		public string SpellBarsNecro2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Necro2 { get { return SpellBarsNecro2; } set { SpellBarsNecro2 = value; InvalidateProperties(); } }

		public string SpellBarsChivalry1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Chivalry1 { get { return SpellBarsChivalry1; } set { SpellBarsChivalry1 = value; InvalidateProperties(); } }

		public string SpellBarsChivalry2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Chivalry2 { get { return SpellBarsChivalry2; } set { SpellBarsChivalry2 = value; InvalidateProperties(); } }

		public string SpellBarsDeath1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Death1 { get { return SpellBarsDeath1; } set { SpellBarsDeath1 = value; InvalidateProperties(); } }

		public string SpellBarsDeath2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Death2 { get { return SpellBarsDeath2; } set { SpellBarsDeath2 = value; InvalidateProperties(); } }

		public string SpellBarsBard1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Bard1 { get { return SpellBarsBard1; } set { SpellBarsBard1 = value; InvalidateProperties(); } }

		public string SpellBarsBard2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Bard2 { get { return SpellBarsBard2; } set { SpellBarsBard2 = value; InvalidateProperties(); } }

		public string SpellBarsPriest1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Priest1 { get { return SpellBarsPriest1; } set { SpellBarsPriest1 = value; InvalidateProperties(); } }

		public string SpellBarsPriest2;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Priest2 { get { return SpellBarsPriest2; } set { SpellBarsPriest2 = value; InvalidateProperties(); } }

		public string SpellBarsMonk1;
		[CommandProperty( AccessLevel.GameMaster )]
		public string SpellBars_Monk1 { get{ return SpellBarsMonk1; } set{ SpellBarsMonk1 = value; } }

                public string SpellBarsMonk2;
                [CommandProperty( AccessLevel.GameMaster )]
                public string SpellBars_Monk2 { get{ return SpellBarsMonk2; } set{ SpellBarsMonk2 = value; } }

                public string SpellBarsWizard1;
                [CommandProperty( AccessLevel.GameMaster )]
                public string SpellBars_Wizard1 { get { return SpellBarsWizard1; } set { SpellBarsWizard1 = value; InvalidateProperties(); } }

                public string SpellBarsWizard2;
                [CommandProperty( AccessLevel.GameMaster )]
                public string SpellBars_Wizard2 { get { return SpellBarsWizard2; } set { SpellBarsWizard2 = value; InvalidateProperties(); } }

                public string SpellBarsWizard3;
                [CommandProperty( AccessLevel.GameMaster )]
                public string SpellBars_Wizard3 { get { return SpellBarsWizard3; } set { SpellBarsWizard3 = value; InvalidateProperties(); } }

                public string OpenToolbars;
                [CommandProperty( AccessLevel.GameMaster )]
                public string Open_Toolbars { get { return OpenToolbars; } set { OpenToolbars = value; } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string ThiefQuest;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Thief_Quest { get{ return ThiefQuest; } set{ ThiefQuest = value; } }

		public string KilledSpecialMonsters;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Killed_SpecialMonsters { get{ return KilledSpecialMonsters; } set{ KilledSpecialMonsters = value; } }

		public string MusicPlaylist;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Music_Playlist { get{ return MusicPlaylist; } set{ MusicPlaylist = value; } }

		public int CharacterBarbaric;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Character_Conan { get { return CharacterBarbaric; } set { CharacterBarbaric = value; InvalidateProperties(); } }

		public int SkillDisplay;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Skill_Display { get { return SkillDisplay; } set { SkillDisplay = value; InvalidateProperties(); } }

		public int MagerySpellHue;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Magery_SpellHue { get { return MagerySpellHue; } set { MagerySpellHue = value; InvalidateProperties(); } }

		public int ClassicPoisoning;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Classic_Poisoning { get { return ClassicPoisoning; } set { ClassicPoisoning = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Constructable]
		public CharacterDatabase() : base( 0x3F1A )
		{
			LootType = LootType.Blessed;
			Visible = false;
			Movable = false;
			Weight = 1.0;
			Name = "Character Statue";
		}

		public override bool DisplayLootType{ get{ return false; } }
		public override bool DisplayWeight{ get{ return false; } }

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( CharacterOwner != null ){ list.Add( 1070722, "Statue of " + CharacterOwner.Name + "" ); }
        }

		public CharacterDatabase( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)5 ); // version
			writer.Write( (Mobile)CharacterOwner );
			writer.Write( CharacterMOTD );
			writer.Write( CharacterSkill );
			writer.Write( CharacterKeys );
			writer.Write( CharacterDiscovered );
			writer.Write( CharacterSheath );
			writer.Write( CharacterGuilds );
			writer.Write( CharacterBoatDoor );
			writer.Write( CharacterPublicDoor );
			writer.Write( CharacterBegging );
			writer.Write( CharacterWepAbNames );

			writer.Write( ArtifactQuestTime );

			writer.Write( StandardQuest );
			writer.Write( FishingQuest );
			writer.Write( AssassinQuest );
			writer.Write( MessageQuest );
			writer.Write( BardsTaleQuest );

			writer.Write( SpellBarsMage1 );
			writer.Write( SpellBarsMage2 );
			writer.Write( SpellBarsMage3 );
			writer.Write( SpellBarsMage4 );
			writer.Write( SpellBarsNecro1 );
			writer.Write( SpellBarsNecro2 );
			writer.Write( SpellBarsChivalry1 );
			writer.Write( SpellBarsChivalry2 );
			writer.Write( SpellBarsDeath1 );
			writer.Write( SpellBarsDeath2 );
			writer.Write( SpellBarsBard1 );
			writer.Write( SpellBarsBard2 );
			writer.Write( SpellBarsPriest1 );
			writer.Write( SpellBarsPriest2 );
                        writer.Write( SpellBarsWizard1 );
                        writer.Write( SpellBarsWizard2 );
                        writer.Write( SpellBarsWizard3 );
                        writer.Write( SpellBarsMonk1 );
                        writer.Write( SpellBarsMonk2 );

                        writer.Write( OpenToolbars );

			writer.Write( ThiefQuest );
			writer.Write( KilledSpecialMonsters );
			writer.Write( MusicPlaylist );
			writer.Write( CharacterWanted );
			writer.Write( CharacterLoot );
			writer.Write( CharacterLootB );
			writer.Write( CharacterLootC );
			writer.Write( CharMusical );
			writer.Write( EpicQuestName );
			writer.Write( CharacterBarbaric );
			writer.Write( SkillDisplay );
			writer.Write( MagerySpellHue );
			writer.Write( ClassicPoisoning );
			writer.Write( CharacterEvil );
			writer.Write( CharacterOriental );
			writer.Write( CharHue );
			writer.Write( CharHairHue );
			writer.Write( EpicQuestNumber );
			writer.Write( CharacterAutolootContainer );
			writer.Write( CharacterAutolootContainerB );
			writer.Write( CharacterAutolootContainerC );
			writer.Write( CharacterSkinningAutolootContainer );
        }

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			CharacterOwner = reader.ReadMobile();
			CharacterMOTD = reader.ReadInt();
			CharacterSkill = reader.ReadInt();
			CharacterKeys = reader.ReadString();
			CharacterDiscovered = reader.ReadString();
			CharacterSheath = reader.ReadInt();
			CharacterGuilds = reader.ReadInt();
			CharacterBoatDoor = reader.ReadString();
			CharacterPublicDoor = reader.ReadString();
			CharacterBegging = reader.ReadInt();
			CharacterWepAbNames = reader.ReadInt();

			ArtifactQuestTime = reader.ReadString();

			StandardQuest = reader.ReadString();
			FishingQuest = reader.ReadString();
			AssassinQuest = reader.ReadString();
			MessageQuest = reader.ReadString();
			BardsTaleQuest = reader.ReadString();

			SpellBarsMage1 = reader.ReadString();
			SpellBarsMage2 = reader.ReadString();
			SpellBarsMage3 = reader.ReadString();
			SpellBarsMage4 = reader.ReadString();
			SpellBarsNecro1 = reader.ReadString();
			SpellBarsNecro2 = reader.ReadString();
			SpellBarsChivalry1 = reader.ReadString();
			SpellBarsChivalry2 = reader.ReadString();
			SpellBarsDeath1 = reader.ReadString();
			SpellBarsDeath2 = reader.ReadString();
			SpellBarsBard1 = reader.ReadString();
			SpellBarsBard2 = reader.ReadString();
			SpellBarsPriest1 = reader.ReadString();
			SpellBarsPriest2 = reader.ReadString();
                        SpellBarsWizard1 = reader.ReadString();
                        SpellBarsWizard2 = reader.ReadString();
                        SpellBarsWizard3 = reader.ReadString();
                        SpellBarsMonk1 = reader.ReadString();
                        SpellBarsMonk2 = reader.ReadString();

                        if (version >= 3)
                        {
                                OpenToolbars = reader.ReadString();
                        }

			ThiefQuest = reader.ReadString();
			KilledSpecialMonsters = reader.ReadString();
			MusicPlaylist = reader.ReadString();
			CharacterWanted = reader.ReadString();
			CharacterLoot = reader.ReadString();
			if (version >= 4)
			{
				CharacterLootB = reader.ReadString();
				CharacterLootC = reader.ReadString();
			}
			CharMusical = reader.ReadString();
			EpicQuestName = reader.ReadString();
			CharacterBarbaric = reader.ReadInt();
			SkillDisplay = reader.ReadInt();
			MagerySpellHue = reader.ReadInt();
			ClassicPoisoning = reader.ReadInt();
			CharacterEvil = reader.ReadInt();
			CharacterOriental = reader.ReadInt();
			CharHue = reader.ReadInt();
			CharHairHue = reader.ReadInt();
			EpicQuestNumber = reader.ReadInt();

                        if (version > 1)
                        {
                                CharacterAutolootContainer = reader.ReadInt();
            }

			if (version >= 4)
			{
				CharacterAutolootContainerB = reader.ReadInt();
				CharacterAutolootContainerC = reader.ReadInt();
			}

			if (version >= 5)
			{
				CharacterSkinningAutolootContainer = reader.ReadInt();
			}

                        if (version < 3)
                        {
                                OpenToolbars = string.Empty;
                        }

			if (version < 4)
			{
				CharacterLootB = null;
				CharacterLootC = null;
				CharacterAutolootContainerB = Serial.Zero;
				CharacterAutolootContainerC = Serial.Zero;
			}

			if (version < 5)
			{
				CharacterSkinningAutolootContainer = Serial.Zero;
			}
		}

		public static CharacterDatabase GetDB( Mobile m ) // ----------------------------------------------------------------------------------------
		{
			if (m == null || m.BankBox == null) return null;
			foreach ( Item item in m.BankBox.Items )
			{
				if ( item is CharacterDatabase )
				{
					CharacterDatabase db = (CharacterDatabase)item;
					if ( db.CharacterOwner == m )
					{
						return db;
					}
				}
			}

			return null;
		}

		public static int GetMySpellHue( Mobile m, int hue ) // ----------------------------------------------------------------------------------------
		{
			if ( m is PlayerMobile )
			{
				CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
				int color = DB.MagerySpellHue;

				if ( color >= 0 ){ hue = color-1; } else { hue = -1; }
			}

			return hue;
		}

		public static bool GetWanted( Mobile m ) // -------------------------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string wanted = DB.CharacterWanted;

			bool isWanted = true;

			if ( wanted == null ){ isWanted = false; }

			return isWanted;
		}

		public static void SetSavage( Mobile m ) // -------------------------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			CharacterDatabase.SetDiscovered( m, "the Savaged Empire", true );
			m.Skills.Cap = MyServerSettings.skillcapbarbaric(); // Odyssey default 11000
			Server.Misc.MorphingTime.RemoveMyClothes( m );

			if ( m.Female )
			{
				DB.CharacterEvil = 0;
				DB.CharacterOriental = 0;
				DB.CharacterBarbaric = 2;
			}
			else
			{
				DB.CharacterEvil = 0;
				DB.CharacterOriental = 0;
				DB.CharacterBarbaric = 1;
			}
			Server.Items.BarbaricSatchel.GivePack( m );

			BaseArmor hat = new LeatherCap();
			hat.Resource = CraftResource.DinosaurLeather;
			hat.Hue = 0xB61;
			hat.Name = "savage cap";
			hat.StrRequirement = 10;
			hat.ItemID = 0x5648;
			m.AddItem( hat );

			BaseArmor gloves = new LeatherGloves();
			gloves.Resource = CraftResource.DinosaurLeather;
			gloves.Hue = 0xB61;
			gloves.Name = "savage gloves";
			gloves.StrRequirement = 10;
			gloves.ItemID = 0x564E;
			m.AddItem( gloves );

			BaseArmor chest = new LeatherChest();
			chest.Resource = CraftResource.DinosaurLeather;
			chest.Hue = 0xB61;
			chest.Name = "savage tunic";
			chest.StrRequirement = 10;
			chest.ItemID = 0x5651;
			m.AddItem( chest );

			BaseArmor boots = new LeatherBoots();
			boots.Resource = CraftResource.DinosaurLeather;
			boots.Hue = 0xB61;
			boots.Name = "savage sandals";
			boots.StrRequirement = 10;
			boots.ItemID = 0x170d;
			m.AddItem( boots );

			BaseArmor pants = new LeatherLegs();
			pants.Resource = CraftResource.DinosaurLeather;
			pants.Hue = 0xB61;
			pants.Name = "savage skirt";
			pants.StrRequirement = 10;
			pants.ItemID = 0x1C08;
			m.AddItem( pants );

			BaseArmor bracers = new LeatherArms();
			bracers.Resource = CraftResource.DinosaurLeather;
			bracers.Hue = 0xB61;
			bracers.Name = "savage bracers";
			bracers.StrRequirement = 10;
			bracers.ItemID = 0x564D;
			m.AddItem( bracers );

			SavageTalisman talisman = new SavageTalisman();
			talisman.ItemOwner = m;
			m.AddItem( talisman );

			BaseWeapon dagger = new Dagger();
			dagger.Resource = CraftResource.Steel;
			m.AddItem( dagger );

			m.AddToBackpack( new Gold( 400 ) );
			m.AddToBackpack( new LambLeg( 15 ) );
			m.AddToBackpack( new Bandage( 100 ) );
			m.AddToBackpack( new Skillet() );

			CampersTent tent = new CampersTent();
			tent.Charges = 50;
			m.AddToBackpack( tent );
		}

		public static void SetSpaceMan( Mobile m ) // -------------------------------------------------------------------------------------------------
		{
			Point3D loc = new Point3D( 7000, 4000, 0 );
			m.MoveToWorld( loc, Map.Felucca );

			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			m.Skills.Cap = MyServerSettings.skillcapalien(); // Odyssey default 40000
			Server.Misc.MorphingTime.RemoveMyClothes( m );

			List<Item> contents = new List<Item>();
			foreach( Item i in m.Backpack.Items )
			{
				contents.Add(i);
			}
			foreach ( Item item in contents )
			{
				item.Delete();
			}

			for( int i = 0; i < m.Skills.Length; i++ )
			{
				Skill skill = (Skill)m.Skills[i];
				skill.Base = 0;
			}

			m.AddItem( new FancyShirt() );
			m.AddItem( new Boots() );
			m.AddItem( new LongPants() );

			BaseWeapon dagger = new Dagger();
			dagger.Name = "knife";
			m.AddItem( dagger );

			Item meat = new CookedBird( 10 );
			meat.Hue = 0xB64;
			meat.Name = "cooked alien meat";
			m.AddToBackpack( meat );

			Item water = new Waterskin();
			water.ItemID = 0x4971;
			water.Name = "empty canteen";
			m.AddToBackpack( water );

			MedicalRecord record = new MedicalRecord();
			record.DataPatient = m.Name;
			m.AddToBackpack( record );

			loc = new Point3D( 4109, 3775, 2 );
			m.MoveToWorld( loc, Map.Trammel );
		}

		public static void SetWanted( Mobile m ) // -------------------------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			string wName = NameList.RandomName( "male" );
			string wTitle = "King";
			string wPron = "he";

			switch ( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0: wTitle = "Emperor"; break;
				case 1: wTitle = "Duke"; break;
				case 2: wTitle = "Earl"; break;
				case 3: wTitle = "Baron"; break;
				case 4: wTitle = "King"; break;
				case 5: wTitle = "Prince"; break;
			}

			if ( Utility.RandomMinMax( 0, 2 ) == 2 ) 
			{
				wName = NameList.RandomName( "female" );
				wPron = "she";

				switch ( Utility.RandomMinMax( 0, 5 ) )
				{
					case 0: wTitle = "Empress"; break;
					case 1: wTitle = "Duchess"; break;
					case 2: wTitle = "Countess"; break;
					case 3: wTitle = "Baroness"; break;
					case 4: wTitle = "Queen"; break;
					case 5: wTitle = "Princess"; break;
				}
			} 

			DB.CharacterWanted = m.Name + " is wanted for the murder of " + wTitle + " " + wName + ". The " + wTitle + " was attacked while " + wPron + " was visting " + RandomThings.GetRandomCity() + ". Citizens stated they seen " + m.Name + " leaving the area with a blood covered " + Server.Misc.RandomThings.GetRandomWeapon() + ". The guard captain " + QuestCharacters.ParchmentWriter() + " warns all citizen to be on the lookout for " + m.Name + " as they escaped their jail cell in Britain.";
			int words = Utility.RandomMinMax( 1, 3 );
			if ( words == 2 ) 
			{
				DB.CharacterWanted = m.Name + " is wanted for the murder of " + wTitle + " " + wName + ". The " + wTitle + " was attacked while " + wPron + " was visting " + RandomThings.GetRandomCity() + ". " + m.Name + " also stole " + Server.Misc.QuestCharacters.QuestItems( true ) + " that the " + wTitle + " had with them. The guard captain " + QuestCharacters.ParchmentWriter() + " warns all citizen to be on the lookout for " + m.Name + " as they escaped their jail cell in Britain.";
			}
			else if ( words == 3 ) 
			{
				DB.CharacterWanted = m.Name + " is wanted for the murder of " + wTitle + " " + wName + ". The " + wTitle + " was assassinated by orders from a group calling themselves " + RandomThings.GetRandomSociety() + ". " + m.Name + " was hired by them to carry out the deed, but their motivations remain unclear. The guard captain " + QuestCharacters.ParchmentWriter() + " warns all citizen to be on the lookout for " + m.Name + " as they escaped their jail cell in Britain.";
			}

			m.Profile = DB.CharacterWanted;
			SetBardsTaleQuest( m, "BardsTaleWin", true );
			m.Skills.Cap = MyServerSettings.skillcapwanted(); // Odyssey default 13000
			m.Kills = 1;
			m.Criminal = true;
			((PlayerMobile)m).Profession = 1;

			GuardNote note = new GuardNote();
			note.ScrollText = DB.CharacterWanted;
			m.AddToBackpack( note );
		}

		public static string GetWantedStory( Mobile m ) // -------------------------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string wanted = DB.CharacterWanted;

			return wanted;
		}

		public static bool GetQuestState( Mobile m, string quest ) // -------------------------------------------------------------------------------
		{
			CharacterDatabase.MarkQuestInfo( m );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			string goal = DB.StandardQuest;	

			if ( quest == "StandardQuest" ){ goal = DB.StandardQuest; }
			else if ( quest == "FishingQuest" ){ goal = DB.FishingQuest; }
			else if ( quest == "AssassinQuest" ){ goal = DB.AssassinQuest; }
			else if ( quest == "MessageQuest" ){ goal = DB.MessageQuest; }
			else if ( quest == "ThiefQuest" ){ goal = DB.ThiefQuest; }

			int nEntry = 1;

			if ( goal.Length > 0 )
			{
				string[] goals = goal.Split('#');
				foreach (string goalz in goals)
				{
					nEntry++;
				}
			}

			if ( nEntry > 3 ){ return true; }

			return false;
		}

		public static string GetQuestInfo( Mobile m, string quest ) // ------------------------------------------------------------------------------
		{
			CharacterDatabase.MarkQuestInfo( m );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			string goal = DB.StandardQuest;	

			if ( quest == "StandardQuest" ){ goal = DB.StandardQuest; }
			else if ( quest == "FishingQuest" ){ goal = DB.FishingQuest; }
			else if ( quest == "AssassinQuest" ){ goal = DB.AssassinQuest; }
			else if ( quest == "MessageQuest" ){ goal = DB.MessageQuest; }
			else if ( quest == "ThiefQuest" ){ goal = DB.ThiefQuest; }

			return goal;
		}

		public static void SetQuestInfo( Mobile m, string quest, string setting ) // ----------------------------------------------------------------
		{
			CharacterDatabase.MarkQuestInfo( m );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			if ( quest == "StandardQuest" ){ DB.StandardQuest = setting; }
			else if ( quest == "FishingQuest" ){ DB.FishingQuest = setting; }
			else if ( quest == "AssassinQuest" ){ DB.AssassinQuest = setting; }
			else if ( quest == "MessageQuest" ){ DB.MessageQuest = setting; }
			else if ( quest == "ThiefQuest" ){ DB.ThiefQuest = setting; }
		}

		public static void ClearQuestInfo( Mobile m, string quest ) // ------------------------------------------------------------------------------
		{
			CharacterDatabase.MarkQuestInfo( m );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			if ( quest == "StandardQuest" ){ DB.StandardQuest = ""; }
			else if ( quest == "FishingQuest" ){ DB.FishingQuest = ""; }
			else if ( quest == "AssassinQuest" ){ DB.AssassinQuest = ""; }
			else if ( quest == "MessageQuest" ){ DB.MessageQuest = ""; }
			else if ( quest == "ThiefQuest" ){ DB.ThiefQuest = ""; }
		}

		public static void MarkQuestInfo( Mobile m ) // ---------------------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			if ( DB.StandardQuest == null ){ DB.StandardQuest = ""; }
			if ( DB.FishingQuest == null ){ DB.FishingQuest = ""; }
			if ( DB.AssassinQuest == null ){ DB.AssassinQuest = ""; }
			if ( DB.MessageQuest == null ){ DB.MessageQuest = ""; }
			if ( DB.ThiefQuest == null ){ DB.ThiefQuest = ""; }
		}

		public static bool GetDiscovered( Mobile m, string world ) // -------------------------------------------------------------------------------
		{
			SetDiscovered( m, "none", false );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string discovered = DB.CharacterDiscovered;

			bool BeenThere = false;

			if ( discovered.Length > 0 )
			{
				string[] discoveries = discovered.Split('#');
				int nEntry = 1;
				foreach (string found in discoveries)
				{
					if ( nEntry == 1 && found == "1" && world == "the Land of Lodoria" ){ BeenThere = true; }
					else if ( nEntry == 2 && found == "1" && world == "the Land of Sosaria" ){ BeenThere = true; }
					else if ( nEntry == 3 && found == "1" && world == "the Island of Umber Veil" ){ BeenThere = true; }
					else if ( nEntry == 4 && found == "1" && world == "the Land of Ambrosia" ){ BeenThere = true; }
					else if ( nEntry == 5 && found == "1" && world == "the Serpent Island" ){ BeenThere = true; }
					else if ( nEntry == 6 && found == "1" && world == "the Isles of Dread" ){ BeenThere = true; }
					else if ( nEntry == 7 && found == "1" && world == "the Savaged Empire" ){ BeenThere = true; }
					else if ( nEntry == 8 && found == "1" && world == "the Bottle World of Kuldar" ){ BeenThere = true; }
					else if ( nEntry == 9 && found == "1" && world == "the Underworld" ){ BeenThere = true; }
					else if ( nEntry == 10 && found == "1" && world == "DarkMoor" ){ BeenThere = true; }

					nEntry++;
				}
			}

			return BeenThere;
		}

		public static void SetDiscovered( Mobile m, string world, bool repeat ) // ------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string discovered = DB.CharacterDiscovered;
			int records = 10; // TOTAL ENTRIES

			if ( discovered == null ){ discovered = "0#0#0#0#0#0#0#0#0#0#"; }

			if ( discovered.Length > 0 )
			{
				string[] discoveries = discovered.Split('#');
				string entry = "";
				int nEntry = 1;

				foreach ( string lands in discoveries )
				{
					if ( nEntry == 1 && world == "the Land of Lodoria" ){ entry = entry + "1#"; }
					else if ( nEntry == 2 && world == "the Land of Sosaria" ){ entry = entry + "1#"; }
					else if ( nEntry == 3 && world == "the Island of Umber Veil" ){ entry = entry + "1#"; }
					else if ( nEntry == 4 && world == "the Land of Ambrosia" ){ entry = entry + "1#"; }
					else if ( nEntry == 5 && world == "the Serpent Island" ){ entry = entry + "1#"; }
					else if ( nEntry == 6 && world == "the Isles of Dread" ){ entry = entry + "1#"; }
					else if ( nEntry == 7 && world == "the Savaged Empire" ){ entry = entry + "1#"; }
					else if ( nEntry == 8 && world == "the Bottle World of Kuldar" ){ entry = entry + "1#"; }
					else if ( nEntry == 9 && world == "the Underworld" ){ entry = entry + "1#"; }
					else if ( nEntry == 10 && world == "DarkMoor" ){ entry = entry + "1#"; }
					else if ( nEntry == 1 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 2 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 3 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 4 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 5 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 6 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 7 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 8 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 9 ){ entry = entry + lands + "#"; }
					else if ( nEntry == 10 ){ entry = entry + lands + "#"; }

					nEntry++;
				}

				while ( nEntry < records+1 )
				{
					entry = entry + "0#";
					nEntry++;
				}

				DB.CharacterDiscovered = entry;

				if ( repeat ){ SetDiscovered( m, world, false ); }
			}
		}

		public static bool GetKeys( Mobile m, string key ) // ---------------------------------------------------------------------------------------
		{
                        SetKeys( m, "none", false );
                        CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
                        string keys = DB.CharacterKeys;
                        if ( keys == null ){ keys = string.Empty; }

                        bool HaveIt = false;

                        if ( keys.Length > 0 )
                        {
                                string[] discoveries = keys.Split('#');
                                int records = 13;

                                for ( int index = 0; index < records && !HaveIt; index++ )
                                {
                                        int nEntry = index + 1;
                                        string found = index < discoveries.Length ? discoveries[index] : "0";

                                        if ( nEntry == 1 && found == "1" && key == "UndermountainKey" ){ HaveIt = true; }
                                        else if ( nEntry == 2 && found == "1" && key == "BlackKnightKey" ){ HaveIt = true; }
                                        else if ( nEntry == 3 && found == "1" && key == "RangerOutpost" ){ HaveIt = true; }
                                        else if ( nEntry == 4 && found == "1" && key == "VordoKey" ){ HaveIt = true; }
                                        else if ( nEntry == 5 && found == "1" && key == "SkullGate" ){ HaveIt = true; }
                                        else if ( nEntry == 6 && found == "1" && key == "SerpentPillars" ){ HaveIt = true; }
                                        else if ( nEntry == 7 && found == "1" && key == "Antiques" ){ HaveIt = true; }
                                        else if ( nEntry == 8 && found == "1" && key == "Museums" ){ HaveIt = true; }
                                        else if ( nEntry == 9 && found == "1" && key == "Runes" ){ HaveIt = true; }
                                        else if ( nEntry == 10 && found == "1" && key == "Virtue" ){ HaveIt = true; }
                                        else if ( nEntry == 11 && found == "1" && key == "Corrupt" ){ HaveIt = true; }
                                        else if ( nEntry == 12 && found == "1" && key == "Gygax" ){ HaveIt = true; }
                                        else if ( nEntry == 13 && found == "1" && key == "DragonRiding" ){ HaveIt = true; }
                                }
                        }

			return HaveIt;
		}

		public static void SetKeys( Mobile m, string key, bool repeat ) // --------------------------------------------------------------------------
		{
                        CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
                        string keys = DB.CharacterKeys;

                        if ( keys == null ){ keys = string.Empty; }

                        string[] discoveries = keys.Split('#');
                        int records = 13; // TOTAL ENTRIES
                        StringBuilder entry = new StringBuilder();

                        for ( int index = 0; index < records; index++ )
                        {
                                int nEntry = index + 1;
                                string keyset = index < discoveries.Length ? discoveries[index] : "0";
                                string sets = ( keyset == "1" ) ? "1" : "0";

                                if ( nEntry == 1 && key == "UndermountainKey" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 2 && key == "BlackKnightKey" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 3 && key == "RangerOutpost" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 4 && key == "VordoKey" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 5 && key == "SkullGate" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 6 && key == "SerpentPillars" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 7 && key == "Antiques" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 8 && key == "Museums" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 9 && key == "Runes" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 10 && key == "Virtue" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 11 && key == "Corrupt" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 12 && key == "Gygax" ){ entry.Append( "1#" ); }
                                else if ( nEntry == 13 && key == "DragonRiding" ){ entry.Append( "1#" ); }
                                else
                                {
                                        entry.Append( sets );
                                        entry.Append( '#' );
                                }
                        }

                        DB.CharacterKeys = entry.ToString();

                        if ( repeat ){ SetKeys( m, key, false ); }
                }

		public static bool GetSpecialsKilled( Mobile m, string who ) // ---------------------------------------------------------------------------------------
		{
			SetSpecialsKilled( m, "none", false );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string whos = DB.KilledSpecialMonsters;

			bool AlreadyDid = false;

			if ( whos.Length > 0 )
			{
				string[] enemies = whos.Split('#');
				int nEntry = 1;
				foreach (string found in enemies)
				{
					if ( nEntry == 1 && found == "1" && who == "Arachnar" ){ AlreadyDid = true; }
					else if ( nEntry == 2 && found == "1" && who == "BlackGateDemon" ){ AlreadyDid = true; }
					else if ( nEntry == 3 && found == "1" && who == "BloodDemigod" ){ AlreadyDid = true; }
					else if ( nEntry == 4 && found == "1" && who == "Xurtzar" ){ AlreadyDid = true; }
					else if ( nEntry == 5 && found == "1" && who == "CaddelliteDragon" ){ AlreadyDid = true; }
					else if ( nEntry == 6 && found == "1" && who == "DragonKing" ){ AlreadyDid = true; }
					else if ( nEntry == 7 && found == "1" && who == "Vulcrum" ){ AlreadyDid = true; }
					else if ( nEntry == 8 && found == "1" && who == "OrkDemigod" ){ AlreadyDid = true; }
					else if ( nEntry == 9 && found == "1" && who == "Mangar" ){ AlreadyDid = true; }
					else if ( nEntry == 10 && found == "1" && who == "Astaroth" ){ AlreadyDid = true; }
					else if ( nEntry == 11 && found == "1" && who == "Faulinei" ){ AlreadyDid = true; }
					else if ( nEntry == 12 && found == "1" && who == "Nosfentor" ){ AlreadyDid = true; }
					else if ( nEntry == 13 && found == "1" && who == "Tarjan" ){ AlreadyDid = true; }
					else if ( nEntry == 14 && found == "1" && who == "Dracula" ){ AlreadyDid = true; }
					else if ( nEntry == 15 && found == "1" && who == "LichKing" ){ AlreadyDid = true; }
					else if ( nEntry == 16 && found == "1" && who == "Surtaz" ){ AlreadyDid = true; }
					else if ( nEntry == 17 && found == "1" && who == "TitanLithos" ){ AlreadyDid = true; }
					else if ( nEntry == 18 && found == "1" && who == "TitanPyros" ){ AlreadyDid = true; }
					else if ( nEntry == 19 && found == "1" && who == "TitanHydros" ){ AlreadyDid = true; }
					else if ( nEntry == 20 && found == "1" && who == "TitanStatos" ){ AlreadyDid = true; }
					else if ( nEntry == 21 && found == "1" && who == "Jormungandr" ){ AlreadyDid = true; }

					nEntry++;
				}
			}

			return AlreadyDid;
		}

		public static void SetSpecialsKilled( Mobile m, string who, bool repeat ) // ----------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string whos = DB.KilledSpecialMonsters;
			int records = 21; // TOTAL ENTRIES

			if ( whos == null ){ whos = "0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#"; }

			if ( whos.Length > 0 )
			{
				string[] enemies = whos.Split('#');
				string entry = "";
				int nEntry = 1;

				foreach ( string killed in enemies )
				{
					if ( nEntry == 1 && who == "Arachnar" ){ entry = entry + "1#"; }
					else if ( nEntry == 2 && who == "BlackGateDemon" ){ entry = entry + "1#"; }
					else if ( nEntry == 3 && who == "BloodDemigod" ){ entry = entry + "1#"; }
					else if ( nEntry == 4 && who == "Xurtzar" ){ entry = entry + "1#"; }
					else if ( nEntry == 5 && who == "CaddelliteDragon" ){ entry = entry + "1#"; }
					else if ( nEntry == 6 && who == "DragonKing" ){ entry = entry + "1#"; }
					else if ( nEntry == 7 && who == "Vulcrum" ){ entry = entry + "1#"; }
					else if ( nEntry == 8 && who == "OrkDemigod" ){ entry = entry + "1#"; }
					else if ( nEntry == 9 && who == "Mangar" ){ entry = entry + "1#"; }
					else if ( nEntry == 10 && who == "Astaroth" ){ entry = entry + "1#"; }
					else if ( nEntry == 11 && who == "Faulinei" ){ entry = entry + "1#"; }
					else if ( nEntry == 12 && who == "Nosfentor" ){ entry = entry + "1#"; }
					else if ( nEntry == 13 && who == "Tarjan" ){ entry = entry + "1#"; }
					else if ( nEntry == 14 && who == "Dracula" ){ entry = entry + "1#"; }
					else if ( nEntry == 15 && who == "LichKing" ){ entry = entry + "1#"; }
					else if ( nEntry == 16 && who == "Surtaz" ){ entry = entry + "1#"; }
					else if ( nEntry == 17 && who == "TitanLithos" ){ entry = entry + "1#"; }
					else if ( nEntry == 18 && who == "TitanPyros" ){ entry = entry + "1#"; }
					else if ( nEntry == 19 && who == "TitanHydros" ){ entry = entry + "1#"; }
					else if ( nEntry == 20 && who == "TitanStatos" ){ entry = entry + "1#"; }
					else if ( nEntry == 21 && who == "Jormungandr" ){ entry = entry + "1#"; }

					else if ( nEntry == 1 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 2 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 3 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 4 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 5 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 6 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 7 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 8 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 9 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 10 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 11 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 12 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 13 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 14 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 15 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 16 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 17 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 18 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 19 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 20 ){ entry = entry + killed + "#"; }
					else if ( nEntry == 21 ){ entry = entry + killed + "#"; }

					nEntry++;
				}

				while ( nEntry < records+1 )
				{
					entry = entry + "0#";
					nEntry++;
				}

				DB.KilledSpecialMonsters = entry;

				if ( repeat ){ SetSpecialsKilled( m, who, false ); }
			}
		}

		public static bool GetBardsTaleQuest( Mobile m, string part ) // -----------------------------------------------------------------------------
		{
			SetBardsTaleQuest( m, "none", false );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string quest = DB.BardsTaleQuest;

			string[] quests = quest.Split('#');
			int nEntry = 1;
			foreach (string goal in quests)
			{
				if ( nEntry == 1 && goal == "1" && part == "BardsTaleMadGodName" ){ return true; }
				else if ( nEntry == 2 && goal == "1" && part == "BardsTaleCatacombKey" ){ return true; }
				else if ( nEntry == 3 && goal == "1" && part == "BardsTaleEbonyKey" ){ return true; }
				else if ( nEntry == 4 && goal == "1" && part == "BardsTaleKylearanKey" ){ return true; }
				else if ( nEntry == 5 && goal == "1" && part == "BardsTaleHarkynKey" ){ return true; }
				else if ( nEntry == 6 && goal == "1" && part == "BardsTaleDragonKey" ){ return true; }
				else if ( nEntry == 7 && goal == "1" && part == "BardsTaleSpectreEye" ){ return true; }
				else if ( nEntry == 8 && goal == "1" && part == "BardsTaleCrystalSword" ){ return true; }
				else if ( nEntry == 9 && goal == "1" && part == "BardsTaleSilverSquare" ){ return true; }
				else if ( nEntry == 10 && goal == "1" && part == "BardsTaleBedroomKey" ){ return true; }
				else if ( nEntry == 11 && goal == "1" && part == "BardsTaleSilverTriangle" ){ return true; }
				else if ( nEntry == 12 && goal == "1" && part == "BardsTaleCrystalGolem" ){ return true; }
				else if ( nEntry == 13 && goal == "1" && part == "BardsTaleSilverCircle" ){ return true; }
				else if ( nEntry == 14 && goal == "1" && part == "BardsTaleMangarKey" ){ return true; }
				else if ( nEntry == 15 && goal == "1" && part == "BardsTaleWin" ){ return true; }

				nEntry++;
			}

			return false;
		}

		public static bool IsBardsTaleQuestActive( Mobile m ) // -------------------------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			if ( DB == null || String.IsNullOrEmpty( DB.BardsTaleQuest ) )
				return false;

			string[] quests = DB.BardsTaleQuest.Split( '#' );

			for ( int i = 0; i < 14 && i < quests.Length; ++i )
			{
				if ( quests[i] == "1" )
					return true;
			}

			return false;
		}

		public static void SetBardsTaleQuest( Mobile m, string part, bool repeat ) // ---------------------------------------------------------------
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			string quest = DB.BardsTaleQuest;
			int records = 15; // TOTAL ENTRIES

			if ( quest == null ){ quest = "0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#"; }

			if ( quest.Length > 0 )
			{
				string[] quests = quest.Split('#');
				string entry = "";
				int nEntry = 1;
				int Finished = 0;

				foreach ( string goal in quests )
				{
					if ( nEntry == 1 && part == "BardsTaleMadGodName" ){ entry = entry + "1#"; }
					else if ( nEntry == 2 && part == "BardsTaleCatacombKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 3 && part == "BardsTaleEbonyKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 4 && part == "BardsTaleKylearanKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 5 && part == "BardsTaleHarkynKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 6 && part == "BardsTaleDragonKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 7 && part == "BardsTaleSpectreEye" ){ entry = entry + "1#"; }
					else if ( nEntry == 8 && part == "BardsTaleCrystalSword" ){ entry = entry + "1#"; }
					else if ( nEntry == 9 && part == "BardsTaleSilverSquare" ){ entry = entry + "1#"; }
					else if ( nEntry == 10 && part == "BardsTaleBedroomKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 11 && part == "BardsTaleSilverTriangle" ){ entry = entry + "1#"; }
					else if ( nEntry == 12 && part == "BardsTaleCrystalGolem" ){ entry = entry + "1#"; }
					else if ( nEntry == 13 && part == "BardsTaleSilverCircle" ){ entry = entry + "1#"; }
					else if ( nEntry == 14 && part == "BardsTaleMangarKey" ){ entry = entry + "1#"; }
					else if ( nEntry == 15 && part == "BardsTaleWin" ){ entry = entry + "1#"; Finished = 1; }

					else if ( nEntry == 1 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 2 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 3 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 4 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 5 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 6 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 7 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 8 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 9 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 10 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 11 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 12 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 13 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 14 ){ entry = entry + goal + "#"; }
					else if ( nEntry == 15 ){ entry = entry + goal + "#"; }

					nEntry++;
				}

				while ( nEntry < records+1 )
				{
					entry = entry + "0#";
					nEntry++;
				}

				DB.BardsTaleQuest = entry;

				if ( Finished > 0 ){ DB.BardsTaleQuest = "0#0#0#0#0#0#0#0#0#0#0#0#0#0#1#"; }

				if ( repeat ){ SetBardsTaleQuest( m, part, false ); }
			}
		}

		public static void SetAutolootContainer( Mobile m, Serial serial )
		{
            CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB(m);
			if (DB == null) return;

			DB.CharacterSkinningAutolootContainer = serial;
        }

		public static Serial GetSkinningAutolootContainer( Mobile m )
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB(m);
			if (DB == null) return Serial.Zero;

			return DB.CharacterSkinningAutolootContainer;
		}

		public static void SetAutolootContainer( Mobile m, Serial serial, int outputIndex )
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB(m);
			if (DB == null) return;

			switch (outputIndex)
			{
				case 2:
					DB.CharacterAutolootContainerB = serial;
					break;
				case 3:
					DB.CharacterAutolootContainerC = serial;
					break;
				default:
					DB.CharacterAutolootContainer = serial;
					break;
			}
		}

		private static Container FindAutolootContainer(PlayerMobile player, Serial serial)
		{
			if (player == null || player.Backpack == null || serial == Serial.Zero)
			{
				return null;
			}

			return player.FindItemRecursive(player.Backpack, i => i.Serial == serial, true) as Container;
		}

		private static bool IsUnidentifiedJewelry( Item item )
		{
			Container wrapper = item as Container;

			if ( wrapper == null )
				return false;

			foreach ( Item contained in wrapper.Items )
			{
				if ( contained is BaseJewel )
					return true;
			}

			return false;
		}

		public static void LootContainer( Mobile m, Container box ) // -------------------------------------------------------------------------------------
		{
			if (box.Deleted || box == null )
				return;

			if (box is Corpse && ( !((Corpse)box).CheckLoot(m, box) || ((Corpse)box).IsCriminalAction(m) ) )
			{
				m.SendMessage( "Corpse ignored, cannot loot corpse or looting is a criminal act!" );
				return;
			}
				
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
			Server.Misc.LootChoiceUpdates.InitializeLootChoice( m );

			string lootingA = DB.CharacterLoot;
			string lootingB = DB.CharacterLootB;
			string lootingC = DB.CharacterLootC;

			if ( lootingA.Length > 0 || lootingB.Length > 0 || lootingC.Length > 0 )
			{
				int foundCoins = 0;
				int foundNuggets = 0;
				int foundGems = 0;
				int foundJewels = 0;
				int foundArrows = 0;
				int foundBolts = 0;
				int foundBandages = 0;
				int foundScrolls = 0;
				int foundReagents = 0;
				int foundPotions = 0;
				int foundParchments = 0;
				int foundBottles = 0;
				int foundNotes = 0;
				int foundBooks = 0;
				int foundSeeds = 0;
				int foundBlankScrolls = 0;
				int foundBodyParts = 0;
				int foundBones = 0;
				int foundUnidentifiedGear = 0;
				int foundUnidentifiedJewelry = 0;

				List<Item> belongingsA = new List<Item>();
				List<Item> belongingsB = new List<Item>();
				List<Item> belongingsC = new List<Item>();
				HashSet<Item> collected = new HashSet<Item>();

				string[] discoveriesA = lootingA.Split('#');
				string[] discoveriesB = lootingB.Split('#');
				string[] discoveriesC = lootingC.Split('#');

				for (int nEntry = 1; nEntry <= 27; nEntry++)
				{
					int index = nEntry - 1;
					int outputIndex = 0;

					if (index >= 0 && index < discoveriesA.Length && discoveriesA[index] == "1")
					{
						outputIndex = 1;
					}
					else if (index >= 0 && index < discoveriesB.Length && discoveriesB[index] == "1")
					{
						outputIndex = 2;
					}
					else if (index >= 0 && index < discoveriesC.Length && discoveriesC[index] == "1")
					{
						outputIndex = 3;
					}

					if (outputIndex == 0)
					{
						continue;
					}

					List<Item> belongings = outputIndex == 1 ? belongingsA : outputIndex == 2 ? belongingsB : belongingsC;

					if ( nEntry == 1 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is Gold ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDCopper ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDSilver ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDXormite ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDJewels ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDGemstones ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is Crystals ){ if (collected.Add(i)) belongings.Add(i); foundCoins = 1; }
							else if ( i is DDGoldNuggets ){ if (collected.Add(i)) belongings.Add(i); foundNuggets = 1; }
						}
					}
                                        else if ( nEntry == 2 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is StarSapphire ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Emerald ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Sapphire ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Ruby ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Citrine ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Amethyst ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is MysticalPearl ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Tourmaline ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Tourmaline ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Amber ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is Diamond ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                        else if ( i is DDRelicGem ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
							else if ( i is ArcaneGem ){ if (collected.Add(i)) belongings.Add(i); foundGems = 1; }
                                                }
                                        }
                                        else if ( nEntry == 3 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is DDRelicJewels ){ if (collected.Add(i)) belongings.Add(i); foundJewels = 1; }
                                                        else if ( i is BaseJewel ){ if (collected.Add(i)) belongings.Add(i); foundJewels = 1; }
                                                }
                                        }
                                        else if ( nEntry == 4 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is Arrow ){ if (collected.Add(i)) belongings.Add(i); foundArrows = 1; }
                                                        else if ( i is ManyArrows100 ){ if (collected.Add(i)) belongings.Add(i); foundArrows = 1; }
                                                        else if ( i is ManyArrows1000 ){ if (collected.Add(i)) belongings.Add(i); foundArrows = 1; }
                                                }
                                        }
                                        else if ( nEntry == 5 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is Bolt ){ if (collected.Add(i)) belongings.Add(i); foundBolts = 1; }
                                                        else if ( i is ManyBolts100 ){ if (collected.Add(i)) belongings.Add(i); foundBolts = 1; }
                                                        else if ( i is ManyBolts1000 ){ if (collected.Add(i)) belongings.Add(i); foundBolts = 1; }
                                                }
                                        }
                                        else if ( nEntry == 6 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is Bandage ){ if (collected.Add(i)) belongings.Add(i); foundBandages = 1; }
                                                }
                                        }
                                        else if ( nEntry == 7 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is ReactiveArmorScroll || i is ClumsyScroll || i is CreateFoodScroll || i is FeeblemindScroll || 
							 i is HealScroll || i is MagicArrowScroll || i is NightSightScroll || i is WeakenScroll || 
							 i is AgilityScroll || i is CunningScroll || i is CureScroll || i is HarmScroll || 
							 i is MagicTrapScroll || i is MagicUnTrapScroll || i is ProtectionScroll || i is StrengthScroll || 
							 i is BlessScroll || i is FireballScroll || i is MagicLockScroll || i is PoisonScroll || 
							 i is TelekinisisScroll || i is TeleportScroll || i is UnlockScroll || i is WallOfStoneScroll || 
							 i is ArchCureScroll || i is ArchProtectionScroll || i is CurseScroll || i is FireFieldScroll || 
							 i is GreaterHealScroll || i is LightningScroll || i is ManaDrainScroll || i is RecallScroll || 
							 i is BladeSpiritsScroll || i is DispelFieldScroll || i is IncognitoScroll || i is MagicReflectScroll || 
							 i is MindBlastScroll || i is ParalyzeScroll || i is PoisonFieldScroll || i is SummonCreatureScroll || 
							 i is DispelScroll || i is EnergyBoltScroll || i is ExplosionScroll || i is InvisibilityScroll || 
							 i is MarkScroll || i is MassCurseScroll || i is ParalyzeFieldScroll || i is RevealScroll || 
							 i is ChainLightningScroll || i is EnergyFieldScroll || i is FlamestrikeScroll || i is GateTravelScroll || 
							 i is ManaVampireScroll || i is MassDispelScroll || i is MeteorSwarmScroll || i is PolymorphScroll || 
							 i is EarthquakeScroll || i is EnergyVortexScroll || i is ResurrectionScroll || i is SummonAirElementalScroll || 
							 i is SummonDaemonScroll || i is SummonEarthElementalScroll || i is SummonFireElementalScroll || i is SummonWaterElementalScroll )
							{ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
						}
					}
                                        else if ( nEntry == 8 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is AnimateDeadScroll || i is BloodOathScroll || i is CorpseSkinScroll || i is CurseWeaponScroll ||
                                                         i is EvilOmenScroll || i is HorrificBeastScroll || i is LichFormScroll || i is MindRotScroll ||
                                                         i is PainSpikeScroll || i is PoisonStrikeScroll || i is StrangleScroll || i is SummonFamiliarScroll ||
                                                         i is VampiricEmbraceScroll || i is VengefulSpiritScroll || i is WitherScroll || i is WraithFormScroll ||
                                                         i is ExorcismScroll )
                                                        { if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
                                                }
                                        }
                                        else if ( nEntry == 9 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is BlackPearl ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is Bloodmoss ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is Garlic ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is Ginseng ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is MandrakeRoot ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is Nightshade ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is SpidersSilk ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is SulfurousAsh ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is reagents_magic_jar1 ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
						}
					}
                                        else if ( nEntry == 10 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is BatWing ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is DaemonBlood ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PigIron ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is NoxCrystal ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is GraveDust ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is reagents_magic_jar2 ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
						}
					}
                                        else if ( nEntry == 13 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is UnknownReagent ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
                                                }
                                        }
                                        else if ( nEntry == 14 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is BasePotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is AutoResPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is ShieldOfEarthPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is WoodlandProtectionPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is ProtectiveFairyPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is HerbalHealingPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is GraspingRootsPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is BlendWithForestPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is SwarmOfInsectsPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is VolcanicEruptionPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is TreefellowPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is StoneCirclePotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is DruidicRunePotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is LureStonePotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is NaturesPassagePotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is MushroomGatewayPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is RestorativeSoilPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is FireflyPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is HellsGateScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is ManaLeechScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is NecroCurePoisonScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is NecroPoisonScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is NecroUnlockScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is PhantasmScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is RetchedAirScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is SpectreShadowScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is UndeadEyesScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is VampireGiftScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is WallOfSpikesScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is BloodPactScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is GhostlyImagesScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is GhostPhaseScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is GraveyardGatewayScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is HellsBrandScroll ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is MagicalDyes ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is UnusualDyes ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is BottleOfAcid ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is CrystallineJar ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is NecroSkinPotion ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilWood ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilAmethyst ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilCaddellite ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilEmerald ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilGarnet ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilIce ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilJade ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilLeather ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilMarble ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilMetal ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilOnyx ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilQuartz ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilRuby ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilSapphire ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilSilver ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilSpinel ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilStarRuby ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilTopaz ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is OilWood ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
						}
					}
                                        else if ( nEntry == 15 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is UnknownKeg ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
							else if ( i is UnknownLiquid ){ if (collected.Add(i)) belongings.Add(i); foundPotions = 1; }
						}
					}
                                        else if ( nEntry == 16 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is ArmysPaeonScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is EnchantingEtudeScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is EnergyCarolScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is EnergyThrenodyScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is FireCarolScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is FireThrenodyScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is FoeRequiemScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is IceCarolScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is IceThrenodyScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is KnightsMinneScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is MagesBalladScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is MagicFinaleScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is PoisonCarolScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is PoisonThrenodyScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is SheepfoeMamboScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
							else if ( i is SinewyEtudeScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
						}
					}
                                        else if ( nEntry == 17 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is UnknownScroll ){ if (collected.Add(i)) belongings.Add(i); foundScrolls = 1; }
                                                }
                                        }
					else if ( nEntry == 18 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is ScrollClue ){ if (collected.Add(i)) belongings.Add(i); foundParchments = 1; }
							else if ( i is MapItem ){ if (collected.Add(i)) belongings.Add(i); foundParchments = 1; }
						}
					}
					else if ( nEntry == 19 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is Bottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is BeverageBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is BottleOfAcid ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is BottleOfParts ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is HairDyeBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is GlassBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is GreenBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is RedBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is SmallBrownBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is SmallGreenBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is SmallVioletBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is TinyYellowBottle ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
							else if ( i is SmallBlueFlask ){ if (collected.Add(i)) belongings.Add(i); foundBottles = 1; }
						}
					}
					else if ( nEntry == 20 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is SomeRandomNote ){ if (collected.Add(i)) belongings.Add(i); foundNotes = 1; }
							else if ( i is BardsTaleNote ){ if (collected.Add(i)) belongings.Add(i); foundNotes = 1; }
							else if ( i is DoomFlayerNote ){ if (collected.Add(i)) belongings.Add(i); foundNotes = 1; }
							else if ( i is AssassinNote ){ if (collected.Add(i)) belongings.Add(i); foundNotes = 1; }
							else if ( i is GuardNote ){ if (collected.Add(i)) belongings.Add(i); foundNotes = 1; }
						}
					}
					else if ( nEntry == 21 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is BaseBook ){ if (collected.Add(i)) belongings.Add(i); foundBooks = 1; }
							else if ( i is DynamicBook ){ if (collected.Add(i)) belongings.Add(i); foundBooks = 1; }
						}
					}
					else if ( nEntry == 22 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is Server.Engines.Plants.Seed ){ if (collected.Add(i)) belongings.Add(i); foundSeeds = 1; }
							else if ( i is FertileDirt ){ if (collected.Add(i)) belongings.Add(i); foundSeeds = 1; }
						}
					}
					else if ( nEntry == 23 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is BlankScroll ){ if (collected.Add(i)) belongings.Add(i); foundBlankScrolls = 1; }
						}
					}
					else if ( nEntry == 24 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is BodyPart ){ if (collected.Add(i)) belongings.Add(i); foundBodyParts = 1; }
						}
					}
					else if ( nEntry == 25 )
					{
						foreach( Item i in box.Items )
						{
							if ( i is Bone ){ if (collected.Add(i)) belongings.Add(i); foundBones = 1; }
						}
					}
					else if ( nEntry == 26 )
					{
						foreach( Item i in box.Items )
						{
							if ( ( i is UnidentifiedItem || i is UnidentifiedArtifact ) && !IsUnidentifiedJewelry( i ) ){ if (collected.Add(i)) belongings.Add(i); foundUnidentifiedGear = 1; }
						}
					}
					else if ( nEntry == 27 )
					{
						foreach( Item i in box.Items )
						{
							if ( ( i is UnidentifiedItem || i is UnidentifiedArtifact ) && IsUnidentifiedJewelry( i ) ){ if (collected.Add(i)) belongings.Add(i); foundUnidentifiedJewelry = 1; }
						}
					}
                                        else if ( nEntry == 11 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
							if ( i is EyeOfToad ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is FairyEgg ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is GargoyleEar ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is BeetleShell ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is MoonCrystal ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PixieSkull ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is RedLotus ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is SeaSalt ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is SilverWidow ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is SwampBerries ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is Brimstone ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is ButterflyWings ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is reagents_magic_jar3 ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
						}
					}
                                        else if ( nEntry == 12 )
                                        {
                                                foreach( Item i in box.Items )
                                                {
                                                        if ( i is PlantHerbalism_Leaf ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PlantHerbalism_Flower ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PlantHerbalism_Mushroom ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PlantHerbalism_Lilly ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PlantHerbalism_Cactus ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
							else if ( i is PlantHerbalism_Grass ){ if (collected.Add(i)) belongings.Add(i); foundReagents = 1; }
						}
					}
				}

				Container autolootContainerA = null;
				Container autolootContainerB = null;
				Container autolootContainerC = null;
				PlayerMobile player = m as PlayerMobile;
				if (player != null && player.Backpack != null)
				{
					autolootContainerA = FindAutolootContainer(player, DB.CharacterAutolootContainer);
					autolootContainerB = FindAutolootContainer(player, DB.CharacterAutolootContainerB);
					autolootContainerC = FindAutolootContainer(player, DB.CharacterAutolootContainerC);
                }

                int sound = 0;
				foreach ( Item item in belongingsA )
				{
					sound = 1;
					if (autolootContainerA != null)
                    {
						if (item.Deleted || player.Puritain && !item.IsPure) continue;

						autolootContainerA.OnDragDrop(player, item); // Needs to respect coinpouches, etc
                    }
					else
					{
                        m.AddToBackpack(item);
                    }
                }

				foreach ( Item item in belongingsB )
				{
					sound = 1;
					if (autolootContainerB != null)
					{
						if (item.Deleted || player.Puritain && !item.IsPure) continue;

						autolootContainerB.OnDragDrop(player, item); // Needs to respect coinpouches, etc
					}
					else
					{
						m.AddToBackpack(item);
					}
				}

				foreach ( Item item in belongingsC )
				{
					sound = 1;
					if (autolootContainerC != null)
					{
						if (item.Deleted || player.Puritain && !item.IsPure) continue;

						autolootContainerC.OnDragDrop(player, item); // Needs to respect coinpouches, etc
					}
					else
					{
						m.AddToBackpack(item);
					}
				}

				if ( sound > 0 )
				{
					m.PlaySound( 0x048 );

					string sMessage = "You take some ";

					if ( foundCoins > 0 ){ sMessage = sMessage + "coins, "; }
					if ( foundGems > 0 ){ sMessage = sMessage + "gems, "; }
					if ( foundNuggets > 0 ){ sMessage = sMessage + "nuggets, "; }
					if ( foundJewels > 0 ){ sMessage = sMessage + "jewels, "; }
					if ( foundArrows > 0 ){ sMessage = sMessage +" arrows, "; }
					if ( foundBolts > 0 ){ sMessage = sMessage + "bolts, "; }
					if ( foundBandages > 0 ){ sMessage = sMessage + "bandages, "; }
					if ( foundScrolls > 0 ){ sMessage = sMessage + "scrolls, "; }
					if ( foundReagents > 0 ){ sMessage = sMessage + "reagents, "; }
					if ( foundPotions > 0 ){ sMessage = sMessage + "potions, "; }
					if ( foundParchments > 0 ){ sMessage = sMessage + "parchments, "; }
					if ( foundBottles > 0 ){ sMessage = sMessage + "bottles, "; }
					if ( foundNotes > 0 ){ sMessage = sMessage + "notes, "; }
					if ( foundBooks > 0 ){ sMessage = sMessage + "books, "; }
					if ( foundSeeds > 0 ){ sMessage = sMessage + "seeds, "; }
					if ( foundBlankScrolls > 0 ){ sMessage = sMessage + "blank scrolls, "; }
					if ( foundBodyParts > 0 ){ sMessage = sMessage + "body parts, "; }
					if ( foundBones > 0 ){ sMessage = sMessage + "bones, "; }
					if ( foundUnidentifiedGear > 0 ){ sMessage = sMessage + "unidentified gear, "; }
					if ( foundUnidentifiedJewelry > 0 ){ sMessage = sMessage + "unidentified jewelry, "; }

					sMessage = sMessage + "and put them in your pack.";

					m.SendMessage( sMessage );
				}
			}
			
			ReagentBelt reagentBelt = ReagentBelt.FindReagentBelt( m );

			if ( reagentBelt != null )
				reagentBelt.AutoLootFromContainer( m, box );

			Server.Gumps.MReagentGump.XReagentGump( m );
			Server.Gumps.QuickBar.RefreshQuickBar( m );
			Server.Gumps.WealthBar.RefreshWealthBar( m );
		}
	}
}


namespace Server.Commands
{
	public class CreateDB
	{
		public static void Initialize()
		{
			CommandSystem.Register( "CreateDB", AccessLevel.Counselor, new CommandEventHandler( DBs_OnCommand ) );
		}

		private class DBsTarget : Target
		{
			public DBsTarget() : base( -1, true, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is PlayerMobile )
				{
					Mobile m = (Mobile)o;

					CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

					if ( DB == null )
					{
						CharacterDatabase MyDB = new CharacterDatabase();
						MyDB.CharacterOwner = m;
						m.BankBox.DropItem( MyDB );
					}
				}
			}
		}

		[Usage( "CreateDB" )]
		[Description( "Creates a character statue in the character bank box, which acts as a database." )]
		private static void DBs_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new DBsTarget();
		}
	}
}
