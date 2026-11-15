using System;
using Server; 
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Misc;
using Server.Network;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Commands;
using System.Globalization;
using Server.Regions;
using Server.Accounting;
using Server.SkillHandlers;


namespace Server.Items
{
	public class ThiefNote : Item
	{
		public const int HollowStumpType = 1;
		public const int HayCrateType = 2;

		public Mobile NoteOwner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Note_Owner { get{ return NoteOwner; } set{ NoteOwner = value; } }

		public string NoteItemCategory;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_ItemCategory { get { return NoteItemCategory; } set { NoteItemCategory = value; InvalidateProperties(); } }

		public string NoteItem;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_Item { get { return NoteItem; } set { NoteItem = value; InvalidateProperties(); } }

		public int NoteItemGot;
		[CommandProperty(AccessLevel.Owner)]
		public int Note_ItemGot { get { return NoteItemGot; } set { NoteItemGot = value; InvalidateProperties(); } }

		public string NoteItemArea;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_ItemArea { get { return NoteItemArea; } set { NoteItemArea = value; InvalidateProperties(); } }

		public string NoteItemPerson;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_ItemPerson { get { return NoteItemPerson; } set { NoteItemPerson = value; InvalidateProperties(); } }

		public int NoteDeliverType;
		[CommandProperty(AccessLevel.Owner)]
		public int Note_DeliverType { get { return NoteDeliverType; } set { NoteDeliverType = value; InvalidateProperties(); } }

		public string NoteDeliverTo;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_DeliverTo { get { return NoteDeliverTo; } set { NoteDeliverTo = value; InvalidateProperties(); } }

		public int NoteReward;
		[CommandProperty(AccessLevel.Owner)]
		public int Note_Reward { get { return NoteReward; } set { NoteReward = value; InvalidateProperties(); } }

		public int NoteFallbackReward;
		[CommandProperty(AccessLevel.Owner)]
		public int Note_FallbackReward { get { return NoteFallbackReward; } set { NoteFallbackReward = value; InvalidateProperties(); } }

		public string NoteStory;
		[CommandProperty(AccessLevel.Owner)]
		public string Note_Story { get { return NoteStory; } set { NoteStory = value; InvalidateProperties(); } }
		
		public int Consecutive;
		[CommandProperty(AccessLevel.Owner)]
		public int Con_secutive { get { return Consecutive; } set { Consecutive = value; InvalidateProperties(); } }

