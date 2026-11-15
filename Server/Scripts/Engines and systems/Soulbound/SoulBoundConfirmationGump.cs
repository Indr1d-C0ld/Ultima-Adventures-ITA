using Server.Mobiles;
using Server.Gumps;
using System;

namespace Server.Engines.Soulbound
{
	public class SoulBoundConfirmationGump : Gump
	{
		private readonly PlayerMobile m_From;

		public SoulBoundConfirmationGump(PlayerMobile from) : base(25, 25)
		{
			m_From = from;

			from.CloseGump(typeof(SoulBoundConfirmationGump));

			const string COLOR_ORANGE = "#FFA200";
			const string COLOR_YELLOW = "#FCFF00";
			const string COLOR_RED = "#FF0000";

			const int WIDTH = 480;
			const int HEIGHT = 320;
			AddImageTiled(0, 0, WIDTH, HEIGHT, 154); // Trim - Red block
			AddImageTiled(1, 1, WIDTH - 2, HEIGHT - 2, 163); // Background - Black block

			AddImage(7, 7, 145); // TL Overlay - Knights
			AddImageTiled(166, 7, 90, 30, 156); // Background - Black block (30x30)

			AddImage(282, 9, 146); // TR Overlay - Dragon
			AddImageTiled(282, 24, 45, 30, 156); // Background - Black block (30x30)

			// Title
			AddHtml(167, 122, 322, 100, @"<BODY><BASEFONT Color=" + COLOR_YELLOW + "><BIG>A new journey - " + m_From.SbTemplate.Name + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

			// Body
			const string message = "You feel an energy within you begin to grow. Will you embrace your new legacy with renewed vigor?";
			AddHtml(97, 152, WIDTH - 160, 100, @"<BODY><BASEFONT Color=" + COLOR_YELLOW + "><BIG>" + message + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml(97, 220, 322, 200, @"<BODY><BASEFONT Color=" + COLOR_RED + "><BIG>" + "Re-rolling wil cause the gods will weaken your body." + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

			int buttonY = 270;
			AddButton(100, buttonY, 4005, 4005, 1, GumpButtonType.Reply, 0);
			AddHtml(137, buttonY, 100, 20, @"<BODY><BASEFONT Color=" + COLOR_ORANGE + "><BIG>I am ready</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
			AddButton(308, buttonY, 4020, 4020, 2, GumpButtonType.Reply, 0);
			AddHtml(346, buttonY, 58, 20, @"<BODY><BASEFONT Color=" + COLOR_RED + "><BIG>Re-roll</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 0) return;

			switch (info.ButtonID)
			{
				case 1:
					m_From.ResetPlayer(false);
					break;

				case 2:
					var oldTemplate = m_From.SbTemplate;
					do
					{
						m_From.SbTemplate = Template.GetRandomTemplate();
					} while (m_From.SbTemplate.GetType() == oldTemplate.GetType());
					m_From.SbRerolls += 1;

					Timer.DelayCall(TimeSpan.FromMilliseconds(500), () =>
					{
						if (m_From.Alive) return;

						m_From.SendGump(new SoulBoundConfirmationGump(m_From));
					});
					break;
			}
		}
	}
}
