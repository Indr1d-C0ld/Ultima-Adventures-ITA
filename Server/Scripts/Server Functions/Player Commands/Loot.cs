using System;
using Server;
using System.Collections;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Misc;
using Server.Commands;
using Server.Commands.Generic;
using Server.Prompts;
using Server.Gumps;
using Server.Targeting;

namespace Server.Misc
{
    class LootChoiceUpdates
    {
		private const string DefaultLootSettings = "0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#"; // Default per-category loot settings
		private const int LootCategoryCount = 27;

		public static void UpdateLootChoice( Mobile m, int outputIndex, int nChange )
		{
			LootChoiceUpdates.InitializeLootChoice( m );

			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );

			string LootChoiceSetting = GetLootChoiceSetting( DB, outputIndex );

			string[] eachSetting = LootChoiceSetting.Split('#');
			int nLine = 1;
			string newSettings = "";

			foreach (string eachSettings in eachSetting)
			{
				if ( nLine == nChange )
				{
					string sChange = "0";
					if ( eachSettings == "0" ){ sChange = "1"; }
					newSettings = newSettings + sChange + "#";
				}
				else if ( nLine > LootCategoryCount )
				{
				}
				else
				{
					newSettings = newSettings + eachSettings + "#";
				}
				nLine++;
			}

			SetLootChoiceSetting( DB, outputIndex, newSettings ); 
		}

		public static void SetLootContainer( Mobile m, int origin, int outputIndex )
		{
			var t = new InternalTarget(origin, outputIndex);
			m.Target = t;
		}

		public static void InitializeLootChoice( Mobile m )
		{
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( m );
                        if ( DB.CharacterLoot == null ){ DB.CharacterLoot = DefaultLootSettings; }
						if ( DB.CharacterLootB == null ){ DB.CharacterLootB = DefaultLootSettings; }
						if ( DB.CharacterLootC == null ){ DB.CharacterLootC = DefaultLootSettings; }

						DB.CharacterLoot = NormalizeLootSettings( DB.CharacterLoot );
						DB.CharacterLootB = NormalizeLootSettings( DB.CharacterLootB );
						DB.CharacterLootC = NormalizeLootSettings( DB.CharacterLootC );
		}

		private static string NormalizeLootSettings( string settings )
		{
			if ( string.IsNullOrEmpty( settings ) )
			{
				return DefaultLootSettings;
			}

			string[] parts = settings.Split('#');
			if ( parts.Length >= LootCategoryCount + 1 )
			{
				return settings;
			}

			var newSettings = settings;
			for ( int i = parts.Length; i <= LootCategoryCount; i++ )
			{
				newSettings += "0#";
			}

			return newSettings;
		}

		private class InternalTarget : Target
        {
            public int m_Origin; // Source gump origin (help navigation return target)
			public int m_OutputIndex; // Selected loot output index (A=1, B=2, C=3)

            public InternalTarget(int origin, int outputIndex) : base( 0, false, TargetFlags.None )
			{
				m_Origin = origin;
				m_OutputIndex = outputIndex;
			}

			protected override void OnTarget( Mobile from, object targeted )
            {
				var item = targeted as Item;
				var serial = from != targeted && item != null && item is Container ? item.Serial : Serial.Zero;
                Server.Items.CharacterDatabase.SetAutolootContainer(from, serial, m_OutputIndex);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                base.OnTargetFinish(from);

                from.SendGump(new LootChoices(from, m_Origin, m_OutputIndex));
            }
        }

		public static string GetLootChoiceSetting( CharacterDatabase db, int outputIndex )
		{
			if ( db == null )
			{
				return DefaultLootSettings;
			}

			switch (outputIndex)
			{
				case 2:
					return db.CharacterLootB;
				case 3:
					return db.CharacterLootC;
				default:
					return db.CharacterLoot;
			}
		}

		public static void SetLootChoiceSetting( CharacterDatabase db, int outputIndex, string settings )
		{
			if ( db == null )
			{
				return;
			}

			switch (outputIndex)
			{
				case 2:
					db.CharacterLootB = settings;
					break;
				case 3:
					db.CharacterLootC = settings;
					break;
				default:
					db.CharacterLoot = settings;
					break;
			}
		}

