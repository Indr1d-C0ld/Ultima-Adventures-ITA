using System;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Misc;
using Server.Network;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Regions;
using Server.Commands;
using System.Text;

namespace Server.Mobiles
{
    public class TownHerald : BasePerson
	{
		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }

		public override bool ClickTitle{ get{ return false; } }

		[Constructable]
		public TownHerald() : base( )
		{
			NameHue = -1;

			InitStats( 100, 100, 25 );

			Title = "the town crier";
			Hue = Server.Misc.RandomThings.GetRandomSkinColor();

			AddItem( new FancyShirt( Utility.RandomBlueHue() ) );

			Item skirt;

			switch ( Utility.Random( 2 ) )
			{
				case 0: skirt = new Skirt(); break;
				default: case 1: skirt = new Kilt(); break;
			}

			skirt.Hue = Utility.RandomGreenHue();

			AddItem( skirt );

			AddItem( new FeatheredHat( Utility.RandomGreenHue() ) );

			Item boots;

			switch ( Utility.Random( 2 ) )
			{
				case 0: boots = new Boots(); break;
				default: case 1: boots = new ThighBoots(); break;
			}

			AddItem( boots );
			AddItem( new LightCitizen( true ) );

			Utility.AssignRandomHair( this );
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			Region reg = Region.Find( this.Location, this.Map );
			if ( DateTime.UtcNow >= m_NextTalk && InRange( m, 10 ) && m is PlayerMobile )
			{

				if (AdventuresFunctions.InfectedRegions.Count > 0)
				{
					if (AetherGlobe.carrier != null && Utility.RandomDouble() > 0.70)
						Say("I've heard word of a being named " + AetherGlobe.carrier + " spreading a mysterious infection in the lands!");
					else
						Say("Infected have been seen in the lands! Help is needed!");
				}
				else if ( LoggingFunctions.LoggingEvents() == true )
				{
					string sEvents = LoggingFunctions.LogShout();
					Say( sEvents );
				}
				else
				{
					Say( "All is well in the land!" );
				}
				m_NextTalk = (DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 15, 30 ) ));
			}
		}

		public override bool HandlesOnSpeech( Mobile from ) 
		{ 
			return true; 
		} 

                public override void OnSpeech( SpeechEventArgs e )
                {
                        if( e.Mobile.InRange( this, 4 ))
                        {
                                if ( Insensitive.Contains( e.Speech, "infected") )
                                {
                                        TalkInfection( e.Mobile );
                                }
                        }

                }

                public void TalkInfection( Mobile from )
                {
                        PlayerMobile player = from as PlayerMobile;

                        if ( player == null )
                                return;

                        Say( "*unfurls a scroll*" );
                        PlaySound( 0x249 );

                        Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), () =>
                        {
                                if ( Deleted || player.Deleted )
                                        return;

                                Say( "By the order of lord british these foul creatures are to be hunted to the last!" );

                                Timer.DelayCall( TimeSpan.FromSeconds( 0.25 ), () =>
                                {
                                        if ( Deleted || player.Deleted )
                                                return;

                                        AdventuresFunctions.CheckInfection();
                                        List<FacetGroup> groups = BuildFacetGroups();
                                        List<string> lines = BuildDisplayLines( groups );

                                        player.CloseGump( typeof( InfectedLocationsGump ) );
                                        player.SendGump( new InfectedLocationsGump( lines ) );
                                } );
                        } );
                }

                private static List<FacetGroup> BuildFacetGroups()
                {
                        List<FacetGroup> groups = new List<FacetGroup>();

                        if ( AdventuresFunctions.InfectedRegionsByFacet == null || AdventuresFunctions.InfectedRegionsByFacet.Count == 0 )
                                return groups;

                        foreach ( KeyValuePair<string, HashSet<string>> entry in AdventuresFunctions.InfectedRegionsByFacet )
                        {
                                bool facetInfected = false;
                                List<string> locations = new List<string>();

                                foreach ( string location in entry.Value )
                                {
                                        if ( string.Equals( location, entry.Key, StringComparison.OrdinalIgnoreCase ) )
                                                facetInfected = true;
                                        else
                                                locations.Add( location );
                                }

                                locations.Sort( StringComparer.OrdinalIgnoreCase );

                                groups.Add( new FacetGroup
                                {
                                        FacetName = entry.Key,
                                        InfectedAtFacet = facetInfected,
                                        Locations = locations
                                } );
                        }

                        groups.Sort( ( left, right ) => StringComparer.OrdinalIgnoreCase.Compare( left.FacetName, right.FacetName ) );

                        return groups;
                }

                private static List<string> BuildDisplayLines( List<FacetGroup> groups )
                {
                        List<string> lines = new List<string>();

                        if ( groups.Count == 0 )
                        {
                                lines.Add( Utility.FixHtml( "Thank the balance! The infected are contained." ) );
                                return lines;
                        }

                        for ( int i = 0; i < groups.Count; i++ )
                        {
                                FacetGroup group = groups[i];
                                string facetLabel = SanitizeLocationName( group.FacetName );

                                if ( group.InfectedAtFacet )
                                        facetLabel += " *";

                                lines.Add( Utility.FixHtml( facetLabel ) );

                                foreach ( string location in group.Locations )
                                {
                                        string cleaned = SanitizeLocationName( location );
                                        lines.Add( Utility.FixHtml( "---" + cleaned ) );
                                }

                                if ( i < (groups.Count - 1) )
                                        lines.Add( string.Empty );
                        }

                        return lines;
                }

                private static string SanitizeLocationName( string value )
                {
                        if ( string.IsNullOrWhiteSpace( value ) )
                                return "an unknown location";

                        string cleaned = value.Trim();

                        if ( cleaned.StartsWith( "the land of ", StringComparison.OrdinalIgnoreCase ) )
                                cleaned = cleaned.Substring( "the land of ".Length );
                        else if ( cleaned.StartsWith( "the ", StringComparison.OrdinalIgnoreCase ) )
                                cleaned = cleaned.Substring( 4 );

                        cleaned = cleaned.Trim();

                        return string.IsNullOrWhiteSpace( cleaned ) ? "an unknown location" : cleaned;
                }

                private class FacetGroup
                {
                        public string FacetName { get; set; }
                        public bool InfectedAtFacet { get; set; }
                        public List<string> Locations { get; set; }
                }

                private class InfectedLocationsGump : Gump
                {
                        public InfectedLocationsGump( List<string> lines ) : base( 100, 100 )
                        {
                                Closable = true;
                                Disposable = true;
                                Dragable = true;
                                Resizable = false;

                                AddPage( 0 );

                                const int topSegmentHeight = 37;
                                const int contentStartY = 95;
                                const int contentWidth = 260;
                                const int contentX = 40;
                                const int contentLineHeight = 18;
                                const int minimumContentHeight = 20;
                                const int bottomMargin = 40;
                                const int minimumTiledHeight = 120;
                                const int estimatedCharsPerLine = 30;

                                int lineCount = Math.Max( 1, lines.Count );
                                int estimatedLineCount = EstimateLineCount( lines, estimatedCharsPerLine );
                                int effectiveLineCount = Math.Max( lineCount, estimatedLineCount );
                                int contentHeight = Math.Max( minimumContentHeight, effectiveLineCount * contentLineHeight );
                                int tiledHeight = Math.Max( minimumTiledHeight, (contentStartY - topSegmentHeight) + contentHeight + bottomMargin );
                                int bottomImageY = topSegmentHeight + tiledHeight;

                                AddImage( 0, 0, 0x1F40 );
                                AddImageTiled( 20, topSegmentHeight, 300, tiledHeight, 0x1F42 );
                                AddImage( 20, bottomImageY, 0x1F43 );

                                AddHtml( 40, 55, 260, 24, @"<BODY><BASEFONT Color=#000000><CENTER><BIG>Infected Locations</BIG></CENTER></BASEFONT></BODY>", false, false );
                                AddImageTiled( 40, 82, 260, 2, 0x23C5 );

                                StringBuilder builder = new StringBuilder();

                                for ( int i = 0; i < lines.Count; i++ )
                                {
                                        builder.Append( lines[i] );

                                        if ( i < (lines.Count - 1) )
                                                builder.Append( "<br>" );
                                }

                                AddHtml( contentX, contentStartY, contentWidth, contentHeight, "<BODY><BASEFONT Color=#000000>" + builder.ToString() + "</BASEFONT></BODY>", false, false );
                        }

                        private static int EstimateLineCount( List<string> lines, int estimatedCharsPerLine )
                        {
                                int count = 0;

                                for ( int i = 0; i < lines.Count; i++ )
                                {
                                        string line = lines[i];
                                        int lineLength = string.IsNullOrEmpty( line ) ? 1 : line.Length;
                                        int estimatedLines = Math.Max( 1, (int)Math.Ceiling( (double)lineLength / estimatedCharsPerLine ) );

                                        count += estimatedLines;
                                }

                                return Math.Max( 1, count );
                        }
                }

        public TownHerald(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new TownHeraldEntry( from, this ) ); 
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
		
		public class TownHeraldEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private Mobile m_Giver;
			
			public TownHeraldEntry( Mobile from, Mobile giver ) : base( 6146, 3 )
			{
				m_Mobile = from;
				m_Giver = giver;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
					return;

				String[] talkEntries = {
					"Hear ye! When thou build a house in our lands, the more wood, nails, and stone thou bring to the construction site, the bigger thy house can be!",
					"Hear ye! Town vendors no longer give large bulk order deeds, thou need to use thy Shoppe and build up reputation to get them for better rewards!",
					"Hear ye! Items do not decay and disappear from the ground on their own in our realm, so don't litter unnecessarily! Please keep our lands clean!",
					"Hear ye! Provisioners now sell trapped pouches, opening them will damage thee but can be used to negate paralysis quickly! Be careful with them!",
					"Hear ye! Gravekeepers in town are not free, their service costs 20000 gold. They take thy money as soon as thou ask them to fetch thy corpse!",
					"Hear ye! Unless thou art good at lockpicking, be sure thou carry skeleton keys or bottles of acid in case thou unexpectedly face a locked door!",
				};
				
				m_Giver.Say(talkEntries[Utility.Random(talkEntries.Length)]);

				/* -- FIXME: Disabled for now, apparently could cause a client crash in the past. Not sure if it's still the case, needs extra testing --
				if (LoggingFunctions.LoggingEvents() == true)
				{
					PlayerMobile mobile = (PlayerMobile) m_Mobile;

					if (!mobile.HasGump(typeof(LoggingGumpCrier)))
					{
						mobile.SendGump(new LoggingGumpCrier(mobile, 1));
					}
				}
				else
				{
					m_Giver.Say("Good day to you, " + m_Mobile.Name + ".");
				}
				*/
			}
        }
	}  
}