		[Constructable]
		public ThiefNote() : base( 0x2DD )
		{
			Name = "secret note";
			Weight = 1.0;
			Consecutive = 0;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( NoteOwner != null ){ list.Add( 1049644, "Coded for " + NoteOwner.Name + "" ); }
			if ( Consecutive > 0 ){ list.Add( 1070722, "Consecutive missions: " + Consecutive ); }
			if ( NoteItemGot > 0 ){ list.Add( 1070722, "Stolen " + NoteItem ); }
			
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( ThiefAllowed( from ) != null )
			{
				from.SendMessage( "You need a break from the last job, so read this note in about " + ThiefAllowed( from ) + " minutes." );
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else if ( NoteOwner != from )
			{
				from.SendMessage( "This note is written in a code you don't understand so you throw it away!" );
				bool remove = true;
				foreach ( Account a in Accounts.GetAccounts() )
				{
					if (a == null)
						break;

					int index = 0;

					for (int i = 0; i < a.Length; ++i)
					{
						Mobile m = a[i];

						if (m == null)
							continue;

						if ( m == NoteOwner )
						{
							m.AddToBackpack( this );
							remove = false;
						}

						++index;
					}
				}
				if ( remove )
				{
					this.Delete();
				}
			}
			else
			{
				from.PlaySound( 0x249 );
				from.CloseGump( typeof( NoteGump ) );
				from.SendGump( new NoteGump( this ) );
			}
		}

		public static void ThiefTimeAllowed( Mobile m )
		{
			DateTime TimeFinished = DateTime.UtcNow;
			string sFinished = Convert.ToString(TimeFinished);
			CharacterDatabase.SetQuestInfo( m, "ThiefQuest", sFinished );
		}

		public static int ThiefTimeNew( Mobile m )
		{
			int ThiefTime = 10000;
			string sTime = CharacterDatabase.GetQuestInfo( m, "ThiefQuest" );

			if ( sTime.Length > 0 && !( CharacterDatabase.GetQuestState( m, "ThiefQuest" ) ) )
			{
				DateTime TimeThen = Convert.ToDateTime(sTime);
				DateTime TimeNow = DateTime.UtcNow;
				long ticksThen = TimeThen.Ticks;
				long ticksNow = TimeNow.Ticks;
				int minsThen = (int)TimeSpan.FromTicks(ticksThen).TotalMinutes;
				int minsNow = (int)TimeSpan.FromTicks(ticksNow).TotalMinutes;
				ThiefTime = minsNow - minsThen;
			}
			return ThiefTime;
		}

		public static string ThiefAllowed( Mobile from )
		{
			int nAllowedForAnotherQuest = ThiefTimeNew( from );
			int nServerQuestTimeAllowed = MyServerSettings.GetTimeBetweenQuests();
			int nWhenForAnotherQuest = nServerQuestTimeAllowed - nAllowedForAnotherQuest;
			string sAllowedForAnotherQuest = nWhenForAnotherQuest.ToString();

			if ( nWhenForAnotherQuest > 0 ){ return sAllowedForAnotherQuest; }

			return null;
		}

		public ThiefNote(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 1);

			writer.Write( (Mobile)NoteOwner);
			writer.Write( NoteItemCategory );
			writer.Write( NoteItem );
			writer.Write( NoteItemGot );
			writer.Write( NoteItemArea );
			writer.Write( NoteItemPerson );
			writer.Write( NoteDeliverType );
			writer.Write( NoteDeliverTo );
			writer.Write( NoteReward );
			writer.Write( NoteStory );
			writer.Write( Consecutive );
			writer.Write( NoteFallbackReward );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			NoteOwner = reader.ReadMobile();
			NoteItemCategory = reader.ReadString();
			NoteItem = reader.ReadString();
			NoteItemGot = reader.ReadInt();
			NoteItemArea = reader.ReadString();
			NoteItemPerson = reader.ReadString();
			NoteDeliverType = reader.ReadInt();
			NoteDeliverTo = reader.ReadString();
			NoteReward = reader.ReadInt();
			NoteStory = reader.ReadString();
			Consecutive = reader.ReadInt();

			if ( version >= 1 )
			{
				NoteFallbackReward = reader.ReadInt();
			}
			else
			{
				NoteFallbackReward = NoteReward;
			}

			if ( NoteFallbackReward < 100 && NoteReward > 0 )
			{
				NoteFallbackReward = NoteReward;
			}
		}

		public class NoteGump : Gump
		{
			private ThiefNote m_Note;

			public NoteGump( ThiefNote note ) : base( 25, 25 )
			{
				m_Note = note;

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(0, 0, 9390);
				AddImage(114, 0, 9391);
				AddImage(0, 140, 9396);
				AddImage(285, 140, 9398);
				AddImage(171, 0, 9391);
				AddImage(228, 0, 9391);
				AddImage(285, 0, 9392);
				AddImage(114, 140, 9397);
				AddImage(171, 140, 9397);
				AddImage(228, 140, 9397);

				string describe = "<br><br>Keep this note with you at all times if you are going to pursue this job. If you are to steal something from a dungeon, you need to find the pedestal with the bag or box that a thief would normally attempt to steal. Use the bag or box on the pedestal to see if you successfully steal the item. If a town merchant is someone you need to pilfer from, find their coffer use your stealing skill on it to see if you steal the item from that. Be warned, you may be flagged a criminal and the guards will surely dispatch of you if caught. If you manage to escape with you life, and the item sought, then bring this note to the location specified in these instructions. If you lose this secret note, then find the thief guildmaster and they will give you a copy of the message.";

				AddHtml( 28, 40, 342, 201, @"<BODY><BASEFONT Color=#111111><BIG>" + m_Note.NoteStory + describe+ "</BIG></BASEFONT></BODY>", (bool)false, (bool)true);
			}
		}

