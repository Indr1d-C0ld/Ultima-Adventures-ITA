using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Spells;

namespace Server.Regions
{
	public class GuardedRegion : BaseRegion, IHarvestRespawnRegion
	{
		private bool m_HasHarvestRespawnOverride;
		private TimeSpan m_HarvestMinRespawn;
		private TimeSpan m_HarvestMaxRespawn;

		public GuardedRegion( XmlElement xml, Map map, Region parent ) : base( xml, map, parent )
		{
			m_HasHarvestRespawnOverride = ReadHarvestRespawn( xml, out m_HarvestMinRespawn, out m_HarvestMaxRespawn );
		}

		public bool HasHarvestRespawnOverride { get { return m_HasHarvestRespawnOverride; } }
		public TimeSpan HarvestMinRespawn { get { return m_HarvestMinRespawn; } }
		public TimeSpan HarvestMaxRespawn { get { return m_HarvestMaxRespawn; } }
	}
}
