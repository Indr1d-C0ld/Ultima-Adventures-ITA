using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;

namespace Server.Items
{
	public class MagicPigment : Item, IDyePreviewSource
	{
		private const int DefaultHueValue = 99999;
		private int _hue;
		private bool _grandfathered;

		[Constructable]
		public MagicPigment() : base(0x4C5A)
		{
			string OwnerName = RandomThings.GetRandomName();
			if (OwnerName.EndsWith("s"))
			{
				OwnerName = OwnerName + "'";
			}
			else
			{
				OwnerName = OwnerName + "'s";
			}

			string ItemName = LootPackEntry.MagicItemAdj("start", false, false, ItemID);

			Weight = 2.0;
			Name = OwnerName + " " + ItemName + " paints";

			_hue = DefaultHueValue;
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			list.Add(1070722, "Paint Almost Anything");
			if (_hue == DefaultHueValue || _grandfathered)
			{
				list.Add(1049644, "Needs Color Added By Dyeing It");
				list.Add(1049644, "Can only be colored once, choose wisely.");
			}
			else
			{
				list.Add(1049644, "Has been colored.");
				list.Add(1049644, "The color cannot change.");
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			BeginTargeting(from);
		}

		public Item SourceItem { get { return this; } }
		public int PreviewHue { get { return Hue == 0x2EF ? 0 : Hue; } }

		public bool CanBeginPreview(Mobile from, out string failureMessage)
		{
			failureMessage = null;

			if (Deleted || from == null || from.Backpack == null || !IsChildOf(from.Backpack))
			{
				failureMessage = "The pigment must be in your backpack to use it.";
				return false;
			}

			return true;
		}

		public bool CanPreviewTarget(Mobile from, Item target, int selectedHue, out string failureMessage)
		{
			failureMessage = null;

			if (!DyePreviewUtility.IsOwnedGear(from, target))
			{
				failureMessage = "You may only paint equipment you are wearing or carrying in your backpack.";
				return false;
			}

			if (target.Stackable || target.ItemID == 8702 || target.ItemID == 4011)
			{
				failureMessage = "You cannot paint that.";
				return false;
			}

			return true;
		}

		public bool CanApply(Mobile from, Item target, int selectedHue, out string failureMessage)
		{
			return CanBeginPreview(from, out failureMessage)
				&& CanPreviewTarget(from, target, selectedHue, out failureMessage);
		}

		public bool ApplyPermanentDye(Mobile from, Item target, int selectedHue)
		{
			string failureMessage;

			if (!CanApply(from, target, selectedHue, out failureMessage))
			{
				if (from != null && !String.IsNullOrEmpty(failureMessage))
					from.SendMessage(failureMessage);

				return false;
			}

			target.Hue = selectedHue;
			from.RevealingAction();
			from.PlaySound(0x23F);
			return true;
		}

		public bool IsSourceStillValid(Mobile from)
		{
			string failureMessage;
			return CanBeginPreview(from, out failureMessage);
		}

		public string GetSourceDisplayName()
		{
			return !String.IsNullOrEmpty(Name) ? Name : "magic pigment";
		}

		public string GetConsumptionMessage()
		{
			return null;
		}

		public void BeginTargeting(Mobile from)
		{
			string failureMessage;

			if (!CanBeginPreview(from, out failureMessage))
			{
				if (from != null)
					from.SendLocalizedMessage(1060640);

				return;
			}

			from.SendMessage("What do you want to paint?");
			from.Target = new DyeTarget(this);
		}

		private class DyeTarget : Target
		{
			private MagicPigment m_Dye;

			public DyeTarget(MagicPigment tube) : base(1, false, TargetFlags.None)
			{
				m_Dye = tube;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item)
				{
					Item iDye = targeted as Item;

					if (DyePreviewUtility.IsWearableGear(iDye))
					{
						DyePreviewSession.Begin(from, m_Dye, iDye);
					}
					else if (!iDye.IsChildOf(from.Backpack))
					{
						from.SendMessage("You can only paint things in your pack.");
					}
					else if ((iDye.Stackable == true) || (iDye.ItemID == 8702) || (iDye.ItemID == 4011))
					{
						from.SendMessage("You cannot paint that.");
					}
					else if (iDye.IsChildOf(from.Backpack))
					{
						if ( targeted is MagicPigment )
						{
							MagicPigment pigment = (MagicPigment)targeted;
                            pigment.ApplyHue(from, m_Dye.Hue, 0x23F);
                        } 
						else 
						{
							iDye.Hue = m_Dye.Hue;
							if ( iDye.Hue == 0x2EF ){ iDye.Hue = 0; }
							from.RevealingAction();
							from.PlaySound( 0x23F );
						}
					}
					else
					{
						from.SendMessage("You cannot paint that with this.");
					}
				}
				else
				{
					from.SendMessage("You cannot paint that with this.");
				}
			}
		}

		public override int Hue
		{
			get
			{
                return _grandfathered || _hue == DefaultHueValue ? base.Hue : _hue;
            }
			set
			{
				// Setting the base triggers necessary events
				base.Hue = value;
			}
		}

		public bool ApplyHue(Mobile from, int targetHue, int sound)
		{
			bool applied = false;
			if (_hue == DefaultHueValue) //new item just been dyed
			{
				applied = true;
			}
			else if (_hue != DefaultHueValue && _hue != targetHue && !_grandfathered) //someone tried to change the hue, change back
			{
				this.Hue = _hue;
			}
			else if (_hue != DefaultHueValue && _hue != targetHue && _grandfathered) // grandfathered dyes, someone changed hue, set grandfathered to false
			{
				applied = true;
			}

			if (applied)
			{
				_hue = Hue = targetHue;
				_grandfathered = false;
                InvalidateProperties();
                from.RevealingAction();
				from.PlaySound(sound);
            }
			else
			{
				from.SendMessage("You cannot dye that.");
			}

			return applied;
		}

		public MagicPigment(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1); // version
			writer.Write((int)_hue);
			writer.Write((bool)_grandfathered);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			if (version == 0)
                _grandfathered = true;
            if (version >= 1)
			{
				_hue = reader.ReadInt();
				_grandfathered = reader.ReadBool();
			}
		}
	}
}