                private static int CalculateReward(Mobile owner, double baseReward, double fallbackBase, out int fallbackReward)
                {
                        double effectiveFallback = fallbackBase;

                        if ( effectiveFallback < 100.0 )
                                effectiveFallback = 100.0;

			int rewardModifier = Server.Misc.MyServerSettings.QuestRewardModifier(owner, null);
			double rewardScale = rewardModifier * 0.01;

			double scaledBase = baseReward * rewardScale;
			double scaledFallback = rewardScale > 0.0 ? effectiveFallback * rewardScale : effectiveFallback;

			if ( double.IsNaN(scaledFallback) || double.IsInfinity(scaledFallback) || scaledFallback < 100.0 )
				scaledFallback = Math.Max(100.0, effectiveFallback);

			fallbackReward = (int)Math.Floor(scaledFallback);

			if ( fallbackReward < 100 )
				fallbackReward = 100;

			if ( double.IsNaN(scaledBase) || double.IsInfinity(scaledBase) || scaledBase <= 0.0 )
				scaledBase = fallbackReward;

			int reward = (int)Math.Floor(scaledBase);

			if ( reward < 100 )
				reward = 100;

                        return reward;
                }

                public static bool IsInAssignedRegion(Mobile mobile, string assignedRegion)
                {
                        if (mobile == null || string.IsNullOrEmpty(assignedRegion))
                                return false;

                        string currentRegion = Worlds.GetRegionName(mobile.Map, mobile.Location);

                        if (!string.IsNullOrEmpty(currentRegion) && string.Equals(currentRegion, assignedRegion, StringComparison.OrdinalIgnoreCase))
                                return true;

                        Region region = Region.Find(mobile.Location, mobile.Map);

                        if (region != null && region.IsPartOf(assignedRegion))
                                return true;

                        string currentWorld = Worlds.GetMyWorld(mobile.Map, mobile.Location, mobile.X, mobile.Y);

                        if (!string.IsNullOrEmpty(currentWorld) && string.Equals(currentWorld, assignedRegion, StringComparison.OrdinalIgnoreCase))
                                return true;

                        if (!string.IsNullOrEmpty(currentRegion) && (currentRegion.IndexOf(assignedRegion, StringComparison.OrdinalIgnoreCase) >= 0 || assignedRegion.IndexOf(currentRegion, StringComparison.OrdinalIgnoreCase) >= 0))
                                return true;

                        return false;
                }

		public static Item GetMyCurrentJob( Mobile m )
		{
			if ( m is PlayerMobile )
			{
				if (m.Backpack == null)
					return null;

				foreach ( Item item in m.Backpack.Items )
				if ( item is ThiefNote )
				{
					if ( ((ThiefNote)item).NoteOwner == m ){ return item; }
				}

			}
			return null;
		}

                public static void SetupNote( ThiefNote note, Mobile m )
                {
                        SetupNote( note, m, 0);
                }

                private static bool TryAssignTownHeist(ThiefNote note, Mobile m, int consecutive)
                {
                        int rewardMod;
                        string searchLocation = PickTownSearchLocation(m, out rewardMod);
                        List<Coffer> candidates = new List<Coffer>();

                        foreach (Item target in World.Items.Values)
                        {
                                Coffer coffer = target as Coffer;

                                if (!IsValidTownCoffer(coffer, searchLocation))
                                        continue;

                                candidates.Add(coffer);
                        }

                        if (candidates.Count == 0)
                                return false;

                        Coffer chosen = candidates[Utility.Random(candidates.Count)];
                        string area = chosen.CofferTown;
                        string category = chosen.CofferType;

                        if (string.IsNullOrEmpty(area))
                        {
                                area = Server.Misc.Worlds.GetRegionName(chosen.Map, chosen.Location);
                        }

                        if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(category))
                                return false;