		public static Serial GetLootContainerSerial( CharacterDatabase db, int outputIndex )
		{
			if ( db == null )
			{
				return Serial.Zero;
			}

			switch (outputIndex)
			{
				case 2:
					return db.CharacterAutolootContainerB;
				case 3:
					return db.CharacterAutolootContainerC;
				default:
					return db.CharacterAutolootContainer;
			}
		}
	}
}

namespace Server.Gumps 
{
    public class LootChoices : Gump
    {
		public int m_Origin; // Source gump origin (help navigation return target)
		public int m_OutputIndex; // Selected loot output index (A=1, B=2, C=3)
		public int m_FilterPage; // Loot filter page (1 or 2)

		private const int ButtonTabA = 1000; // Tab A selector button
		private const int ButtonTabB = 1001; // Tab B selector button
		private const int ButtonTabC = 1002; // Tab C selector button
		private const int ButtonContainerA = 1100; // Container selection for A
		private const int ButtonContainerB = 1101; // Container selection for B
		private const int ButtonContainerC = 1102; // Container selection for C
		private const int ButtonFilterPage2 = 1200;
		private const int ButtonFilterPage1 = 1201;

		public static void Initialize()
		{
            CommandSystem.Register( "loot", AccessLevel.Player, new CommandEventHandler( LootChoice_OnCommand ) );
		}

		public static void Register( string command, AccessLevel access, CommandEventHandler handler )
		{
            CommandSystem.Register(command, access, handler);
		}

		[Usage( "loot" )]
		[Description( "Allows you to setup automatic looting." )]
		public static void LootChoice_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			from.CloseGump( typeof( LootChoices ) );
			from.SendGump( new LootChoices( from, 0 ) );
        }

        public LootChoices ( Mobile from, int origin ) : this ( from, origin, 1, 1 )
        {
        }

        public LootChoices ( Mobile from, int origin, int outputIndex ) : this ( from, origin, outputIndex, 1 )
        {
        }

        public LootChoices ( Mobile from, int origin, int outputIndex, int filterPage ) : base ( 25,25 )
        {
			m_Origin = origin;
			m_OutputIndex = outputIndex;
			m_FilterPage = filterPage;

			LootChoiceUpdates.InitializeLootChoice( from );
			CharacterDatabase DB = Server.Items.CharacterDatabase.GetDB( from );
			string mySettings = LootChoiceUpdates.GetLootChoiceSetting( DB, m_OutputIndex );

			this.Closable=true;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;

			BuildFrame();
			BuildHeaderAndTopControls(DB);

			if ( m_FilterPage == 2 )
				BuildFilterPageTwo(mySettings);
			else
				BuildFilterPageOne(mySettings);
        }

		private void BuildFrame()
		{
			AddPage(0);
			AddImage(0, 0, 151);
			AddImage(300, 0, 151);
			AddImage(0, 300, 151);
			AddImage(300, 300, 151);
			AddImage(0, 338, 151);
			AddImage(300, 338, 151);
			AddImage(2, 2, 129);
			AddImage(298, 2, 129);
			AddImage(2, 298, 129);
			AddImage(298, 298, 129);
			AddImage(2, 336, 129);
			AddImage(298, 336, 129);
			AddImage(362, 8, 138);
			AddImage(8, 8, 133);
                        AddImage(7, 413, 148);
                        AddImage(251, 413, 144);
                        AddImage(380, 569, 159);
                        AddImage(185, 569, 143);
                        AddImage(188, 577, 156);
                        AddImage(376, 579, 156);
                        AddImage(213, 577, 156);
                        AddImage(240, 587, 156);
                        AddImage(349, 579, 156);
                        AddImage(320, 585, 156);
                        AddImage(294, 591, 156);
                        AddImage(269, 594, 156);
		}

