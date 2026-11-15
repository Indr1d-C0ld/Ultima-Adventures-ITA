// Clone Character On Logout v1.1.2
// Author: Felladrin
// Started: 2016-01-25
// Updated: 2016-02-05

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Server;
using Server.Mobiles;
using Server.Regions;
using Server.Misc;
using Server.Diagnostics;

namespace Felladrin.Automations
{
    public static class CloneCharacterOnLogout
    {
        private static Dictionary<Serial, List<CharacterClone>> m_CloneIndex = new Dictionary<Serial, List<CharacterClone>>();
        private const long SlowCloneCleanupThreshold = 50;

        public static class Config
        {
            public static bool Enabled = true;      // Is this system enabled?
            public static bool CanWander = true;    // Can the clones wander freely around the world or should they be frozen?
            public static bool CanTeach = true;     // Can other player train skills with this clone?
        }

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                EventSink.Logout += OnLogout;
                EventSink.Login += OnLogin;
                CheckFirstRun();
            }
        }

        static void OnLogout(LogoutEventArgs e)
        {
            if (e.Mobile != null && e.Mobile.AccessLevel == AccessLevel.Player && Utility.RandomDouble() > 0.33 && e.Mobile.RawStatTotal > CharacterClone.MinimumTotalStats)
                CreateCloneOf(e.Mobile);
        }

        static void OnLogin(LoginEventArgs e)
        {
            if (e.Mobile != null && e.Mobile.AccessLevel == AccessLevel.Player)
                DeleteClonesOf(e.Mobile);
        }

        private static long GetElapsedMilliseconds(long start)
        {
            return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
        }

        static void RegisterClone(CharacterClone clone)
        {
            if (clone == null || clone.Deleted || clone.Original == null)
                return;

            Serial serial = clone.Original.Serial;
            List<CharacterClone> list;

            if (!m_CloneIndex.TryGetValue(serial, out list))
                m_CloneIndex[serial] = list = new List<CharacterClone>();

            if (!list.Contains(clone))
                list.Add(clone);
        }

        static void UnregisterClone(CharacterClone clone)
        {
            if (clone == null || clone.Original == null)
                return;

            Serial serial = clone.Original.Serial;
            List<CharacterClone> list;

            if (!m_CloneIndex.TryGetValue(serial, out list))
                return;

            list.Remove(clone);

            if (list.Count == 0)
                m_CloneIndex.Remove(serial);
        }

        static void CreateCloneOf(Mobile m)
        {
            var characterClone = new CharacterClone(m);

            foreach (var itemOriginal in m.Items)
                if (itemOriginal.Parent == m && itemOriginal.Layer != Layer.Mount)
                    characterClone.AddItem(new ItemClone(itemOriginal));

            if (m.Mounted)
            {
                var baseMount = m.Mount as BaseMount;
                var etherealMount = m.Mount as EtherealMount;

                if (baseMount != null)
                    new MountClone(baseMount).Rider = characterClone;
                else if (etherealMount != null)
                    new EtherealMountClone(etherealMount).Rider = characterClone;
            }

            characterClone.DistributeIfNecessary();
        }

        static void DeleteClonesOf(Mobile m)
        {
            if (m == null)
                return;

            long start = Stopwatch.GetTimestamp();
            int deleted = 0;
            List<CharacterClone> list;

            if (!m_CloneIndex.TryGetValue(m.Serial, out list) || list == null || list.Count == 0)
                return;

            CharacterClone[] snapshot = list.ToArray();

            for (int i = 0; i < snapshot.Length; ++i)
            {
                CharacterClone clone = snapshot[i];

                if (clone != null && !clone.Deleted && clone.Original == m)
                {
                    clone.Delete();
                    ++deleted;
                }
            }

            long elapsed = GetElapsedMilliseconds(start);

            if (elapsed >= SlowCloneCleanupThreshold)
                DiagnosticConsole.WriteParsedLine("[Lag] Clone cleanup for {0} took {1}ms; clonesDeleted={2}", m, elapsed, deleted);
        }

        static void CheckFirstRun()
        {
            // Don't clone if at least one already exists
            foreach (KeyValuePair<Serial, Mobile> entry in World.Mobiles)
                if (entry.Value is CharacterClone)
                    return;

            List<Mobile> snapshot = new List<Mobile>();

            foreach (KeyValuePair<Serial, Mobile> entry in World.Mobiles)
                snapshot.Add(entry.Value);

            foreach (Mobile mobile in snapshot)
            {
                if (mobile.RawStatTotal < CharacterClone.MinimumTotalStats) continue;

                if (mobile is PlayerMobile && mobile.Alive && mobile.AccessLevel == AccessLevel.Player)
                {
                    CreateCloneOf(mobile);
                }
            }
        }

        public class CharacterClone : BaseCreature
        {
            public const int MinimumTotalStats = 175;

            [CommandProperty(AccessLevel.GameMaster)]
            public Mobile Original { get; set; }

            public CharacterClone(Mobile original) : base(Config.CanWander ? AIType.AI_Vendor : AIType.AI_Use_Default, FightMode.None, 10, 1, 0.2, 0.2)
            {
                Original = original;

                foreach (var property in (typeof(Mobile)).GetProperties())
                    if (property.CanRead && property.CanWrite)
                        property.SetValue(this, property.GetValue(Original, null), null);

                for (int i = 0, l = Original.Skills.Length; i < l; ++i)
                    Skills[i].Base = Original.Skills[i].Base;

                Player = false;

                if (Map == Map.Internal)
                    Map = LogoutMap;
				
				Blessed = true;

                RegisterClone(this);
				
            }

            public override void GetProperties(ObjectPropertyList list)
            {
                if (Original != null)
                    Original.GetProperties(list);
                else
                    base.GetProperties(list);
            }

           // public override bool IsInvulnerable{ get{ return true; } }

			public virtual bool IsInvulnerable{ get{ return true; } }

            public override bool CanRegenHits { get { return true; } }

            public override bool CanTeach { get { return Config.CanTeach; } }

            public CharacterClone(Serial serial) : base(serial) { }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0);
                writer.Write(Original);
            }

            public override void Deserialize(GenericReader reader)
            {
                try
                {
                    base.Deserialize(reader);
                    reader.ReadInt();
                    Original = reader.ReadMobile();

                    if (Original == null)
                    {
                        Delete();
                        return;
                    }

                    RegisterClone(this);

                    DistributeIfNecessary();
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Failed to deserialize clone. {0}", e.Message);
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }

            public void DistributeIfNecessary(int daysSinceLoginThreshold = 30)
            {
                PlayerMobile pm = Original as PlayerMobile;

                if (pm == null || this.Map == null)
                    return;

                Region reg = Region.Find( this.Location, this.Map );

                if (reg == null)
                    return;

                if ( reg.IsPartOf( "the Bank" ) || reg.IsPartOf(typeof(SafeRegion) ) || reg.IsPartOf(typeof(PublicRegion)) )  
                {
                    if ( pm.LastOnline < (DateTime.UtcNow - TimeSpan.FromDays( daysSinceLoginThreshold ) ) )
                    {
                        int whatever = Utility.RandomMinMax(1, 100);
                        String world = "";
                        if (whatever <= 25) // 25%
                            world = "the Isles of Dread";
                        else if (whatever <= 30) // 5%
                            world = "the Land of Sosaria";
                        else if (whatever <= 45) // 15%
                            world = "the Land of Lodoria";
                        else if (whatever <= 65) // 20%
                            world = "the Savaged Empire";
                        else if (whatever <= 85) // 20%
                            world = "the Serpent Island";
                        else 
                            world = "the Island of Umber Veil";

                        whatever = Utility.RandomMinMax(1, 100);

                        Point3D gagme;
                        if (whatever <= 75)
                            gagme = Worlds.GetRandomTown( world, false );
                        else 
                            gagme = Worlds.GetRandomLocation( world, "land" );

                        MoveToWorld( gagme, Worlds.GetMyDefaultMap( world ) );
                    }
                }
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();
                UnregisterClone(this);
            }
        }

        public class ItemClone : Item
        {
            [CommandProperty(AccessLevel.GameMaster)]
            public Item Original { get; set; }

            public ItemClone(Item original)
            {
                Original = original;

                foreach (var property in (typeof(Item)).GetProperties())
                    if (property.CanRead && property.CanWrite)
                        property.SetValue(this, property.GetValue(original, null), null);

                Movable = false;
            }

            public override void GetProperties(ObjectPropertyList list)
            {
                if (Original != null)
                    Original.GetProperties(list);
                else
                    base.GetProperties(list);
            }

            public ItemClone(Serial serial) : base(serial) { }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0);
                writer.Write(Original);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt();
                Original = reader.ReadItem();

                if (Original == null)
                    Delete();
            }
        }

        public class MountClone : BaseMount
        {
            public MountClone(BaseMount original) : base(original.Name,  original.BodyValue, original.ItemID, original.AI, original.FightMode, original.RangePerception, original.RangeFight, original.ActiveSpeed, original.PassiveSpeed)
            {
                foreach (var property in (typeof(BaseMount)).GetProperties())
                    if (property.CanRead && property.CanWrite && property.Name != "Rider" && property.Name != "ControlMaster")
                        property.SetValue(this, property.GetValue(original, null), null);
            }

            public MountClone(Serial serial) : base(serial) { }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt();
            }
        }

        public class EtherealMountClone : EtherealMount
        {
            public EtherealMountClone(EtherealMount original) : base(original.RegularID, original.MountedID)
            {
                foreach (var property in (typeof(EtherealMount)).GetProperties())
                    if (property.CanRead && property.CanWrite && property.Name != "Rider" && property.Name != "ControlMaster")
                        property.SetValue(this, property.GetValue(original, null), null);
            }

            public EtherealMountClone(Serial serial) : base(serial) { }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt();
            }
        }
    }
}