                        note.NoteItemArea = area;
                        note.NoteItemCategory = category;
                        note.Consecutive = consecutive;

                        int modded = Server.Misc.AdventuresFunctions.DiminishingReturns(consecutive, 200);
                        double baseReward = (rewardMod * (int)(m.Skills[SkillName.Stealing].Value * 10)) + (Utility.RandomMinMax(1, 15) * rewardMod * (int)(m.Skills[SkillName.Stealing].Value * 2));
                        baseReward *= (1 + ((double)modded / 100));

                        double skillValue = m != null ? m.Skills[SkillName.Stealing].Value : 0;
                        double fallbackBase = Math.Max(rewardMod * 500, rewardMod * Math.Max(100, skillValue * 10));
                        int fallbackReward;
                        note.NoteReward = CalculateReward(note.NoteOwner, baseReward, fallbackBase, out fallbackReward);
                        note.NoteFallbackReward = Math.Max(fallbackReward, note.NoteReward);

                        return true;
                }

                private static bool TryAssignDungeonHeist(ThiefNote note, Mobile m, int consecutive)
                {
                        string searchLocation = PickDungeonSearchLocation(m);
                        List<StealBase> candidates = new List<StealBase>();

                        foreach (Item target in World.Items.Values)
                        {
                                StealBase stealable = target as StealBase;

                                if (stealable == null || stealable.Deleted || stealable.Map == null || stealable.Map == Map.Internal)
                                        continue;

                                if (Server.Misc.Worlds.GetMyWorld(stealable.Map, stealable.Location, stealable.X, stealable.Y) != searchLocation)
                                        continue;

                                candidates.Add(stealable);
                        }

                        if (candidates.Count == 0)
                                return false;

                        StealBase chosen = candidates[Utility.Random(candidates.Count)];
                        string area = Server.Misc.Worlds.GetRegionName(chosen.Map, chosen.Location);

                        if (string.IsNullOrEmpty(area))
                        {
                                area = searchLocation;
                        }

                        int difficult = Server.Misc.MyServerSettings.GetDifficultyLevel(chosen.Location, chosen.Map) + 2;

                        if (difficult < 2)
                        {
                                difficult = 2;
                        }

                        note.NoteItemArea = area;
                        note.NoteItemCategory = "";
                        note.Consecutive = consecutive;

                        int modded = Server.Misc.AdventuresFunctions.DiminishingReturns(consecutive, 200);
                        double baseReward = (difficult * (int)(m.Skills[SkillName.Stealing].Value * 10)) + (Utility.RandomMinMax(0, 30) * (int)(m.Skills[SkillName.Stealing].Value * 2));
                        baseReward *= (1 + ((double)modded / 100));

                        double skillValue = m != null ? m.Skills[SkillName.Stealing].Value : 0;
                        double fallbackBase = Math.Max(difficult * 500, difficult * Math.Max(100, skillValue * 10));
                        int fallbackReward;
                        note.NoteReward = CalculateReward(note.NoteOwner, baseReward, fallbackBase, out fallbackReward);
                        note.NoteFallbackReward = Math.Max(fallbackReward, note.NoteReward);

                        return true;
                }

