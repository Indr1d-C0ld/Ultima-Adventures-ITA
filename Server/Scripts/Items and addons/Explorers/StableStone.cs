using System; 
using Server.Items; 
using Server.Misc; 
using Server.Network; 
using Server.Mobiles; 
using Server.Multis; 
using Server.Gumps;
using Server.Targeting;
using Server.ContextMenus;
using System.Collections.Generic;

namespace Server.Items 
{ 
public class StableStone : Item
	{ 
		[Constructable] 
		public StableStone() : base( 0x14E7 ) 
		{ 
			Name = "hitching post"; 
			Weight = 20.0;
		} 

		public StableStone( Serial serial ) : base( serial ) 
		{ 
		}

		private class StableEntry : ContextMenuEntry
		{
			private StableStone m_Trainer;
			private Mobile m_From;

			public StableEntry( StableStone trainer, Mobile from ) : base( 6126, 2 )
			{
				m_Trainer = trainer;
				m_From = from;
			}

			public override void OnClick()
			{
				m_Trainer.BeginStable( m_From );
			}
		}

		public class ClaimingGumpEntry : ContextMenuEntry
		{
			private StableStone m_Trainer;
			private Mobile m_From;
			
			public ClaimingGumpEntry( StableStone trainer, Mobile from ) : base( 6165, 3 )
			{
				m_Trainer = trainer;
				m_From = from;
			}

			public override void OnClick()
			{
			    if( !( m_From is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_From;
				{
					m_Trainer.BeginClaimList( m_From );
				}
            }
        }

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list  )
		{
			base.GetContextMenuEntries( from, list );
			if ( from.Alive )
			{
				list.Add( new StableEntry( this, from ) );
				list.Add( new ClaimingGumpEntry( this, from ) );

				if ( from.Stabled.Count > 0 )
					list.Add( new ClaimListEntry( this, from ) );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( this.ItemID == 0x14E7 ){ this.ItemID = 0x14E8; } else { this.ItemID = 0x14E7; }
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Stable Your Pets At Your Home");
			list.Add( 1049644, "For Grandmasters In Camping");
        } 

		private class StableTarget : Target
		{
			private StableStone m_Trainer;

			public StableTarget( StableStone trainer ) : base( 12, false, TargetFlags.None )
			{
				m_Trainer = trainer;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is BaseCreature )
					m_Trainer.EndStable( from, (BaseCreature)targeted );
				else
					from.SendMessage ("You can't stable that!");
			}
		}

		public void BeginStable( Mobile from )
		{
			if ( Deleted || !from.CheckAlive() )
				return;

            if (this.Movable)
			{
                from.SendMessage("This must be locked down in a house to use!");
			}
			else if ( from.Skills[SkillName.Camping].Base < 100 )
			{
				from.SendMessage ("Only grandmasters in camping may stable pets at home!");
			}
			else if ( from.Stabled.Count >= Server.Mobiles.AnimalTrainer.GetMaxStabled( from ) )
			{
				from.SendMessage ("You can't stable that! You have too many pets stabled, you have reached your max amount");
			}
			else
			{
				from.SendMessage ("The hitching post requires 30 gold per pet for each real world week for maintenance!");
				from.SendMessage ("the gold is automatically withdrawn from your bank account");
				from.SendMessage ("Target the animal you wish to stable!");
				from.Target = new StableTarget( this );
			}
		}

