using System;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Commands
{
        public class PetStandbyRange
        {
                public static void Initialize()
                {
                        CommandSystem.Register( "petrange", AccessLevel.Player, new CommandEventHandler( PetRange_OnCommand ) );
                        CommandSystem.Register( "petstandby", AccessLevel.Player, new CommandEventHandler( PetRange_OnCommand ) );
                }

                [Usage( "petrange [0-10]" )]
                [Description( "Sets how far your pets spread out while following or standing by. Use 0 to disable spreading." )]
                public static void PetRange_OnCommand( CommandEventArgs e )
                {
                        PlayerMobile player = e.Mobile as PlayerMobile;

                        if ( player == null )
                                return;

                        string option = e.ArgString == null ? String.Empty : e.ArgString.Trim();

                        if ( option.Length == 0 )
                        {
                                if ( player.PetStandbyRange == 0 )
                                        player.SendMessage( "Pet standby spreading is disabled." );
                                else
                                        player.SendMessage( "Your pet standby range is set to {0}.", player.PetStandbyRange );

                                player.SendMessage( "Usage: [petrange 0-10" );
                                return;
                        }

                        int range;

                        if ( Int32.TryParse( option, out range ) && range >= PlayerMobile.MinPetStandbyRange && range <= PlayerMobile.MaxPetStandbyRange )
                        {
                                player.PetStandbyRange = range;

                                if ( range == 0 )
                                        player.SendMessage( 68, "Pet standby spreading is now disabled." );
                                else
                                        player.SendMessage( 68, "Your pet standby range is now {0}.", player.PetStandbyRange );
                        }
                        else
                        {
                                player.SendMessage( "Usage: [petrange 0-10" );
                        }
                }
        }
}