                private static string PickTownSearchLocation(Mobile m, out int rewardMod)
                {
                        rewardMod = 1;
                        string searchLocation = "the Land of Sosaria";

                        switch (Utility.RandomMinMax(0, 14))
                        {
                                case 0:
                                case 1:
                                case 2:
                                        searchLocation = "the Land of Sosaria";
                                        break;
                                case 3:
                                case 4:
                                case 5:
                                        rewardMod = 2;
                                        searchLocation = "the Land of Lodoria";
                                        break;
                                case 6:
                                case 7:
                                case 8:
                                        rewardMod = 3;
                                        searchLocation = "the Serpent Island";
                                        break;
                                case 9:
                                        rewardMod = 4;
                                        searchLocation = "the Isles of Dread";
                                        break;
                                case 10:
                                case 11:
                                        rewardMod = 5;
                                        searchLocation = "the Savaged Empire";
                                        break;
                                case 12:
                                        rewardMod = 2;
                                        searchLocation = "the Island of Umber Veil";
                                        break;
                                case 13:
                                        rewardMod = 4;
                                        searchLocation = "the Bottle World of Kuldar";
                                        break;
                                default:
                                        rewardMod = 3;
                                        searchLocation = "DarkMoor";
                                        break;
                        }

                        if (m == null)
                                return searchLocation;

                        if (!CharacterDatabase.GetDiscovered(m, searchLocation))
                        {
                                rewardMod = 1;
                                searchLocation = "the Land of Sosaria";
                        }

                        return searchLocation;
                }

                private static string PickDungeonSearchLocation(Mobile m)
                {
                        string searchLocation = "the Land of Sosaria";

                        switch (Utility.RandomMinMax(0, 15))
                        {
                                case 0:
                                case 1:
                                case 2:
                                        searchLocation = "the Land of Sosaria";
                                        break;
                                case 3:
                                case 4:
                                case 5:
                                        searchLocation = "the Land of Lodoria";
                                        break;
                                case 6:
                                case 7:
                                case 8:
                                        searchLocation = "the Serpent Island";
                                        break;
                                case 9:
                                        searchLocation = "the Isles of Dread";
                                        break;
                                case 10:
                                case 11:
                                        searchLocation = "the Savaged Empire";
                                        break;
                                case 12:
                                        searchLocation = "the Island of Umber Veil";
                                        break;
                                case 13:
                                        searchLocation = "the Bottle World of Kuldar";
                                        break;
                                case 14:
                                        searchLocation = "the Underworld";
                                        break;
                                default:
                                        searchLocation = "the Land of Ambrosia";
                                        break;
                        }

                        if (m != null)
                        {
                                if (!CharacterDatabase.GetDiscovered(m, searchLocation))
                                {
                                        searchLocation = "the Land of Sosaria";
                                }

                                if (!CharacterDatabase.GetDiscovered(m, "the Land of Sosaria") && searchLocation == "the Land of Sosaria")
                                {
                                        if (m.Skills.Cap == 11000)
                                        {
                                                searchLocation = "the Savaged Empire";
                                        }
                                        else
                                        {
                                                searchLocation = "the Land of Lodoria";
                                        }
                                }
                        }

                        return searchLocation;
                }

                private static bool IsValidTownCoffer(Coffer coffer, string searchLocation)
                {
                        if (coffer == null || coffer.Deleted || coffer.Map == null || coffer.Map == Map.Internal)
                                return false;

                        if (Server.Misc.Worlds.GetMyWorld(coffer.Map, coffer.Location, coffer.X, coffer.Y) != searchLocation)
                                return false;

                        if (string.IsNullOrEmpty(coffer.CofferTown))
                        {
                                Server.Items.Coffer.GetNearbyTown(coffer);
                        }

                        if (string.IsNullOrEmpty(coffer.CofferTown) || string.IsNullOrEmpty(coffer.CofferType))
                                return false;

                        return true;
                }