		public void EndStable( Mobile from, BaseCreature pet )
		{
			if ( Deleted || !from.CheckAlive() )
				return;

			if ( !pet.Controlled || pet.ControlMaster != from )
			{
				from.SendMessage ("That is not your pet!");
			}
			else if ( pet.IsDeadPet )
			{
				from.SendMessage ("That pet is dead and cannot be stabled!");
			}
			else if ( pet.Summoned )
			{
				from.SendMessage ("You can not stable summoned creatures");
			}
			else if ( pet.Body.IsHuman )
			{
				from.SendMessage ("That is not your pet!");
			}
			else if ( (pet is PackLlama || pet is PackHorse || pet is Beetle) && (pet.Backpack != null && pet.Backpack.Items.Count > 0) )
			{
				from.SendMessage ("You need to unload the pack animal before you can stable it!");
			}
			else if ( pet.Combatant != null && pet.InRange( pet.Combatant, 12 ) && pet.Map == pet.Combatant.Map )
			{
				from.SendMessage ("Your pet seems to be busy at the moment, try again when its not!");
			}
			else if ( from.Stabled.Count >= Server.Mobiles.AnimalTrainer.GetMaxStabled( from ) )
			{
				from.SendMessage ("You have too many pets in the stables!");
			}
			else
			{
				Container bank = from.BankBox;

				if ( bank != null && bank.ConsumeTotal( typeof( Gold ), 30 ) )
				{
					pet.Language = null;
					pet.ControlTarget = null;
					pet.ControlOrder = OrderType.Stay;
					pet.Internalize();

                                        pet.SetControlMaster( null );
                                        pet.SummonMaster = null;

                                        pet.IsStabled = true;
                                        pet.LastStableTime = DateTime.UtcNow;
                                        from.Stabled.Add( pet );
					from.SendMessage ("Your pet is stabled. You may recover it by saying 'claim'. In one real world week,");
					from.SendMessage ("if your pet is not claimed by then, it will vanish if it is not claimed!");
				}
				else
				{
					from.SendMessage ("You lack The necessary bank funds to do this!");
				}
			}
		}

                private static string GetStableSortName( BaseCreature pet )
                {
                        if ( pet == null )
                                return String.Empty;

                        string name = pet.Name;

                        if ( String.IsNullOrEmpty( name ) )
                                name = pet.GetType().Name;

                        return name ?? String.Empty;
                }

                public void BeginClaimList( Mobile from, int page = 0 )
                {
                        if ( Deleted || !from.CheckAlive() )
                                return;

                        List<BaseCreature> list = new List<BaseCreature>();

			for ( int i = 0; i < from.Stabled.Count; ++i )
			{
				BaseCreature pet = from.Stabled[i] as BaseCreature;

				if ( pet == null || pet.Deleted )
				{
					pet.IsStabled = false;
					from.Stabled.RemoveAt( i );
					--i;
					continue;
				}

                                list.Add( pet );
                        }

                        if ( this.Movable )
                        {
                                from.SendMessage( "This must be locked down in a house to use!" );
                                return;
                        }

                        if ( from.Skills[SkillName.Camping].Base < 100 )
                        {
                                from.SendMessage( "Only grandmasters in camping may stable pets at home!" );
                                return;
                        }

                        if ( list.Count > 0 )
                        {
                                list.Sort( delegate ( BaseCreature a, BaseCreature b )
                                {
                                        string nameA = GetStableSortName( a );
                                        string nameB = GetStableSortName( b );
                                        return String.Compare( nameA, nameB, StringComparison.OrdinalIgnoreCase );
                                } );

                                StableMasterGumpOptions options = new StableMasterGumpOptions
                                {
                                        CapacityResolver = AnimalTrainer.GetMaxStabled,
                                        ReopenHandler = delegate ( Mobile mobile, int nextPage )
                                        {
                                                BeginClaimList( mobile, nextPage );
                                        },
                                        ClaimHandler = EndClaimList,
                                        TryPurchaseExpansion = TryPurchaseStableExpansion,
                                        GetExpansionCost = AnimalTrainer.GetStableExpansionCost,
                                        WeeklyCostPerPet = AnimalTrainer.StableWeeklyCostPerPet,
                                        BaseStableSlots = AnimalTrainer.BaseStableSlots,
                                        StableExpansionAmount = AnimalTrainer.StableExpansionAmount,
                                        MaxStableSlots = AnimalTrainer.MaxStableSlots,
                                        MaxStableExpansions = AnimalTrainer.MaxStableExpansions
                                };

                                from.SendGump( new StableMasterGump( "Stable Overview", from, list, page, options ) );
                        }
                        else
                                from.SendMessage( "But I have no animals stabled with me at the moment!" );
                }