		private void BuildHeaderAndTopControls(CharacterDatabase DB)
		{
			AddHtml( 253, 43, 94, 21, @"<BODY><BASEFONT Color=#FBFBFB><BIG><CENTER>LOOT</CENTER></BIG></BASEFONT></BODY>", false, false);
			AddHtml( 102, 85, 396, 124, @"<BODY><BASEFONT Color=#FBFBFB>Check the categories of items to automatically take from common dungeon chests or corpses and put them in your backpack. Magery and necromancer reagents are those used specifically by those characters. Alchemic reagents are unique to alchemy only. Herbalist reagents are plants that one may find, used in druidic herbalism.</BASEFONT></BODY>", false, false);
			AddHtml( 110, 190, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Loot Output</BASEFONT></BODY>", false, false);

			int tabAIcon = m_OutputIndex == 1 ? 4018 : 3609;
			int tabBIcon = m_OutputIndex == 2 ? 4018 : 3609;
			int tabCIcon = m_OutputIndex == 3 ? 4018 : 3609;
			AddButton(200, 190, tabAIcon, tabAIcon, ButtonTabA, GumpButtonType.Reply, 1);
			AddHtml( 222, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>A</BASEFONT></BODY>", false, false);
			AddButton(240, 190, tabBIcon, tabBIcon, ButtonTabB, GumpButtonType.Reply, 1);
			AddHtml( 262, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>B</BASEFONT></BODY>", false, false);
			AddButton(280, 190, tabCIcon, tabCIcon, ButtonTabC, GumpButtonType.Reply, 1);
			AddHtml( 302, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>C</BASEFONT></BODY>", false, false);

			AddHtml( 320, 190, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Set Loot Container</BASEFONT></BODY>", false, false);
			int checkboxIconA = LootChoiceUpdates.GetLootContainerSerial(DB, 1) != 0 ? 2153 : 2151;
			int checkboxIconB = LootChoiceUpdates.GetLootContainerSerial(DB, 2) != 0 ? 2153 : 2151;
			int checkboxIconC = LootChoiceUpdates.GetLootContainerSerial(DB, 3) != 0 ? 2153 : 2151;
            AddButton(430, 185, checkboxIconA, checkboxIconA, ButtonContainerA, GumpButtonType.Reply, 1);
			AddHtml( 448, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>A</BASEFONT></BODY>", false, false);
            AddButton(455, 185, checkboxIconB, checkboxIconB, ButtonContainerB, GumpButtonType.Reply, 1);
			AddHtml( 473, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>B</BASEFONT></BODY>", false, false);
            AddButton(480, 185, checkboxIconC, checkboxIconC, ButtonContainerC, GumpButtonType.Reply, 1);
			AddHtml( 498, 190, 15, 21, @"<BODY><BASEFONT Color=#FBFBFB>C</BASEFONT></BODY>", false, false);
            AddItem(257, 504, 2169);
		}

		private void BuildFilterPageOne(string mySettings)
		{
            AddHtml( 145, 225, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Coins & Nuggets</BASEFONT></BODY>", false, false);
            AddHtml( 145, 260, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Gems</BASEFONT></BODY>", false, false);
            AddHtml( 145, 295, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Jewelry</BASEFONT></BODY>", false, false);
            AddHtml( 145, 330, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Arrows</BASEFONT></BODY>", false, false);
            AddHtml( 145, 365, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Bolts</BASEFONT></BODY>", false, false);
            AddHtml( 145, 400, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Bandages</BASEFONT></BODY>", false, false);
            AddHtml( 145, 435, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Magery Scrolls</BASEFONT></BODY>", false, false);
            AddHtml( 145, 470, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Necromancer Scrolls</BASEFONT></BODY>", false, false);
            AddHtml( 145, 510, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Unknown Scrolls</BASEFONT></BODY>", false, false);

            AddHtml( 320, 225, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Magery Reagents</BASEFONT></BODY>", false, false);
            AddHtml( 320, 260, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Necromancy Reagents</BASEFONT></BODY>", false, false);
            AddHtml( 320, 295, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Alchemic Reagents</BASEFONT></BODY>", false, false);
            AddHtml( 320, 330, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Herbalist Reagents</BASEFONT></BODY>", false, false);
            AddHtml( 320, 365, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Unknown Reagents</BASEFONT></BODY>", false, false);
            AddHtml( 320, 400, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Potions</BASEFONT></BODY>", false, false);
            AddHtml( 320, 435, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Unknown Potions</BASEFONT></BODY>", false, false);
            AddHtml( 320, 470, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Bard Songs</BASEFONT></BODY>", false, false);

            string[] eachLoot = mySettings.Split('#');
			int nLine = 1;
			foreach (string eachLoots in eachLoot)
			{
                if (nLine == 1 && eachLoots == "0"){ AddButton(105, 225, 3609, 3609, 99, GumpButtonType.Reply, 1); } else if (nLine == 1){  AddButton(105, 225, 4018, 4018, 99, GumpButtonType.Reply, 1); }
                if (nLine == 2 && eachLoots == "0"){ AddButton(105, 260, 3609, 3609, 1, GumpButtonType.Reply, 1); } else if (nLine == 2){ AddButton(105, 260, 4018, 4018, 1, GumpButtonType.Reply, 1); }
                if (nLine == 3 && eachLoots == "0"){ AddButton(105, 295, 3609, 3609, 2, GumpButtonType.Reply, 1); } else if (nLine == 3){ AddButton(105, 295, 4018, 4018, 2, GumpButtonType.Reply, 1); }
                if (nLine == 4 && eachLoots == "0"){ AddButton(105, 330, 3609, 3609, 3, GumpButtonType.Reply, 1); } else if (nLine == 4){ AddButton(105, 330, 4018, 4018, 3, GumpButtonType.Reply, 1); }
                if (nLine == 5 && eachLoots == "0"){ AddButton(105, 365, 3609, 3609, 4, GumpButtonType.Reply, 1); } else if (nLine == 5){ AddButton(105, 365, 4018, 4018, 4, GumpButtonType.Reply, 1); }
                if (nLine == 6 && eachLoots == "0"){ AddButton(105, 400, 3609, 3609, 5, GumpButtonType.Reply, 1); } else if (nLine == 6){ AddButton(105, 400, 4018, 4018, 5, GumpButtonType.Reply, 1); }
                if (nLine == 7 && eachLoots == "0"){ AddButton(105, 435, 3609, 3609, 6, GumpButtonType.Reply, 1); } else if (nLine == 7){ AddButton(105, 435, 4018, 4018, 6, GumpButtonType.Reply, 1); }
                if (nLine == 8 && eachLoots == "0"){ AddButton(105, 470, 3609, 3609, 7, GumpButtonType.Reply, 1); } else if (nLine == 8){ AddButton(105, 470, 4018, 4018, 7, GumpButtonType.Reply, 1); }
                if (nLine == 9 && eachLoots == "0"){ AddButton(465, 225, 3609, 3609, 8, GumpButtonType.Reply, 1); } else if (nLine == 9){ AddButton(465, 225, 4018, 4018, 8, GumpButtonType.Reply, 1); }
                if (nLine == 10 && eachLoots == "0"){ AddButton(465, 260, 3609, 3609, 9, GumpButtonType.Reply, 1); } else if (nLine == 10){ AddButton(465, 260, 4018, 4018, 9, GumpButtonType.Reply, 1); }
                if (nLine == 11 && eachLoots == "0"){ AddButton(465, 295, 3609, 3609, 10, GumpButtonType.Reply, 1); } else if (nLine == 11){ AddButton(465, 295, 4018, 4018, 10, GumpButtonType.Reply, 1); }
                if (nLine == 12 && eachLoots == "0"){ AddButton(465, 330, 3609, 3609, 11, GumpButtonType.Reply, 1); } else if (nLine == 12){ AddButton(465, 330, 4018, 4018, 11, GumpButtonType.Reply, 1); }
                if (nLine == 13 && eachLoots == "0"){ AddButton(465, 365, 3609, 3609, 12, GumpButtonType.Reply, 1); } else if (nLine == 13){ AddButton(465, 365, 4018, 4018, 12, GumpButtonType.Reply, 1); }
                if (nLine == 14 && eachLoots == "0"){ AddButton(465, 400, 3609, 3609, 13, GumpButtonType.Reply, 1); } else if (nLine == 14){ AddButton(465, 400, 4018, 4018, 13, GumpButtonType.Reply, 1); }
                if (nLine == 15 && eachLoots == "0"){ AddButton(465, 435, 3609, 3609, 14, GumpButtonType.Reply, 1); } else if (nLine == 15){ AddButton(465, 435, 4018, 4018, 14, GumpButtonType.Reply, 1); }
                if (nLine == 16 && eachLoots == "0"){ AddButton(465, 470, 3609, 3609, 15, GumpButtonType.Reply, 1); } else if (nLine == 16){ AddButton(465, 470, 4018, 4018, 15, GumpButtonType.Reply, 1); }
                if (nLine == 17 && eachLoots == "0"){ AddButton(105, 510, 3609, 3609, 16, GumpButtonType.Reply, 1); } else if (nLine == 17){ AddButton(105, 510, 4018, 4018, 16, GumpButtonType.Reply, 1); }
                nLine++;
			}

			AddButton(465, 510, 4005, 4007, ButtonFilterPage2, GumpButtonType.Reply, 1);
			AddHtml( 490, 510, 60, 21, @"<BODY><BASEFONT Color=#FBFBFB>Page 2</BASEFONT></BODY>", false, false);
		}

		private void BuildFilterPageTwo(string mySettings)
		{
			AddHtml( 145, 225, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Parchments & Maps</BASEFONT></BODY>", false, false);
			AddHtml( 145, 260, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Bottles</BASEFONT></BODY>", false, false);
			AddHtml( 145, 295, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Random Notes</BASEFONT></BODY>", false, false);
			AddHtml( 145, 330, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Books</BASEFONT></BODY>", false, false);
			AddHtml( 145, 365, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Seeds & Dirt</BASEFONT></BODY>", false, false);
			AddHtml( 145, 400, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Blank Scrolls</BASEFONT></BODY>", false, false);
			AddHtml( 145, 435, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Body Parts</BASEFONT></BODY>", false, false);
			AddHtml( 145, 470, 220, 21, @"<BODY><BASEFONT Color=#FCFF00>Bones</BASEFONT></BODY>", false, false);
			AddHtml( 320, 225, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Unid Gear</BASEFONT></BODY>", false, false);
			AddHtml( 320, 260, 130, 21, @"<BODY><BASEFONT Color=#FCFF00>Unid Jewelry</BASEFONT></BODY>", false, false);

			string[] eachLoot = mySettings.Split('#');
			int nLine = 1;
			foreach (string eachLoots in eachLoot)
			{
				if (nLine == 18 && eachLoots == "0"){ AddButton(105, 225, 3609, 3609, 17, GumpButtonType.Reply, 1); } else if (nLine == 18){ AddButton(105, 225, 4018, 4018, 17, GumpButtonType.Reply, 1); }
				if (nLine == 19 && eachLoots == "0"){ AddButton(105, 260, 3609, 3609, 18, GumpButtonType.Reply, 1); } else if (nLine == 19){ AddButton(105, 260, 4018, 4018, 18, GumpButtonType.Reply, 1); }
				if (nLine == 20 && eachLoots == "0"){ AddButton(105, 295, 3609, 3609, 19, GumpButtonType.Reply, 1); } else if (nLine == 20){ AddButton(105, 295, 4018, 4018, 19, GumpButtonType.Reply, 1); }
				if (nLine == 21 && eachLoots == "0"){ AddButton(105, 330, 3609, 3609, 20, GumpButtonType.Reply, 1); } else if (nLine == 21){ AddButton(105, 330, 4018, 4018, 20, GumpButtonType.Reply, 1); }
				if (nLine == 22 && eachLoots == "0"){ AddButton(105, 365, 3609, 3609, 21, GumpButtonType.Reply, 1); } else if (nLine == 22){ AddButton(105, 365, 4018, 4018, 21, GumpButtonType.Reply, 1); }
				if (nLine == 23 && eachLoots == "0"){ AddButton(105, 400, 3609, 3609, 22, GumpButtonType.Reply, 1); } else if (nLine == 23){ AddButton(105, 400, 4018, 4018, 22, GumpButtonType.Reply, 1); }
				if (nLine == 24 && eachLoots == "0"){ AddButton(105, 435, 3609, 3609, 23, GumpButtonType.Reply, 1); } else if (nLine == 24){ AddButton(105, 435, 4018, 4018, 23, GumpButtonType.Reply, 1); }
				if (nLine == 25 && eachLoots == "0"){ AddButton(105, 470, 3609, 3609, 24, GumpButtonType.Reply, 1); } else if (nLine == 25){ AddButton(105, 470, 4018, 4018, 24, GumpButtonType.Reply, 1); }
				if (nLine == 26 && eachLoots == "0"){ AddButton(465, 225, 3609, 3609, 25, GumpButtonType.Reply, 1); } else if (nLine == 26){ AddButton(465, 225, 4018, 4018, 25, GumpButtonType.Reply, 1); }
				if (nLine == 27 && eachLoots == "0"){ AddButton(465, 260, 3609, 3609, 26, GumpButtonType.Reply, 1); } else if (nLine == 27){ AddButton(465, 260, 4018, 4018, 26, GumpButtonType.Reply, 1); }
				nLine++;
			}

			AddButton(465, 510, 4014, 4016, ButtonFilterPage1, GumpButtonType.Reply, 1);
			AddHtml( 490, 510, 60, 21, @"<BODY><BASEFONT Color=#FBFBFB>Page 1</BASEFONT></BODY>", false, false);
		}
    
		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID == ButtonTabA ){ from.SendGump( new LootChoices( from, m_Origin, 1, m_FilterPage ) ); return; }
			else if ( info.ButtonID == ButtonTabB ){ from.SendGump( new LootChoices( from, m_Origin, 2, m_FilterPage ) ); return; }
			else if ( info.ButtonID == ButtonTabC ){ from.SendGump( new LootChoices( from, m_Origin, 3, m_FilterPage ) ); return; }
			else if ( info.ButtonID == ButtonContainerA ) { LootChoiceUpdates.SetLootContainer( from, m_Origin, 1 ); return; }
			else if ( info.ButtonID == ButtonContainerB ) { LootChoiceUpdates.SetLootContainer( from, m_Origin, 2 ); return; }
			else if ( info.ButtonID == ButtonContainerC ) { LootChoiceUpdates.SetLootContainer( from, m_Origin, 3 ); return; }
			else if ( info.ButtonID == ButtonFilterPage2 ) { from.SendGump( new LootChoices( from, m_Origin, m_OutputIndex, 2 ) ); return; }
			else if ( info.ButtonID == ButtonFilterPage1 ) { from.SendGump( new LootChoices( from, m_Origin, m_OutputIndex, 1 ) ); return; }
			else if ( info.ButtonID == 99 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 1 ); }
			else if ( info.ButtonID == 1 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 2 ); }
			else if ( info.ButtonID == 2 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 3 ); }
			else if ( info.ButtonID == 3 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 4 ); }
			else if ( info.ButtonID == 4 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 5 ); }
			else if ( info.ButtonID == 5 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 6 ); }
			else if ( info.ButtonID == 6 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 7 ); }
			else if ( info.ButtonID == 7 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 8 ); }
			else if ( info.ButtonID == 8 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 9 ); }
			else if ( info.ButtonID == 9 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 10 ); }
			else if ( info.ButtonID == 10 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 11 ); }
			else if ( info.ButtonID == 11 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 12 ); }
			else if ( info.ButtonID == 12 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 13 ); }
			else if ( info.ButtonID == 13 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 14 ); }
			else if ( info.ButtonID == 14 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 15 ); }
			else if ( info.ButtonID == 15 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 16 ); }
			else if ( info.ButtonID == 16 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 17 ); }
			else if ( info.ButtonID == 17 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 18 ); }
			else if ( info.ButtonID == 18 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 19 ); }
			else if ( info.ButtonID == 19 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 20 ); }
			else if ( info.ButtonID == 20 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 21 ); }
			else if ( info.ButtonID == 21 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 22 ); }
			else if ( info.ButtonID == 22 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 23 ); }
			else if ( info.ButtonID == 23 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 24 ); }
			else if ( info.ButtonID == 24 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 25 ); }
			else if ( info.ButtonID == 25 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 26 ); }
			else if ( info.ButtonID == 26 ){ LootChoiceUpdates.UpdateLootChoice( from, m_OutputIndex, 27 ); }

			if ( info.ButtonID < 1 && m_Origin > 0 ){ from.SendSound( 0x4A ); from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) ); }
			else if ( info.ButtonID < 1 ){ }
			else { from.SendGump( new LootChoices( from, m_Origin, m_OutputIndex, m_FilterPage ) ); from.SendSound( 0x4A ); }
		}
    }
}