		public static void SetupNote( ThiefNote note, Mobile m, int consecutive )
		{
			note.Hue = Utility.RandomList( 0x95E, 0x95D, 0x95B, 0x952, 0x957, 0x94D, 0x944, 0x945, 0x940, 0x93D, 0xB79 );
			note.ItemID = Utility.RandomList( 0x2DD, 0x201A );

			note.NoteOwner = m;

			note.NoteItemCategory = "";
			note.NoteItem = QuestCharacters.QuestItems( true );
			note.NoteItemGot = 0;
			note.NoteItemPerson = ContainerFunctions.GetOwner( "Pilfer" );
			note.NoteDeliverType = Utility.RandomBool() ? ThiefNote.HollowStumpType : ThiefNote.HayCrateType;
			note.NoteReward = 0;
			note.NoteFallbackReward = 0;

			bool assigned = false;

			if ( Utility.RandomBool() )
			{
				assigned = TryAssignTownHeist(note, m, consecutive);

				if ( !assigned )
				{
					assigned = TryAssignDungeonHeist(note, m, consecutive);
				}
			}
			else
			{
				assigned = TryAssignDungeonHeist(note, m, consecutive);

				if ( !assigned )
				{
					assigned = TryAssignTownHeist(note, m, consecutive);
				}
			}

			if ( !assigned )
			{
				assigned = TryAssignTownHeist(note, m, consecutive);
			}

			if ( !assigned )
			{
				assigned = TryAssignDungeonHeist(note, m, consecutive);
			}

			if ( !assigned )
			{
				string fallbackArea = null;
				if ( m != null )
				{
					fallbackArea = Server.Misc.Worlds.GetRegionName( m.Map, m.Location );
				}

				if ( string.IsNullOrEmpty( fallbackArea ) )
				{
					fallbackArea = "the Land of Sosaria";
				}

				note.NoteItemArea = fallbackArea;
				note.NoteItemCategory = "";
				note.Consecutive = consecutive;
			}
			if ( note.NoteReward <= 0 )
			{
				double skillValue = m != null ? m.Skills[SkillName.Stealing].Value : 0;
				double fallbackBase = Math.Max(500.0, Math.Max(100.0, skillValue * 10.0));
				int fallbackReward;
				note.NoteReward = CalculateReward(note.NoteOwner, note.NoteReward, fallbackBase, out fallbackReward);
				note.NoteFallbackReward = Math.Max(note.NoteFallbackReward, Math.Max(fallbackReward, note.NoteReward));
			}

			string dropLocation = "the Land of Sosaria";
			switch ( Utility.RandomMinMax( 0, 14 ) )
			{
				case 0:		dropLocation = "the Land of Sosaria";			break;
				case 1:		dropLocation = "the Land of Sosaria";			break;
				case 2:		dropLocation = "the Land of Sosaria";			break;
				case 3:		dropLocation = "the Land of Lodoria";			if ( !( CharacterDatabase.GetDiscovered( m, "the Land of Lodoria" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 4:		dropLocation = "the Land of Lodoria";			if ( !( CharacterDatabase.GetDiscovered( m, "the Land of Lodoria" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 5:		dropLocation = "the Land of Lodoria";			if ( !( CharacterDatabase.GetDiscovered( m, "the Land of Lodoria" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 6:		dropLocation = "the Serpent Island";			if ( !( CharacterDatabase.GetDiscovered( m, "the Serpent Island" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 7:		dropLocation = "the Serpent Island";			if ( !( CharacterDatabase.GetDiscovered( m, "the Serpent Island" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 8:		dropLocation = "the Serpent Island";			if ( !( CharacterDatabase.GetDiscovered( m, "the Serpent Island" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 9:		dropLocation = "the Isles of Dread";			if ( !( CharacterDatabase.GetDiscovered( m, "the Isles of Dread" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 10:	dropLocation = "the Savaged Empire";			if ( !( CharacterDatabase.GetDiscovered( m, "the Savaged Empire" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 11:	dropLocation = "the Savaged Empire";			if ( !( CharacterDatabase.GetDiscovered( m, "the Savaged Empire" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 12:	dropLocation = "the Island of Umber Veil";		if ( !( CharacterDatabase.GetDiscovered( m, "the Island of Umber Veil" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 13:	dropLocation = "the Bottle World of Kuldar";	if ( !( CharacterDatabase.GetDiscovered( m, "the Bottle World of Kuldar" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
				case 14:	dropLocation = "DarkMoor";						if ( !( CharacterDatabase.GetDiscovered( m, "DarkMoor" ) ) ){ dropLocation = "the Land of Sosaria"; } break;
			}

			if ( !( CharacterDatabase.GetDiscovered( m, "the Land of Sosaria" ) ) && dropLocation == "the Land of Sosaria" )
			{
				if ( m.Skills.Cap == 11000 ){ dropLocation = "the Savaged Empire"; }
				else { dropLocation = "the Land of Lodoria"; }
			}

			int dCount = 0;
			ArrayList drops = new ArrayList();
			foreach ( Item target in World.Items.Values )
			if ( ( (note.NoteDeliverType == ThiefNote.HollowStumpType && target is HollowStump) || (note.NoteDeliverType == ThiefNote.HayCrateType && target is HayCrate) ) && Worlds.GetMyWorld( target.Map, target.Location, target.X, target.Y ) == dropLocation )
			{
				drops.Add( target ); dCount++;
			}

			dCount = Utility.RandomMinMax( 1, dCount );

			int sCount = 0;
			for ( int i = 0; i < drops.Count; ++i )
			{
				sCount++;

				if ( sCount == dCount )
				{
					Item finding = ( Item )drops[ i ];

					if ( finding is HayCrate )
					{
						HayCrate hay = (HayCrate)finding;
						if (hay.HayTown != null && hay.HayTown != "")
							note.NoteDeliverTo = hay.HayTown;
						else 
							note.NoteDeliverTo = "the City of Britain";
							
					}
					else if ( finding is HollowStump )
					{
						HollowStump stump = (HollowStump)finding;
						if (stump.StumpTown != null && stump.StumpTown != "")
							note.NoteDeliverTo = stump.StumpTown;
						else 
							note.NoteDeliverTo = "the City of Britain";
					}
				}
			}

			string action = "recover";
				switch( Utility.RandomMinMax( 0, 4 ) )
				{
					case 0: action = "recover"; break;
					case 1: action = "steal"; break;
					case 2: action = "acquire"; break;
					case 3: action = "find"; break;
					case 4: action = "get"; break;
				}

			string drop = "drop";
				switch( Utility.RandomMinMax( 0, 4 ) )
				{
					case 0: drop = "leave"; break;
					case 1: drop = "place"; break;
					case 2: drop = "set"; break;
					case 3: drop = "put"; break;
					case 4: drop = "drop"; break;
				}

			string container = "crate of hay in";
			if ( note.NoteDeliverType == ThiefNote.HollowStumpType ){ container = "hollow stump near"; }

			string location = note.NoteItemArea;
				if ( note.NoteItemCategory != "" && note.NoteItemCategory != null ){ location = "the " + note.NoteItemCategory + " in " + note.NoteItemArea; }

			note.NoteStory = note.NoteItemPerson + " wants you to " + action + " " + note.NoteItem + " from " + location + ".";
			note.NoteStory = note.NoteStory + " Once you have it, " + drop + " it in the " + container + " " + note.NoteDeliverTo + ".";
			note.NoteStory = note.NoteStory + " There you will also find your payment of " + note.NoteReward + " gold and instructions for your next job.";

			note.InvalidateProperties();
		}

		public static bool TryGetReward(Mobile from, ThiefNote envelope, string town, int noteType)
		{
			if (envelope.NoteOwner == from && envelope.NoteItemGot > 0 && town == envelope.NoteDeliverTo && envelope.NoteDeliverType == noteType)
			{
				int modded = Server.Misc.AdventuresFunctions.DiminishingReturns(envelope.Consecutive, 200);
				envelope.Consecutive += 1;
				LoggingFunctions.LogStandard(from, "has stolen " + envelope.NoteItem + ".");

				int payout = envelope.NoteReward;

				if ( payout <= 0 )
				{
					int fallbackReward = envelope.NoteFallbackReward;
					double skillValue = from != null ? from.Skills[SkillName.Stealing].Value : 0;
					int difficulty = Math.Max(2, Server.Misc.MyServerSettings.GetDifficultyLevel(from.Location, from.Map) + 2);
					double fallbackBase = Math.Max(difficulty * 500, difficulty * Math.Max(100, skillValue * 10));

					if ( fallbackReward <= 0 )
					{
						payout = CalculateReward(from, 0.0, fallbackBase, out fallbackReward);
					}
					else
					{
						payout = fallbackReward;
					}

					payout = Math.Max(payout, fallbackReward);
					envelope.NoteReward = payout;
					envelope.NoteFallbackReward = Math.Max(envelope.NoteFallbackReward, payout);
				}

				AetherGlobe.QuestEffect(payout, from, false);

				if (payout < 5000 && payout > 0)
					from.AddToBackpack(new Gold(payout));
				else if (payout > 0)
					from.AddToBackpack(new BankCheck(payout));

                                PlayerMobile player = from as PlayerMobile;
                                if (player != null && player.SoulBound)
                                {
                                        int soulForceReward = (int)(payout * 0.25);

                                        if (soulForceReward > 0)
                                        {
                                                int cap = MyServerSettings.SoulForceCap();
                                                if (player.SoulForce + soulForceReward > cap)
                                                {
                                                        soulForceReward = cap - player.SoulForce;
                                                }

                                                if (soulForceReward > 0)
                                                {
                                                        player.SoulForce += soulForceReward;
                                                        player.SendMessage( 0, string.Format("You convert part of the reward into {0} soul force.", soulForceReward) );
                                                }
                                        }
                                }

				Titles.AwardFame(from, ((int)(payout / 25)), true);
				Titles.AwardKarma(from, -((int)(payout / 25)), true);
				Server.Items.ThiefNote.SetupNote(envelope, from, envelope.Consecutive);
				from.LocalOverheadMessage(MessageType.Emote, 1150, true, "You collected your reward.");
				from.SendMessage("You found another secret note for you.");
				from.SendSound(0x3D);
				from.CloseGump(typeof(Server.Items.ThiefNote.NoteGump));
				Server.Items.ThiefNote.ThiefTimeAllowed(from);
				Item rngitem = null;

				if (envelope.Consecutive == 50 || envelope.Consecutive == 100 || envelope.Consecutive == 150 || envelope.Consecutive == 200 || envelope.Consecutive == 250 || envelope.Consecutive == 300 || envelope.Consecutive == 350)
					rngitem = Loot.RandomArty();

				else if (rngitem == null && Utility.RandomDouble() < (0.03 + ((double)modded / 300)))
				{
					switch (Utility.Random(6)) //
					{
						case 0: rngitem = Loot.RandomArty(); break;
						case 1: rngitem = Loot.RandomArmorOrShieldOrWeaponOrJewelryOrClothing(); Stealing.ItemMutate(from, from.Luck, rngitem, (int)(modded / 10)); break;
						case 2: rngitem = Loot.RandomInstrument(); Stealing.ItemMutate(from, from.Luck, rngitem, (int)(modded / 10)); break;
						case 3: rngitem = Loot.RandomQuiver(); Stealing.ItemMutate(from, from.Luck, rngitem, (int)(modded / 10)); break;
						case 4: rngitem = Loot.RandomWand(); Stealing.ItemMutate(from, from.Luck, rngitem, (int)(modded / 10)); break;
						case 5: rngitem = Loot.RandomJewelry(); Stealing.ItemMutate(from, from.Luck, rngitem, (int)(modded / 10)); break;
					}
					if (rngitem != null)
					{
						from.AddToBackpack(rngitem);
						from.SendMessage("The guild offers you a special gift.  May it help you stay in the shadows.");
					}
				}

				return true;
			}

			return false;
		}
	}
}