                private bool TryPurchaseStableExpansion( PlayerMobile pm )
                {
                        if ( pm == null )
                                return false;

                        if ( pm.PurchasedStableExpansions >= AnimalTrainer.MaxStableExpansions )
                        {
                                pm.SendMessage( "You have already unlocked the full capacity of your stables." );
                                return false;
                        }

                        int cost = AnimalTrainer.GetStableExpansionCost( pm );

                        if ( !AnimalTrainer.TryConsumeStableGold( pm, cost ) )
                        {
                                pm.SendMessage( "You lack the funds in your pack or bank." );
                                return false;
                        }

                        pm.PurchasedStableExpansions++;
                        pm.SendMessage( String.Format( "Your stable can now hold {0} pets.", AnimalTrainer.GetMaxStabled( pm ) ) );
                        pm.SendMessage( String.Format( "You paid {0} gold for the expansion.", cost.ToString( "N0" ) ) );
                        return true;
                }

		public void EndClaimList( Mobile from, BaseCreature pet )
		{
			if ( pet == null || pet.Deleted || from.Map != this.Map || !from.Stabled.Contains( pet ) || !from.CheckAlive() )
				return;

			if ( (from.Followers + pet.ControlSlots) <= from.FollowersMax )
			{
				pet.SetControlMaster( from );

				if ( pet.Summoned )
					pet.SummonMaster = from;

				pet.ControlTarget = from;
				pet.ControlOrder = OrderType.Follow;

				pet.MoveToWorld( from.Location, from.Map );

				pet.IsStabled = false;
				from.Stabled.Remove( pet );
				from.SendMessage ("Here you go...");
			}
			else
			{
				from.SendMessage ( "That Pet remained in the stables because you have too many followers");
			}
		}
		public void Claim( Mobile from )
		{
			if ( Deleted || !from.CheckAlive() )
				return;

			bool claimed = false;
			int stabled = 0;

			for ( int i = 0; i < from.Stabled.Count; ++i )
			{
				BaseCreature pet = from.Stabled[i] as BaseCreature;

				if ( pet == null || pet.Deleted )
				{
					pet.IsStabled = false;
					from.Stabled.RemoveAt( i );
					--i;
					continue;
				}

				++stabled;

				if ( (from.Followers + pet.ControlSlots) <= from.FollowersMax )
				{
					pet.SetControlMaster( from );

					if ( pet.Summoned )
						pet.SummonMaster = from;

					pet.Language = null;
					pet.ControlTarget = from;
					pet.ControlOrder = OrderType.Follow;
					pet.Location = new Point3D( pet.X, pet.Y, 0 );
        			World.AddMobile( pet );
					pet.IsStabled = false;
					from.Stabled.RemoveAt( i );
					--i;

					claimed = true;
					
				}
				else
				{
					from.SendMessage ("That Pet remained in the stables because you have too many followers");
				}
			}

			if ( claimed )
				from.SendMessage ("Here you go...");

			if ( stabled == 0 )
				from.SendMessage ("But I have no animals stabled with me at the moment!");
		}
      	
		public override bool HandlesOnSpeech{ get{ return true; } } 

		public override void OnSpeech( SpeechEventArgs e )
		{
			if ( ( !e.Handled && e.HasKeyword( 0x0008 ) ) && ( e.Mobile.InRange( this, 2 ) ) )
			{
				e.Handled = true;
				BeginStable( e.Mobile );
			}
			else if ( ( !e.Handled && e.HasKeyword( 0x0009 ) ) && ( e.Mobile.InRange( this, 2 ) ) )
			{
				e.Handled = true;

				if ( !Insensitive.Equals( e.Speech, "Claim" ) )
					BeginClaimList( e.Mobile );
				else
					BeginClaimList( e.Mobile );
			}
			else
			{
				base.OnSpeech( e );
			}
		}

		private class ClaimListEntry : ContextMenuEntry
		{
			private StableStone m_Trainer;
			private Mobile m_From;

			public ClaimListEntry( StableStone trainer, Mobile from ) : base( 6127, 2 )
			{
				m_Trainer = trainer;
				m_From = from;
			}

			public override void OnClick()
			{
				m_Trainer.BeginClaimList( m_From );
               		m_From.PlaySound( 1050 );
			}
		}

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); // version 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
		} 
   	} 
} 