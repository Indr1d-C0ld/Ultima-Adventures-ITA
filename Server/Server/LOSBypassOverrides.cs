using System;
using System.Collections.Generic;
using System.IO;
using Server.Commands;
using Server.Targeting;

namespace Server
{
	public static class LOSBypassOverrides
	{
		private static readonly HashSet<LOSBypassEntry> m_Entries = new HashSet<LOSBypassEntry>();
		private static readonly string m_FilePath = Path.Combine(Core.BaseDirectory, "Data", "LOSBypassOverrides.bin");

		static LOSBypassOverrides()
		{
			CommandSystem.Register("ToggleLOSBypass", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
			EventSink.WorldLoad += OnWorldLoad;
			EventSink.WorldSave += OnWorldSave;

			// World.Load() occurs before this class is forced in Main, so load immediately.
			OnWorldLoad();
		}

		private static void OnCommand(CommandEventArgs e)
		{
			if (e.Length > 0 && String.Equals(e.GetString(0), "clear", StringComparison.OrdinalIgnoreCase))
			{
				ClearAll(e.Mobile);
				return;
			}

			e.Mobile.SendMessage("Target a land tile/static/item to toggle GLOBAL LOS bypass override. Use [ToggleLOSBypass clear to wipe all.");
			e.Mobile.Target = new LOSBypassTarget();
		}

		
		public static void ClearAll(Mobile from)
		{
			m_Entries.Clear();

			if (from != null)
				from.SendMessage("Cleared all GLOBAL LOS bypass overrides.");
		}

		public static bool ToggleTarget(Mobile from, object targeted)
		{
			if (targeted is StaticTarget)
			{
				StaticTarget st = (StaticTarget)targeted;
				Toggle(from, from != null ? from.Map : Map.Internal, st.Location, st.ItemID & TileData.MaxItemValue);
				return true;
			}

			Item item = targeted as Item;
			if (item != null)
			{
				Toggle(from, item.Map, item.Location, item.ItemID & TileData.MaxItemValue);
				return true;
			}

			IPoint3D p = targeted as IPoint3D;
			if (p != null && !(targeted is Mobile))
			{
				Toggle(from, from != null ? from.Map : Map.Internal, new Point3D(p), -1);
				return true;
			}

			return false;
		}


		public static bool ClearTarget(Mobile from, object targeted)
		{
			if (targeted is StaticTarget)
			{
				StaticTarget st = (StaticTarget)targeted;
				return Remove(from, from != null ? from.Map : Map.Internal, st.Location, st.ItemID & TileData.MaxItemValue);
			}

			Item item = targeted as Item;
			if (item != null)
				return Remove(from, item.Map, item.Location, item.ItemID & TileData.MaxItemValue);

			IPoint3D p = targeted as IPoint3D;
			if (p != null && !(targeted is Mobile))
				return Remove(from, from != null ? from.Map : Map.Internal, new Point3D(p), -1);

			return false;
		}

		public static bool IsIgnored(Mobile viewer, Map map, Point3D point, int staticOrItemId)
		{
			if (map == null || map == Map.Internal)
				return false;

			foreach (LOSBypassEntry entry in m_Entries)
			{
				if (entry.MapIndex != map.MapIndex || entry.X != point.X || entry.Y != point.Y)
					continue;

				if (entry.StaticOrItemID == -1 || entry.StaticOrItemID == staticOrItemId)
					return true;
			}

			return false;
		}

		private static void Toggle(Mobile from, Map map, Point3D location, int staticOrItemId)
		{
			if (from == null || map == null || map == Map.Internal)
				return;

			LOSBypassEntry entry = FindEntry(map.MapIndex, location.X, location.Y, staticOrItemId);

			if (!entry.IsEmpty)
			{
				m_Entries.Remove(entry);
				from.SendMessage("GLOBAL LOS bypass override removed at {0},{1},{2} map {3} id {4}.", location.X, location.Y, location.Z, map.Name, staticOrItemId);
			}
			else
			{
				m_Entries.Add(new LOSBypassEntry(map.MapIndex, location.X, location.Y, location.Z, staticOrItemId));
				from.SendMessage("GLOBAL LOS bypass override added at {0},{1},{2} map {3} id {4}.", location.X, location.Y, location.Z, map.Name, staticOrItemId);
			}
		}



		private static bool Remove(Mobile from, Map map, Point3D location, int staticOrItemId)
		{
			if (map == null || map == Map.Internal)
				return false;

			LOSBypassEntry entry = FindEntry(map.MapIndex, location.X, location.Y, staticOrItemId);
			if (entry.IsEmpty)
				return false;

			m_Entries.Remove(entry);
			if (from != null)
				from.SendMessage("GLOBAL LOS bypass override cleared at {0},{1},{2} map {3} id {4}.", location.X, location.Y, location.Z, map.Name, staticOrItemId);
			return true;
		}

private static LOSBypassEntry FindEntry(int mapIndex, int x, int y, int staticOrItemId)
		{
			foreach (LOSBypassEntry entry in m_Entries)
			{
				if (entry.MapIndex == mapIndex && entry.X == x && entry.Y == y && entry.StaticOrItemID == staticOrItemId)
					return entry;
			}

			return LOSBypassEntry.Empty;
		}

		private static void OnWorldLoad()
		{
			m_Entries.Clear();

			if (!File.Exists(m_FilePath))
				return;

			using (FileStream fs = new FileStream(m_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (BinaryReader br = new BinaryReader(fs))
			{
				int version = br.ReadInt32();

				if (version == 1)
				{
					int count = br.ReadInt32();
					for (int i = 0; i < count; i++)
						m_Entries.Add(new LOSBypassEntry(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32()));
				}
				else if (version == 0)
				{
					// Legacy format: per-mobile dictionary. Migrate to global entries.
					int mobileCount = br.ReadInt32();
					for (int i = 0; i < mobileCount; i++)
					{
						br.ReadInt32(); // mobile serial (ignored in global mode)
						int count = br.ReadInt32();
						for (int j = 0; j < count; j++)
							m_Entries.Add(new LOSBypassEntry(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32()));
					}
				}
			}
		}

		private static void OnWorldSave(WorldSaveEventArgs e)
		{
			string dir = Path.GetDirectoryName(m_FilePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (FileStream fs = new FileStream(m_FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			using (BinaryWriter bw = new BinaryWriter(fs))
			{
				bw.Write(1);
				bw.Write(m_Entries.Count);
				foreach (LOSBypassEntry entry in m_Entries)
				{
					bw.Write(entry.MapIndex);
					bw.Write(entry.X);
					bw.Write(entry.Y);
					bw.Write(entry.Z);
					bw.Write(entry.StaticOrItemID);
				}
			}
		}

		private class LOSBypassTarget : Target
		{
			public LOSBypassTarget() : base(18, true, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (!ToggleTarget(from, targeted))
					from.SendMessage("Unsupported target for LOS bypass.");
			}
		}

		private struct LOSBypassEntry : IEquatable<LOSBypassEntry>
		{
			public static readonly LOSBypassEntry Empty = new LOSBypassEntry(int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue);
			public readonly int MapIndex, X, Y, Z, StaticOrItemID;
			public LOSBypassEntry(int mapIndex, int x, int y, int z, int staticOrItemID)
			{ MapIndex = mapIndex; X = x; Y = y; Z = z; StaticOrItemID = staticOrItemID; }
			public bool Equals(LOSBypassEntry other) { return MapIndex == other.MapIndex && X == other.X && Y == other.Y && Z == other.Z && StaticOrItemID == other.StaticOrItemID; }
			public override bool Equals(object obj) { return obj is LOSBypassEntry && Equals((LOSBypassEntry)obj); }
			public bool IsEmpty { get { return MapIndex == int.MinValue; } }
			public override int GetHashCode() { return MapIndex ^ (X << 4) ^ (Y << 8) ^ (Z << 12) ^ (StaticOrItemID << 16); }
		}
	}
}
