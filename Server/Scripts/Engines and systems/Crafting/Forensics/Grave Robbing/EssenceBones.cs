using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class EssenceBones : Item
	{
		private Type m_mob;

		[CommandProperty(AccessLevel.GameMaster)]
		public Type Mob
		{
			get { return m_mob; }
			set { m_mob = value; }
		}

        private bool m_Identified;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get { return m_Identified; }
            set { m_Identified = value; InvalidateProperties(); }
        }

		public static int[] m_Bone = new int[]
		{
			6921, 6922, 6923, 6924, 6925, 6926, 6927, 6928, 6929, 6930, 6931, 6932,
			6933, 6934, 6935, 6936, 6937, 6938, 6939, 6940, 6880, 6881, 6882, 6883,
			6884
		};

		[Constructable]
		public EssenceBones() : base(6921)
		{
			m_mob = typeof(Chicken);
			Name = "Essence Bones";
			ItemID = m_Bone[Utility.Random(m_Bone.Length)];
			Weight = 0.1;
		}

		[Constructable]
		public EssenceBones(Type moob) : base(6921)
		{
			m_mob = moob;
			Name = "Essence Bones";
			ItemID = m_Bone[Utility.Random(m_Bone.Length)];
			Weight = 0.1;
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			list.Add("The bones emanate a strange essence.");
			list.Add("Place many of them on the ground to see what may happen.");

			if (m_Identified && m_mob != null)
			{
				string mobName = m_mob.Name;
				list.Add("These bear the essence of a slain " + mobName + ".");
			}
			else
			{
				list.Add("Unknown essence.");
			}
		}

		public EssenceBones(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1); // version 1

			writer.Write(m_Identified); // v1
			writer.Write(m_mob != null ? m_mob.FullName : String.Empty);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			if (version >= 1)
			{
				m_Identified = reader.ReadBool();
				string full = reader.ReadString();
				m_mob = String.IsNullOrEmpty(full) ? null : ScriptCompiler.FindTypeByFullName(full);
			}
			else
			{
				// v0 compatibility: only type string was written
				string full = reader.ReadString();
				m_mob = String.IsNullOrEmpty(full) ? null : ScriptCompiler.FindTypeByFullName(full);
				m_Identified = false;
			}
		}

	}
}