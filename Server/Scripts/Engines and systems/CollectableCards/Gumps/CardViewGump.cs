using System;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Gumps
{
    public class CardViewGump : Gump
    {
        private CollectableCard m_Card;
        private Mobile m_From;

        public CardViewGump(CollectableCard card, Mobile from) : base(100, 100)
        {
            m_Card = card;
            m_From = from;

            if (m_From != null && !m_From.Deleted)
                m_From.PlaySound(CardSystemConfig.CardOpenSound);

            BuildGump();
        }

        private void BuildGump()
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            CardRenderer.RenderCard(this, 0, 0, m_Card);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Card == null || m_Card.Deleted)
                return;

            Mobile from = sender.Mobile;

            if (info.ButtonID == 0)
            {
                if (from != null && !from.Deleted)
                    from.PlaySound(CardSystemConfig.CardCloseSound);
                return;
            }
        }
    }
}
