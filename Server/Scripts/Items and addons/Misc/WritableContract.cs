using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public class WritableContract : Item
	{
		private const int MaxTextLength = 2000;

		private string m_Text;

		[CommandProperty( AccessLevel.GameMaster )]
		public string ContractText
		{
			get { return m_Text; }
			set { m_Text = value; InvalidateProperties(); }
		}


		[Constructable]
		public WritableContract() : base( 0x14F0 )
		{
			Weight = 1.0;
			Name = "contract";
			m_Text = String.Empty;
		}

		public WritableContract( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsAccessibleTo( from ) )
			{
				from.SendLocalizedMessage( 1061637 ); // You are not allowed to access this.
				return;
			}

			from.CloseGump( typeof( WritableContractGump ) );
			from.SendGump( new WritableContractGump( from, this ) );
			from.PlaySound( 0x249 );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			writer.Write( m_Text );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
					m_Text = reader.ReadString();
					break;
			}

			if ( m_Text == null )
				m_Text = String.Empty;
		}

		private class WritableContractGump : Gump
		{
			private const int SaveButtonID = 1;
			private const int TextEntryID = 0;

			private readonly Mobile m_From;
			private readonly WritableContract m_Contract;

			public WritableContractGump( Mobile from, WritableContract contract ) : base( 150, 50 )
			{
				m_From = from;
				m_Contract = contract;

				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;

				AddPage( 0 );
				AddImage( 46, 26, 1247 );
				AddTextEntry( 102, 58, 284, 225, 0x481, TextEntryID, m_Contract.ContractText );
				AddButton( 326, 286, 4005, 4007, SaveButtonID, GumpButtonType.Reply, 0 );
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				if ( m_Contract == null || m_Contract.Deleted || m_From == null || m_From.Deleted )
					return;

				if ( info.ButtonID == SaveButtonID )
				{
					TextRelay relay = info.GetTextEntry( TextEntryID );
					string text = relay == null || relay.Text == null ? String.Empty : relay.Text;

					if ( text.Length > MaxTextLength )
						text = text.Substring( 0, MaxTextLength );

					m_Contract.ContractText = text;
					m_From.PlaySound( 0x249 );
				}
			}
		}
	}
}
