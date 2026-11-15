using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    public class WagonDriver : Citizens
    {
        private static readonly Dictionary<string, WagonStop> m_Stops = new Dictionary<string, WagonStop>(StringComparer.OrdinalIgnoreCase);

        private string m_StopName;
        private int m_TravelCost;
        private TimeSpan m_DepartureInterval;
        private TimeSpan m_DockedDuration;
        private List<Serial> m_LinkedDrivers;

        [CommandProperty(AccessLevel.GameMaster)]
        public string StopName
        {
            get { return m_StopName; }
            set
            {
                string oldName = m_StopName;
                m_StopName = value;

                if (!StringComparer.OrdinalIgnoreCase.Equals(oldName, m_StopName))
                    RemoveStop(oldName);

                UpdateStopRegistration();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TravelCost
        {
            get { return m_TravelCost; }
            set { m_TravelCost = Math.Max(0, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DepartureInterval
        {
            get { return m_DepartureInterval; }
            set { m_DepartureInterval = value <= TimeSpan.Zero ? TimeSpan.Zero : value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DockedDuration
        {
            get { return m_DockedDuration; }
            set { m_DockedDuration = value < TimeSpan.Zero ? TimeSpan.Zero : value; }
        }

        public List<Serial> LinkedDrivers
        {
            get
            {
                if (m_LinkedDrivers == null)
                    m_LinkedDrivers = new List<Serial>();

                return m_LinkedDrivers;
            }
        }

        public override bool ClickTitle { get { return false; } }

        [Constructable]
        public WagonDriver() : base()
        {
            m_StopName = "Crossroads";
            m_TravelCost = 300;
            m_DepartureInterval = TimeSpan.FromMinutes(15);
            m_DockedDuration = TimeSpan.Zero;

            CantWalk = true;
            Blessed = true;
            Fame = 0;
            Karma = 0;

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
            }

            Hue = Server.Misc.RandomThings.GetRandomSkinColor();
            SpeechHue = Server.Misc.RandomThings.GetSpeechHue();

            Title = "the wagon driver";

            Utility.AssignRandomHair(this);

            AddItem(new Tunic());
            AddItem(new LongPants());
            AddItem(new Boots());
            AddItem(new WideBrimHat());
        }

        public WagonDriver(Serial serial) : base(serial)
        {
        }

        public static List<WagonDriver> GetWagonDrivers()
        {
            List<WagonDriver> drivers = new List<WagonDriver>();

            foreach (Mobile mobile in World.Mobiles.Values)
            {
                WagonDriver driver = mobile as WagonDriver;

                if (driver != null && !driver.Deleted)
                    drivers.Add(driver);
            }

            drivers.Sort((a, b) => string.Compare(a.StopName, b.StopName, StringComparison.OrdinalIgnoreCase));

            return drivers;
        }

        public static Map NormalizeFacet(Map map)
        {
            return map == null || map == Map.Internal ? null : map;
        }

        public override void OnAfterSpawn()
        {
            base.OnAfterSpawn();
            Blessed = true;
            CantWalk = true;
            UpdateStopRegistration();
        }

        protected override void OnMapChange(Map oldMap)
        {
            base.OnMapChange(oldMap);
            UpdateStopRegistration();
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            UpdateStopRegistration();
        }

        public virtual bool IsInvulnerable
        {
            get { return true; }
        }

        public override void Delete()
        {
            RemoveStop();
            RemoveConnectionsTo(Serial);
            base.Delete();
        }

        private static void RemoveConnectionsTo(Serial target)
        {
            foreach (WagonDriver driver in GetWagonDrivers())
            {
                if (driver.LinkedDrivers.Remove(target))
                    driver.InvalidateProperties();
            }
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            return from.InRange(this, 4);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (e.Handled || !e.Mobile.InRange(this, 4))
                return;

            if (Insensitive.Contains(e.Speech, "wagon") || Insensitive.Contains(e.Speech, "coach") || Insensitive.Contains(e.Speech, "caravan") || Insensitive.Contains(e.Speech, "travel"))
            {
                e.Handled = true;
                OfferTravel(e.Mobile);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this, 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            OfferTravel(from);
        }

        public override bool OnBeforeDeath()
        {
            Say("In Vas Mani");
            Hits = HitsMax;
            FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
            PlaySound(0x202);
            return false;
        }

        private static WagonDriver FindDriver(Serial serial)
        {
            Mobile mobile = World.FindMobile(serial);
            return mobile as WagonDriver;
        }

        private void EnsureValidLinks()
        {
            if (m_LinkedDrivers == null || m_LinkedDrivers.Count == 0)
                return;

            Map facet = NormalizeFacet(Map);

            m_LinkedDrivers.RemoveAll(serial =>
            {
                WagonDriver driver = FindDriver(serial);

                return driver == null || driver.Deleted || driver == this || NormalizeFacet(driver.Map) != facet;
            });
        }

        public bool IsLinkedTo(WagonDriver driver)
        {
            if (driver == null)
                return false;

            EnsureValidLinks();
            return LinkedDrivers.Contains(driver.Serial);
        }

        public void SetLink(WagonDriver driver, bool linked)
        {
            if (driver == null || driver == this)
                return;

            EnsureValidLinks();

            if (NormalizeFacet(driver.Map) != NormalizeFacet(Map))
                return;

            if (linked)
            {
                if (!LinkedDrivers.Contains(driver.Serial))
                    LinkedDrivers.Add(driver.Serial);
            }
            else
            {
                LinkedDrivers.Remove(driver.Serial);
            }
        }

        public bool TrySendPassenger(Mobile from, WagonStop stop)
        {
            if (stop == null || Deleted || from == null)
                return false;

            if (!from.InRange(this, 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return false;
            }

            TimeSpan wait = GetTimeUntilDeparture();

            if (wait > TimeSpan.Zero)
            {
                SayTo(from, "The next wagon departs in {0}. I'll collect your fare when we depart.", FormatInterval(wait));

                ScheduleDepartureNotices(from, stop, wait);
                Timer.DelayCall(GetTimeUntilArrival(), () => AnnounceArrival(from));
                Timer.DelayCall(wait, () => AnnounceArrivalAndDepart(from, stop));
                return true;
            }

            return DepartPassenger(from, stop);
        }

        private void OfferTravel(Mobile from)
        {
            if (from == null || Deleted)
                return;

            List<WagonStop> stops = GetAvailableStops();

            from.CloseGump(typeof(FerryTravelGump));
            from.CloseGump(typeof(WagonTravelGump));
            from.SendGump(new WagonTravelGump(this, stops));

            if (stops.Count == 0)
                SayTo(from, "I'm afraid no other wagons are travelling right now.");
            else
                SayTo(from, "Where would you like to travel?");
        }

        private void UpdateStopRegistration()
        {
            string name = (m_StopName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name) || Map == null || Map == Map.Internal)
            {
                RemoveStop();
                return;
            }

            WagonStop stop = new WagonStop(name, Location, Map, this);
            m_Stops[name] = stop;
        }

        public void RegisterStop()
        {
            UpdateStopRegistration();
        }

        private void RemoveStop(string nameOverride = null)
        {
            string name = (nameOverride ?? m_StopName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                return;

            WagonStop stop;

            if (m_Stops.TryGetValue(name, out stop) && stop.Driver == this)
                m_Stops.Remove(name);
        }

        public TimeSpan GetTimeUntilDeparture()
        {
            TimeSpan arrivalWait = GetTimeUntilArrival();
            return arrivalWait + m_DockedDuration;
        }

        private TimeSpan GetTimeUntilArrival()
        {
            if (m_DepartureInterval <= TimeSpan.Zero)
                return TimeSpan.Zero;

            DateTime now = DateTime.UtcNow;
            long remainder = now.Ticks % m_DepartureInterval.Ticks;

            if (remainder == 0)
                return TimeSpan.Zero;

            return TimeSpan.FromTicks(m_DepartureInterval.Ticks - remainder);
        }

        private bool DepartPassenger(Mobile from, WagonStop stop)
        {
            if (stop == null || Deleted || from == null)
                return false;

            if (stop.Driver == null || stop.Driver.Deleted)
            {
                SayTo(from, "That wagon is not available right now.");
                return false;
            }

            if (!from.InRange(this, 6))
            {
                SayTo(from, "You missed the wagon. Please stay close if you still wish to travel.");
                return false;
            }

            if (stop.Map != Map)
            {
                SayTo(from, "This wagon only travels within this facet.");
                return false;
            }

            if (!TryChargeFare(from))
                return false;

            from.MoveToWorld(stop.Location, stop.Map);
            from.PlaySound(0x1FC);

            SayTo(from, "Safe travels to {0}!", stop.Name);
            return true;
        }

        private bool TryChargeFare(Mobile from)
        {
            if (m_TravelCost <= 0)
                return true;

            if (from.Backpack != null && from.Backpack.ConsumeTotal(typeof(Gold), m_TravelCost))
                return true;

            if (Banker.Withdraw(from, m_TravelCost))
                return true;

            SayTo(from, "A passage will cost you {0} gold.", m_TravelCost);
            return false;
        }

        private void ScheduleDepartureNotices(Mobile from, WagonStop stop, TimeSpan wait)
        {
            if (from == null || stop == null || Deleted)
                return;

            List<TimeSpan> milestones = new List<TimeSpan>();
            HashSet<long> added = new HashSet<long>();

            Action<TimeSpan> addMilestone = remaining =>
            {
                if (remaining <= TimeSpan.Zero || remaining >= wait)
                    return;

                if (added.Add(remaining.Ticks))
                    milestones.Add(remaining);
            };

            int fullMinutes = (int)Math.Floor(wait.TotalMinutes);

            for (int m = fullMinutes; m >= 1; m--)
                addMilestone(TimeSpan.FromMinutes(m));

            TimeSpan[] finalSeconds = new[]
            {
                TimeSpan.FromSeconds(45),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(5)
            };

            foreach (TimeSpan mark in finalSeconds)
            {
                if (wait > mark)
                    addMilestone(mark);
            }

            milestones.Sort((a, b) => b.CompareTo(a));

            foreach (TimeSpan remaining in milestones)
            {
                TimeSpan delay = wait - remaining;
                TimeSpan captured = remaining;

                Timer.DelayCall(delay, () => AnnounceDeparture(from, captured));
            }
        }

        private void AnnounceDeparture(Mobile from, TimeSpan remaining)
        {
            if (from == null || from.Deleted || Deleted)
                return;

            if (!from.InRange(this, 8))
                return;

            SayTo(from, "The wagon departs in {0}.", FormatInterval(remaining));
        }

        private void AnnounceArrival(Mobile from)
        {
            if (from == null || from.Deleted || Deleted)
                return;

            if (!from.InRange(this, 8))
                return;

            if (m_DockedDuration > TimeSpan.Zero)
                SayTo(from, "A wagon has arrived! Departing in {0}.", FormatInterval(m_DockedDuration));
            else
                SayTo(from, "A wagon has arrived!");
        }

        private void AnnounceArrivalAndDepart(Mobile from, WagonStop stop)
        {
            if (from != null && !from.Deleted && !Deleted && from.InRange(this, 8) && m_DockedDuration <= TimeSpan.Zero)
                SayTo(from, "A wagon has arrived!");

            DepartPassenger(from, stop);
        }

        public static string FormatInterval(TimeSpan interval)
        {
            if (interval.TotalMinutes >= 1)
            {
                int minutes = (int)Math.Floor(interval.TotalMinutes);
                int seconds = interval.Seconds;

                if (seconds > 0)
                    return String.Format("{0} minute{1} and {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s");

                return String.Format("{0} minute{1}", minutes, minutes == 1 ? "" : "s");
            }

            return String.Format("{0} second{1}", interval.Seconds, interval.Seconds == 1 ? "" : "s");
        }

        private List<WagonStop> GetAvailableStops()
        {
            List<WagonStop> stops = new List<WagonStop>();

            foreach (WagonStop stop in m_Stops.Values)
            {
                if (stop.Driver == this || stop.Driver == null || stop.Driver.Deleted)
                    continue;

                if (stop.Map != Map)
                    continue;

                if (!IsLinkedTo(stop.Driver))
                    continue;

                stops.Add(stop);
            }

            stops.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return stops;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write(m_StopName);
            writer.Write(m_TravelCost);
            writer.Write(m_DepartureInterval);
            writer.Write(m_DockedDuration);

            writer.Write(LinkedDrivers.Count);

            foreach (Serial serial in LinkedDrivers)
                writer.Write(serial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_StopName = reader.ReadString();
            m_TravelCost = reader.ReadInt();

            if (version >= 2)
            {
                m_DepartureInterval = reader.ReadTimeSpan();
                m_DockedDuration = reader.ReadTimeSpan();
            }
            else
            {
                m_DepartureInterval = TimeSpan.FromMinutes(15);
                m_DockedDuration = TimeSpan.Zero;
            }

            if (version >= 1)
            {
                int count = reader.ReadInt();

                for (int i = 0; i < count; i++)
                    LinkedDrivers.Add(reader.ReadInt());
            }

            Blessed = true;
            CantWalk = true;

            Timer.DelayCall(TimeSpan.Zero, UpdateStopRegistration);
        }

        public class WagonStop
        {
            public WagonStop(string name, Point3D location, Map map, WagonDriver driver)
            {
                Name = name;
                Location = location;
                Map = map;
                Driver = driver;
            }

            public string Name { get; private set; }
            public Point3D Location { get; private set; }
            public Map Map { get; private set; }
            public WagonDriver Driver { get; private set; }
        }
    }

    public class WagonTravelGump : Gump
    {
        private readonly WagonDriver m_Driver;
        private readonly List<WagonDriver.WagonStop> m_Stops;

        public WagonTravelGump(WagonDriver driver, List<WagonDriver.WagonStop> stops) : base(50, 50)
        {
            m_Driver = driver;
            m_Stops = stops ?? new List<WagonDriver.WagonStop>();

            int height = 130 + (Math.Max(1, m_Stops.Count) * 25);

            AddPage(0);
            AddBackground(0, 0, 320, height, 9270);
            AddAlphaRegion(10, 10, 300, height - 20);

            string title = string.IsNullOrWhiteSpace(m_Driver.StopName) ? "Wagon" : m_Driver.StopName;
            AddHtml(20, 20, 280, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Wagon Destinations</BASEFONT></CENTER>", false, false);
            AddLabel(20, 45, 0x481, String.Format("Departing: {0}", title));

            if (m_Driver.TravelCost > 0)
                AddLabel(20, 65, 0x481, String.Format("Fare: {0} gold", m_Driver.TravelCost));
            else
                AddLabel(20, 65, 0x481, "Fare: Free ride");

            TimeSpan wait = m_Driver.GetTimeUntilDeparture();
            AddLabel(20, 85, 0x481, String.Format("Next departure in: {0}", wait == TimeSpan.Zero ? "Departing now" : WagonDriver.FormatInterval(wait)));

            if (m_Stops.Count == 0)
            {
                AddLabel(20, 95, 0x480, "No other wagons are available.");
                return;
            }

            int y = 115;

            for (int i = 0; i < m_Stops.Count; i++)
            {
                WagonDriver.WagonStop stop = m_Stops[i];

                AddButton(20, y, 4005, 4007, 1 + i, GumpButtonType.Reply, 0);
                AddLabel(54, y, 0x480, stop.Name);
                y += 25;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Driver == null || m_Driver.Deleted)
                return;

            int index = info.ButtonID - 1;

            if (index < 0 || index >= m_Stops.Count)
                return;

            WagonDriver.WagonStop stop = m_Stops[index];
            m_Driver.TrySendPassenger(from, stop);
        }
    }
}
