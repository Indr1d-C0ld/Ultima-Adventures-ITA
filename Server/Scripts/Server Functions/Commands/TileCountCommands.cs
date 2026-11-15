using System;
using Server;
using Server.Commands;
using Server.Targeting;

namespace Server
{
	public class TileCountCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register("Tilecount", AccessLevel.GameMaster, new CommandEventHandler(Tilecount_OnCommand));
			CommandSystem.Register("Tilecountsq", AccessLevel.GameMaster, new CommandEventHandler(Tilecountsq_OnCommand));
		}

		[Usage("Tilecount")]
		[Description("Measures distance between two targeted points in tiles.")]
		public static void Tilecount_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Target the first location to measure distance.");
			e.Mobile.Target = new TileCountTarget();
		}

		[Usage("Tilecountsq")]
		[Description("Measures the tile count of a targeted rectangular area.")]
		public static void Tilecountsq_OnCommand(CommandEventArgs e)
		{
			BoundingBoxPicker.Begin(e.Mobile, new BoundingBoxCallback(Tilecountsq_Callback), null);
		}

		private static void Tilecountsq_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
		{
			int width = end.X - start.X + 1;
			int height = end.Y - start.Y + 1;
			int total = width * height;

			from.SendMessage("Area: {0}x{1} tiles ({2} total).", width, height, total);
		}

		private class TileCountTarget : Target
		{
			private readonly Point3D m_First;
			private readonly bool m_FirstSet;
			private readonly Map m_Map;

			public TileCountTarget() : this(Point3D.Zero, false, null)
			{
			}

			private TileCountTarget(Point3D first, bool firstSet, Map map) : base(-1, true, TargetFlags.None)
			{
				m_First = first;
				m_FirstSet = firstSet;
				m_Map = map;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				IPoint3D point = targeted as IPoint3D;

				if (point == null)
					return;

				Map map = from.Map;

				Item item = targeted as Item;

				if (item != null)
				{
					point = item.GetWorldTop();
					map = item.Map;
				}
				else
				{
					Mobile mobile = targeted as Mobile;

					if (mobile != null)
						map = mobile.Map;
				}

				Point3D location = new Point3D(point);

				if (!m_FirstSet)
				{
					from.SendMessage("Target another location to complete the measurement.");
					from.Target = new TileCountTarget(location, true, map);
					return;
				}

				if (map == null || map == Map.Internal || m_Map == null || m_Map == Map.Internal)
				{
					from.SendMessage("Both locations must reside on the same map.");
					return;
				}

				if (map != m_Map)
				{
					from.SendMessage("Both locations must reside on the same map.");
					return;
				}

				int dx = Math.Abs(location.X - m_First.X);
				int dy = Math.Abs(location.Y - m_First.Y);
				double distance = Math.Sqrt((dx * dx) + (dy * dy));

				from.SendMessage("Distance: {0:0.##} tiles (dx {1}, dy {2}).", distance, dx, dy);
			}
		}
	}
}
