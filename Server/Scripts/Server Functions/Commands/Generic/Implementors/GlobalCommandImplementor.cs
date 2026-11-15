using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

namespace Server.Commands.Generic
{
	public class GlobalCommandImplementor : BaseCommandImplementor
	{
		public GlobalCommandImplementor()
		{
			Accessors = new string[]{ "Global" };
			SupportRequirement = CommandSupport.Global;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.Administrator;
			Usage = "Global <command> [condition]";
			Description = "Invokes the command on all appropriate objects in the world. Optional condition arguments can further restrict the set of objects.";
		}

                private class GlobalKillState
                {
                        public GlobalCommandImplementor Implementor { get; private set; }
                        public BaseCommand Command { get; private set; }
                        public string[] Arguments { get; private set; }
                        public object Target { get; private set; }

                        public GlobalKillState( GlobalCommandImplementor implementor, BaseCommand command, string[] arguments, object target )
                        {
                                Implementor = implementor;
                                Command = command;
                                Arguments = arguments;
                                Target = target;
                        }
                }

                public override void Compile( Mobile from, BaseCommand command, ref string[] args, ref object obj )
                {
                        try
                        {
				Extensions ext = Extensions.Parse( from, ref args );

				bool items, mobiles;

				if ( !CheckObjectTypes( command, ext, out items, out mobiles ) )
					return;

                                ArrayList list = new ArrayList();

                                bool isKillCommand = command is KillCommand && command.Commands != null && command.Commands.Length > 0 && Insensitive.Equals( command.Commands[0], "Kill" );

				if ( items )
				{
					foreach ( Item item in World.Items.Values )
					{
						if ( ext.IsValid( item ) )
							list.Add( item );
					}
				}

				if ( mobiles )
				{
                                        foreach ( Mobile mob in World.Mobiles.Values )
                                        {
                                                if ( isKillCommand )
                                                {
                                                        BaseCreature bc = mob as BaseCreature;

                                                        if ( bc != null && bc.IsStabled )
                                                                continue;
                                                }

                                                if ( ext.IsValid( mob ) )
                                                        list.Add( mob );
                                        }
                                }

				ext.Filter( list );

				obj = list;
			}
                        catch ( Exception ex )
                        {
                                from.SendMessage( ex.Message );
                        }
                }

                public override void Process( Mobile from, BaseCommand command, string[] args )
                {
                        try
                        {
                                object obj = null;
                                string[] newArgs = args;

                                Compile( from, command, ref newArgs, ref obj );

                                if ( command is KillCommand && command.Commands != null && command.Commands.Length > 0 && Insensitive.Equals( command.Commands[0], "Kill" ) )
                                {
                                        ArrayList list = obj as ArrayList;

                                        if ( list != null && list.Count > 0 )
                                        {
                                                string message = String.Format( "You are about to globally kill {0} mobile{1}.<br><br>Do you wish to continue?", list.Count, list.Count == 1 ? "" : "s" );

                                                from.SendGump( new WarningGump( 1060635, 30720, message, 0xFFC000, 420, 280, new WarningGumpCallback( OnGlobalKillConfirm ), new GlobalKillState( this, command, newArgs, list ) ) );
                                                from.SendMessage( "Awaiting confirmation for global kill command..." );
                                        }
                                        else
                                        {
                                                TryExecuteGlobalCommand( from, obj, command, newArgs );
                                        }

                                        return;
                                }

                                TryExecuteGlobalCommand( from, obj, command, newArgs );
                        }
                        catch ( Exception ex )
                        {
                                from.SendMessage( ex.Message );
                        }
                }

                private void OnGlobalKillConfirm( Mobile from, bool okay, object state )
                {
                        GlobalKillState info = state as GlobalKillState;

                        if ( info == null || info.Implementor != this )
                                return;

                        if ( !okay )
                        {
                                from.SendMessage( "Global kill command aborted." );
                                return;
                        }

                        if ( TryExecuteGlobalCommand( from, info.Target, info.Command, info.Arguments ) )
                                from.SendMessage( "Global kill command confirmed." );
                }

                private bool TryExecuteGlobalCommand( Mobile from, object target, BaseCommand command, string[] args )
                {
                        if ( !EnsureGlobalCommandSave( from ) )
                                return false;

                        if ( from != null )
                                from.SendMessage( "Save complete. Executing global command..." );

                        RunCommand( from, target, command, args );
                        return true;
                }

                private bool EnsureGlobalCommandSave( Mobile from )
                {
                        if ( from != null )
                                from.SendMessage( "Saving world before executing global command..." );

                        try
                        {
                                AutoSave.Save();
                                return true;
                        }
                        catch ( Exception ex )
                        {
                                if ( from != null )
                                        from.SendMessage( String.Format( "Global command cancelled. Save failed: {0}", ex.Message ) );
                                else
                                        Console.WriteLine( "Global command cancelled. Save failed: {0}", ex );
                        }

                        return false;
                }
        }
}
