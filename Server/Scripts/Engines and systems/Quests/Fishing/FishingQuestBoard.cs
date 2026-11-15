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
using Server.Commands;

namespace Server.Items
{
	[Flipable(0x1E5F, 0x1E5E)]
	public class FishingQuestBoard : Item
	{
		[Constructable]
		public FishingQuestBoard() : base(0x1E5E)
		{
			Weight = 1.0;
			Name = "Seeking Brave Sailors";
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( e.InRange( this.GetWorldLocation(), 4 ) )
			{
				e.CloseGump( typeof( FishingQuestBoardGump ) );
				e.SendGump( new FishingQuestBoardGump( e ) );
			}
			else
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new SpeechGumpEntry( from ) );
			list.Add( new FishingQuestEntry( from ) );
			list.Add( new FishingQuestComplete( from ) ); 
		} 

		public class FishingQuestBoardGump : Gump
		{
			private const int HireButtonId = 1;
			private const int DoneButtonId = 2;

			public FishingQuestBoardGump( Mobile from ): base( 25, 25 )
			{
				string description = SpeechFunctions.SpeechText( from.Name, from.Name, "FishQuestBoard" );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(0, 0, 151);
				AddImage(300, 0, 151);
				AddImage(0, 300, 151);
				AddImage(300, 300, 151);
				AddImage(600, 0, 151);
				AddImage(600, 300, 151);
				AddImage(2, 2, 129);
				AddImage(302, 2, 129);
				AddImage(598, 2, 129);
				AddImage(2, 298, 129);
				AddImage(301, 298, 129);
				AddImage(598, 298, 129);
				AddImage(8, 11, 145);
				AddImage(267, 19, 141);
				AddImage(473, 19, 141);
				AddImage(698, 7, 146);
				AddImage(219, 14, 143);
				AddImage(249, 31, 159);
				AddImage(50, 260, 161);
				AddImage(853, 210, 161);
				AddImage(853, 257, 161);
				AddImage(854, 554, 157);
				AddImage(51, 554, 157);
				AddItem(179, 49, 7775);
				AddHtml( 234, 72, 428, 27, @"<BODY><BASEFONT Color=#FBFBFB><BIG>SEEKING BRAVE SAILORS</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 100, 155, 737, 418, @"<BODY><BASEFONT Color=#FCFF00><BIG>" + description + "</BIG></BASEFONT></BODY>", (bool)false, (bool)true);
				AddButton(100, 385, 4005, 4005, HireButtonId, GumpButtonType.Reply, 0);
				AddHtml( 140, 385, 300, 21, @"<BODY><BASEFONT Color=#FCFF00><BIG>Hire Me</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(100, 410, 4005, 4005, DoneButtonId, GumpButtonType.Reply, 0);
				AddHtml( 140, 410, 300, 21, @"<BODY><BASEFONT Color=#FCFF00><BIG>Done</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;

				switch ( info.ButtonID )
				{
					case HireButtonId:
						HandleHire( from );
						break;
					case DoneButtonId:
						HandleDone( from );
						break;
				}
			}
		}

		public class SpeechGumpEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			
			public SpeechGumpEntry( Mobile from ) : base( 1024, 3 )
			{
				m_Mobile = from;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				{
					if ( ! mobile.HasGump( typeof( SpeechGump ) ) )
					{
						mobile.SendGump(new SpeechGump( "Seeking Brave Sailors", SpeechFunctions.SpeechText( m_Mobile.Name, m_Mobile.Name, "FishQuestBoard" ) ));
					}
				}
            }
        }

		public class FishingQuestEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			
			public FishingQuestEntry( Mobile from ) : base( 6120, 12 )
			{
				m_Mobile = from;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;

				HandleHire( m_Mobile );
            }
        }

		public class FishingQuestComplete : ContextMenuEntry
		{
			private Mobile m_Mobile;
			
			public FishingQuestComplete( Mobile from ) : base( 548, 12 )
			{
				m_Mobile = from;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;

				HandleDone( m_Mobile );
            }
        }

		private static void HandleHire( Mobile mobile )
		{
			string myQuest = CharacterDatabase.GetQuestInfo( mobile, "FishingQuest" );

			int nAllowedForAnotherQuest = FishingQuestFunctions.QuestTimeNew( mobile );
			int nServerQuestTimeAllowed = MyServerSettings.GetTimeBetweenQuests();
			int nWhenForAnotherQuest = nServerQuestTimeAllowed - nAllowedForAnotherQuest;
			string sAllowedForAnotherQuest = nWhenForAnotherQuest.ToString();

			if ( CharacterDatabase.GetQuestState( mobile, "FishingQuest" ) )
			{
				mobile.PrivateOverheadMessage(MessageType.Regular, 1150, false, "You are already on a quest. Return here when you are done.", mobile.NetState);
			}
			else if ( nWhenForAnotherQuest > 0 )
			{
				mobile.PrivateOverheadMessage(MessageType.Regular, 1150, false, "There are no quests at the moment. Check back in " + sAllowedForAnotherQuest + " minutes.", mobile.NetState);
			}
			else
			{
				int nFame = mobile.Fame * 2;
					nFame = Utility.RandomMinMax( 0, nFame )+2000;

				FishingQuestFunctions.FindTarget( mobile, nFame );

				string TellQuest = FishingQuestFunctions.QuestStatus( mobile ) + ".";
				mobile.PrivateOverheadMessage(MessageType.Regular, 1150, false, TellQuest, mobile.NetState);
			}
		}

		private static void HandleDone( Mobile mobile )
		{
			string myQuest = CharacterDatabase.GetQuestInfo( mobile, "FishingQuest" );

			int nSucceed = FishingQuestFunctions.DidQuest( mobile );

			if ( nSucceed > 0 )
			{
				FishingQuestFunctions.PayAdventurer( mobile );
			}
			else if ( myQuest.Length > 0 )
			{
				if ( ! mobile.HasGump( typeof( SpeechGump ) ) )
				{
					mobile.SendGump(new SpeechGump( "Your Reputation Is At Stake", SpeechFunctions.SpeechText( mobile.Name, mobile.Name, "FishQuestBoardFail" ) ));
				}
			}
			else
			{
				mobile.PrivateOverheadMessage(MessageType.Regular, 1150, false, "You are not currently on a quest.", mobile.NetState);
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is Gold )
			{
				int nPenalty = FishingQuestFunctions.QuestFailure( from );

				if ( dropped.Amount == nPenalty )
				{
					CharacterDatabase.ClearQuestInfo( from, "FishingQuest" );
					from.PrivateOverheadMessage(MessageType.Regular, 1153, false, "Someone else will eventually take care of this.", from.NetState);
					dropped.Delete();
				}
				else
				{
					from.AddToBackpack ( dropped );
				}
			}
			else
			{
				from.AddToBackpack ( dropped );
			}
			return true;
		}

		public FishingQuestBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
