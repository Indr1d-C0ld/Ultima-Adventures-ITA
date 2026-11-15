namespace Server.Items
{
	public abstract class RucksackBase : LargeSack
	{
		public override int MaxWeight
		{
			get
			{
				if (IsSecure) return 0;

				var container = RootParentEntity as Container;
				if (container != null && (container.IsSecure || container is BankBox)) return 0;

				return 800;
			}
		}

		public virtual int WeightReduction { get { return 75; } }
		public abstract string FailureMessage { get; }

		protected RucksackBase(string name) : base()
		{
			Weight = 1.0;
			MaxItems = 50;
			Name = name;
		}

		public RucksackBase(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			// Warning: For backwards compatability, do not put anything here. 
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			// Warning: For backwards compatability, do not put anything here. 
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight, WeightReduction);
		}

		public override void UpdateTotal(Item sender, TotalType type, int delta)
		{
			InvalidateProperties();

			base.UpdateTotal(sender, type, delta);
		}

		public override int GetTotal(TotalType type)
		{
			int total = base.GetTotal(type);
			if (type != TotalType.Weight) return total;

			return (int)(total * (100 - (double)WeightReduction) / 100);
		}

		public override void AddItem(Item dropped)
		{
			base.AddItem(dropped);

			InvalidateWeight();
		}

		public override void RemoveItem(Item dropped)
		{
			base.RemoveItem(dropped);

			InvalidateWeight();
		}

		public override bool OnStackAttempt(Mobile from, Item stack, Item dropped)
		{
			if (!base.OnStackAttempt(from, stack, dropped)) return false;

			// Stack with item inside Rucksack
			InvalidateWeight();

			return true;
		}

		public override bool OnDragDropInto(Mobile from, Item dropped, Point3D p)
		{
			if (CanAdd(from, dropped)) return base.OnDragDropInto(from, dropped, p);

			from.SendMessage(FailureMessage);

			return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (CanAdd(from, dropped))
			{
				if (!base.OnDragDrop(from, dropped)) return false;

				// Dropped on Rucksack
				InvalidateWeight();

				return true;
			}

			from.SendMessage(FailureMessage);

			return false;
		}

		public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
		{
			if (!base.TryDropItem(from, dropped, sendFullMessage)) return false;

			if (!CanAdd(from, dropped))
			{
				if (from.Backpack == null) return false;

				from.Backpack.TryDropItem(from, dropped, false);
			}

			return true;
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			return base.CanAdd(from, item) && (item is RucksackBase) == false;
		}

		public void InvalidateWeight()
		{
			if (RootParent is Mobile)
			{
				Mobile m = (Mobile)RootParent;

				m.UpdateTotals();

				// Seems hacky. Oh well.
				if (m.Backpack != null)
				{
					m.Backpack.InvalidateProperties();
				}
			}
		}
	}
}