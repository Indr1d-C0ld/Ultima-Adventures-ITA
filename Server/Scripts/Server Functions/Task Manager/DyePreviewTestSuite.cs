using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Commands
{
	public static class DyePreviewTestSuite
	{
		public static TestResult[] Run()
		{
			List<TestResult> results = new List<TestResult>();

			try
			{
				DyePreviewSession.Initialize();
				RunOwnershipAndPacketTests( results );
				RunSessionLifecycleTests( results );
				RunSourceConsumptionTests( results );
				RunLimitedSourceTests( results );
			}
			catch ( Exception ex )
			{
				AddFailure( results, "Dye preview test execution", ex );
			}

			return results.ToArray();
		}

		private static void RunOwnershipAndPacketTests( List<TestResult> results )
		{
			PlayerMobile player = null;
			PlayerMobile other = null;
			TestWearable equipped = null;
			TestWearable packed = null;
			TestWearable ground = null;
			TestWearable otherItem = null;
			TestDyeSource backpackSource = null;

			try
			{
				player = CreatePlayer();
				other = CreatePlayer();
				equipped = new TestWearable();
				packed = new TestWearable();
				ground = new TestWearable();
				otherItem = new TestWearable();

				player.AddItem( equipped );
				player.Backpack.DropItem( packed );
				other.AddItem( otherItem );

				AddResult( results, "Equipped gear is owned", DyePreviewUtility.IsOwnedGear( player, equipped ) );
				AddResult( results, "Backpack gear is owned", DyePreviewUtility.IsOwnedGear( player, packed ) );
				AddResult( results, "Ground gear is rejected", !DyePreviewUtility.IsOwnedGear( player, ground ) );
				AddResult( results, "Another player's gear is rejected", !DyePreviewUtility.IsOwnedGear( player, otherItem ) );

				int realHue = 44;
				equipped.Hue = realHue;
				DyePreviewDisplay equippedDisplay = DyePreviewPackets.Describe( player, equipped, 1153, false );
				DyePreviewDisplay legacyDisplay = DyePreviewPackets.Describe( player, packed, 1153, false );
				DyePreviewDisplay gridDisplay = DyePreviewPackets.Describe( player, packed, 1153, true );

				AddResult( results, "Equipped packet mode selected", equippedDisplay.Mode == DyePreviewDisplayMode.Equipped );
				AddResult( results, "Legacy backpack packet mode selected", legacyDisplay.Mode == DyePreviewDisplayMode.ContainerLegacy );
				AddResult( results, "Grid backpack packet mode selected", gridDisplay.Mode == DyePreviewDisplayMode.ContainerGrid );
				AddResult( results, "Packet description keeps preview hue", equippedDisplay.Hue == 1153 );
				AddResult( results, "Preview packet has one intended recipient",
					equippedDisplay.Recipient == player && equippedDisplay.Recipient != other );
				AddResult( results, "Packet description leaves real hue unchanged", equipped.Hue == realHue );

				backpackSource = new TestDyeSource( 1167 );
				player.Backpack.DropItem( backpackSource );
				DyePreviewSession backpackSession = DyePreviewSession.Begin( player, backpackSource, packed );
				AddResult( results, "Backpack item preview session begins",
					backpackSession != null && backpackSession.IsActive );

				if ( backpackSession != null )
					backpackSession.Apply();

				AddResult( results, "Backpack item confirm applies intended hue",
					packed.Hue == backpackSource.PreviewHue && backpackSource.ApplyCount == 1 );
			}
			finally
			{
				DyePreviewSession.Cancel( player, null );
				DeleteItem( equipped );
				DeleteItem( packed );
				DeleteItem( ground );
				DeleteItem( otherItem );
				DeleteItem( backpackSource );
				DeleteMobile( player );
				DeleteMobile( other );
			}
		}

		private static void RunSessionLifecycleTests( List<TestResult> results )
		{
			PlayerMobile player = null;
			TestDyeSource source = null;
			TestWearable item = null;
			PlayerMobile opponent = null;

			try
			{
				player = CreatePlayer();
				opponent = CreatePlayer();
				source = new TestDyeSource( 1153 );
				player.Backpack.DropItem( source );
				item = new TestWearable();
				item.Hue = 32;
				player.AddItem( item );

				int originalHue = item.Hue;
				DyePreviewSession session = DyePreviewSession.Begin( player, source, item );

				AddResult( results, "Preview session begins", session != null && session.IsActive );
				AddResult( results, "Preview leaves real hue unchanged", item.Hue == originalHue );
				AddResult( results, "Preview does not consume source", source.ApplyCount == 0 );

				if ( session != null )
					session.Cancel( null );

				AddResult( results, "Cancel leaves real hue unchanged", item.Hue == originalHue );
				AddResult( results, "Cancel leaves source untouched", source.ApplyCount == 0 );

				session = DyePreviewSession.Begin( player, source, item );
				item.Hue = 777;

				if ( session != null )
					session.Cancel( null );

				AddResult( results, "Cancel respects a legitimate real hue change", item.Hue == 777 );

				session = DyePreviewSession.Begin( player, source, item );

				if ( session != null )
					session.Apply();

				AddResult( results, "Apply uses intended hue", item.Hue == source.PreviewHue );
				AddResult( results, "Apply runs exactly once", source.ApplyCount == 1 );

				if ( session != null )
					session.Apply();

				AddResult( results, "Duplicate apply is ignored", source.ApplyCount == 1 );

				DyePreviewSession first = DyePreviewSession.Begin( player, source, item );
				DyePreviewSession second = DyePreviewSession.Begin( player, source, item );

				AddResult( results, "Replacement disposes previous session",
					first != null && second != null && !first.IsActive && second.IsActive );

				if ( first != null )
					first.Apply();

				AddResult( results, "Stale session cannot apply", source.ApplyCount == 1 && second != null && second.IsActive );

				if ( second != null )
					second.Cancel( null );

				session = DyePreviewSession.Begin( player, source, item );
				int retainedHue = session != null ? session.PreviewHue : 0;
				item.Hue = 888;
				source.SetPreviewHue( 1201 );

				if ( session != null )
					session.ChooseAnother();

				AddResult( results, "Choose another keeps the same session active",
					session != null && session.IsActive && DyePreviewSession.GetCurrent( player ) == session );
				AddResult( results, "Choose another clears the first target", session != null && session.TargetItem == null );
				AddResult( results, "Choose another retains the originally selected hue",
					session != null && session.PreviewHue == retainedHue && session.PreviewHue != source.PreviewHue );
				AddResult( results, "Choose another respects the first item's current real hue", item.Hue == 888 );

				if ( session != null )
					session.Cancel( null );

				session = DyePreviewSession.Begin( player, source, item );
				Point3D start = player.Location;
				player.Location = new Point3D( start.X + DyePreviewUtility.MovementAllowance + 1, start.Y, start.Z );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Movement cancellation removes session", session != null && !session.IsActive );
				player.Location = start;

				session = DyePreviewSession.Begin( player, source, item );
				player.Backpack.DropItem( item );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Moving target cancels safely", session != null && !session.IsActive );
				player.AddItem( item );

				session = DyePreviewSession.Begin( player, source, item );
				player.Backpack.DropItem( item );
				player.AddItem( item );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Unequip and re-equip cancels safely", session != null && !session.IsActive );

				TestWearable deletedTarget = new TestWearable();
				player.AddItem( deletedTarget );
				session = DyePreviewSession.Begin( player, source, deletedTarget );
				deletedTarget.Delete();

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Deleting target cancels safely", session != null && !session.IsActive );

				TestDyeSource deletedSource = new TestDyeSource( 1161 );
				player.Backpack.DropItem( deletedSource );
				session = DyePreviewSession.Begin( player, deletedSource, item );
				deletedSource.Delete();

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Deleting source cancels safely", session != null && !session.IsActive );

				session = DyePreviewSession.Begin( player, source, item );
				source.SetLastMoved();

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Moving source cancels safely", session != null && !session.IsActive );

				session = DyePreviewSession.Begin( player, source, item );
				SetPrivateField( session, "m_StartingMap", null );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Map change cancellation removes session", session != null && !session.IsActive );

				session = DyePreviewSession.Begin( player, source, item );
				SetPrivateField( session, "m_StartedAt", DateTime.UtcNow - DyePreviewUtility.SessionTimeout - TimeSpan.FromSeconds( 1.0 ) );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Timeout cancellation removes session", session != null && !session.IsActive );

				session = DyePreviewSession.Begin( player, source, item );
				DyePreviewSession.NotifyDisconnected( player );
				AddResult( results, "Disconnect cleanup removes session", session != null && !session.IsActive );

				session = DyePreviewSession.Begin( player, source, item );
				DyePreviewSession.NotifyDeath( player );
				AddResult( results, "Death cleanup removes session", session != null && !session.IsActive );

				player.Combatant = opponent;
				session = DyePreviewSession.Begin( player, source, item );
				AddResult( results, "Combat does not block preview", session != null && session.IsActive );

				if ( session != null )
					session.ValidateNow();

				AddResult( results, "Combat does not cancel preview", session != null && session.IsActive );
				player.Combatant = null;

				if ( session != null )
					session.Cancel( null );
			}
			finally
			{
				DyePreviewSession.Cancel( player, null );
				DeleteItem( item );
				DeleteItem( source );
				DeleteMobile( player );
				DeleteMobile( opponent );
			}
		}

		private static void RunSourceConsumptionTests( List<TestResult> results )
		{
			PlayerMobile player = null;
			MagicalDyes singleUse = null;
			TestWearable item = null;
			DyeTub tub = null;
			FancyShirt shirt = null;

			try
			{
				player = CreatePlayer();
				singleUse = new MagicalDyes( 2 );
				player.Backpack.DropItem( singleUse );
				item = new TestWearable();
				player.AddItem( item );

				DyePreviewSession session = DyePreviewSession.Begin( player, singleUse, item );
				AddResult( results, "Single-use preview does not consume", singleUse.Amount == 2 );

				if ( session != null )
					session.Cancel( null );

				AddResult( results, "Single-use cancel does not consume", singleUse.Amount == 2 );

				int bottlesBefore = player.Backpack.GetAmount( typeof( Bottle ) );
				session = DyePreviewSession.Begin( player, singleUse, item );

				if ( session != null )
					session.Apply();

				AddResult( results, "Single-use apply consumes exactly one", !singleUse.Deleted && singleUse.Amount == 1 );
				AddResult( results, "Single-use apply creates one bottle",
					player.Backpack.GetAmount( typeof( Bottle ) ) == bottlesBefore + 1 );

				tub = new DyeTub();
				tub.DyedHue = 1175;
				player.Backpack.DropItem( tub );
				shirt = new FancyShirt();
				player.AddItem( shirt );
				session = DyePreviewSession.Begin( player, tub, shirt );

				if ( session != null )
					session.Apply();

				AddResult( results, "Reusable tub applies selected hue", shirt.Hue == 1175 );
				AddResult( results, "Reusable tub is not consumed", !tub.Deleted );
			}
			finally
			{
				DyePreviewSession.Cancel( player, null );
				DeleteItem( item );
				DeleteItem( shirt );
				DeleteItem( singleUse );
				DeleteItem( tub );
				DeleteMobile( player );
			}
		}

		private static void RunLimitedSourceTests( List<TestResult> results )
		{
			PlayerMobile player = null;
			AllDyeTubsArmor paidTub = null;
			LeatherCap armor = null;
			LeatherPaintPalette palette = null;
			LeatherCap paletteArmor = null;
			HueStone stone = null;
			TestWearable stoneItem = null;
			HueVacuumTube vacuum = null;
			TestWearable vacuumItem = null;
			MagicPigment pigment = null;
			TestWearable pigmentItem = null;
			AwesomeDyeNearlyAnythingTub awesome = null;
			LeatherCap awesomeArmor = null;

			try
			{
				player = CreatePlayer();

				paidTub = new AllDyeTubsArmor();
				paidTub.DyedHue = 1181;
				paidTub.Charged = true;
				paidTub.Charges = 2;
				player.Backpack.DropItem( paidTub );
				player.Backpack.DropItem( new Gold( 150 ) );
				armor = new LeatherCap();
				player.AddItem( armor );

				int goldBefore = player.Backpack.GetAmount( typeof( Gold ) );
				DyePreviewSession session = DyePreviewSession.Begin( player, paidTub, armor );
				AddResult( results, "Paid tub preview does not charge gold",
					player.Backpack.GetAmount( typeof( Gold ) ) == goldBefore );
				AddResult( results, "Paid tub preview does not spend charge", paidTub.Charges == 2 );

				if ( session != null )
					session.Apply();

				AddResult( results, "Paid tub charges exact gold",
					player.Backpack.GetAmount( typeof( Gold ) ) == goldBefore - 150 );
				AddResult( results, "Paid tub spends one charge", paidTub.Charges == 1 );

				palette = new LeatherPaintPalette();
				player.Backpack.DropItem( palette );
				paletteArmor = new LeatherCap();
				player.AddItem( paletteArmor );
				int paletteUses = palette.Uses;
				session = DyePreviewSession.Begin( player, palette, paletteArmor );
				AddResult( results, "Palette preview does not spend use", palette.Uses == paletteUses );

				if ( session != null )
					session.Apply();

				AddResult( results, "Palette apply spends one use", palette.Uses == paletteUses - 1 );

				stone = new HueStone();
				stone.Movable = true;
				stone.Hue = 1173;
				stone.NCharges = 2;
				player.Backpack.DropItem( stone );
				stoneItem = new TestWearable();
				player.AddItem( stoneItem );
				session = DyePreviewSession.Begin( player, stone, stoneItem );
				AddResult( results, "Hue stone preview does not spend charge", stone.NCharges == 2 );

				if ( session != null )
					session.Apply();

				AddResult( results, "Hue stone apply spends one charge", stone.NCharges == 1 );

				vacuum = new HueVacuumTube( 1 );
				vacuum.Dye = 1165;
				player.Backpack.DropItem( vacuum );
				vacuumItem = new TestWearable();
				player.AddItem( vacuumItem );
				session = DyePreviewSession.Begin( player, vacuum, vacuumItem );
				AddResult( results, "Hue vacuum preview keeps held dye", vacuum.Dye == 1165 );

				if ( session != null )
					session.Apply();

				AddResult( results, "Hue vacuum apply keeps extraction count", !vacuum.Deleted && vacuum.Uses == 1 );
				AddResult( results, "Hue vacuum apply clears held dye", vacuum.Dye == 0 );

				pigment = new MagicPigment();
				pigment.ApplyHue( player, 1157, 0x23F );
				player.Backpack.DropItem( pigment );
				pigmentItem = new TestWearable();
				player.AddItem( pigmentItem );
				session = DyePreviewSession.Begin( player, pigment, pigmentItem );

				if ( session != null )
					session.Apply();

				AddResult( results, "Magic pigment remains reusable", !pigment.Deleted );

				awesome = new AwesomeDyeNearlyAnythingTub();
				awesome.Hue = 1149;
				player.Backpack.DropItem( awesome );
				player.Backpack.DropItem( new Gold( 50000 ) );
				awesomeArmor = new LeatherCap();
				player.AddItem( awesomeArmor );
				goldBefore = player.Backpack.GetAmount( typeof( Gold ) );
				session = DyePreviewSession.Begin( player, awesome, awesomeArmor );
				AddResult( results, "Awesome tub preview does not charge gold",
					player.Backpack.GetAmount( typeof( Gold ) ) == goldBefore );

				if ( session != null )
					session.Apply();

				AddResult( results, "Awesome tub charges exact category cost",
					player.Backpack.GetAmount( typeof( Gold ) ) == goldBefore - awesome.CostLeatherDye );
			}
			finally
			{
				DyePreviewSession.Cancel( player, null );
				DeleteItem( paidTub );
				DeleteItem( armor );
				DeleteItem( palette );
				DeleteItem( paletteArmor );
				DeleteItem( stone );
				DeleteItem( stoneItem );
				DeleteItem( vacuum );
				DeleteItem( vacuumItem );
				DeleteItem( pigment );
				DeleteItem( pigmentItem );
				DeleteItem( awesome );
				DeleteItem( awesomeArmor );
				DeleteMobile( player );
			}
		}

		private static PlayerMobile CreatePlayer()
		{
			PlayerMobile player = new PlayerMobile();

			if ( player.Backpack == null )
				player.AddItem( new Backpack() );

			return player;
		}

		private static void DeleteItem( Item item )
		{
			if ( item != null && !item.Deleted )
				item.Delete();
		}

		private static void DeleteMobile( Mobile mobile )
		{
			if ( mobile != null && !mobile.Deleted )
				mobile.Delete();
		}

		private static void SetPrivateField( object target, string fieldName, object value )
		{
			if ( target == null )
				return;

			System.Reflection.FieldInfo field = target.GetType().GetField(
				fieldName,
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );

			if ( field == null )
				throw new InvalidOperationException( "Missing test field: " + fieldName );

			field.SetValue( target, value );
		}

		private static void AddResult( List<TestResult> results, string name, bool passed )
		{
			TestResult result = new TestResult();
			result.Name = name;
			result.Level = passed ? TestResultLevel.Pass : TestResultLevel.Fail;
			result.MeasuredValue = passed ? "pass" : "fail";
			result.Threshold = "pass";
			result.Reason = passed ? "Behavior matched the dye preview contract." : "Behavior did not match the dye preview contract.";
			results.Add( result );
		}

		private static void AddFailure( List<TestResult> results, string name, Exception ex )
		{
			TestResult result = new TestResult();
			result.Name = name;
			result.Level = TestResultLevel.Fail;
			result.MeasuredValue = ex.GetType().Name;
			result.Threshold = "no exception";
			result.Reason = ex.ToString();
			results.Add( result );
		}

		private sealed class TestWearable : Item
		{
			public TestWearable() : base( 0x1517 )
			{
				Layer = Layer.Shirt;
			}

			public TestWearable( Serial serial ) : base( serial )
			{
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );
				writer.Write( 0 );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );
				reader.ReadInt();
			}
		}

		private sealed class TestDyeSource : Item, IDyePreviewSource
		{
			private int m_PreviewHue;
			private int m_ApplyCount;

			public Item SourceItem
			{
				get { return this; }
			}

			public int PreviewHue
			{
				get { return m_PreviewHue; }
			}

			public int ApplyCount
			{
				get { return m_ApplyCount; }
			}

			public void SetPreviewHue( int previewHue )
			{
				m_PreviewHue = previewHue;
			}

			public TestDyeSource( int previewHue ) : base( 0xFAB )
			{
				m_PreviewHue = previewHue;
			}

			public TestDyeSource( Serial serial ) : base( serial )
			{
			}

			public bool CanBeginPreview( Mobile from, out string failureMessage )
			{
				failureMessage = null;
				return from != null && !from.Deleted && !Deleted;
			}

			public bool CanPreviewTarget( Mobile from, Item target, int selectedHue, out string failureMessage )
			{
				failureMessage = null;
				return DyePreviewUtility.IsOwnedGear( from, target );
			}

			public bool CanApply( Mobile from, Item target, int selectedHue, out string failureMessage )
			{
				return CanPreviewTarget( from, target, selectedHue, out failureMessage );
			}

			public bool ApplyPermanentDye( Mobile from, Item target, int selectedHue )
			{
				++m_ApplyCount;
				target.Hue = selectedHue;
				return true;
			}

			public bool IsSourceStillValid( Mobile from )
			{
				return from != null && !from.Deleted && !Deleted;
			}

			public string GetSourceDisplayName()
			{
				return "test dye source";
			}

			public string GetConsumptionMessage()
			{
				return "Testing only.";
			}

			public void BeginTargeting( Mobile from )
			{
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );
				writer.Write( 0 );
				writer.Write( m_PreviewHue );
				writer.Write( m_ApplyCount );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );
				reader.ReadInt();
				m_PreviewHue = reader.ReadInt();
				m_ApplyCount = reader.ReadInt();
			}
		}
	}
}
