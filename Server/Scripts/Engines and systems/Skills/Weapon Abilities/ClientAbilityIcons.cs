using System;
using Server;

namespace Server.Items
{
	/// <summary>
	/// Le icone delle mosse speciali del client (primaria/secondaria nel libro delle abilità di combattimento)
	/// inviano il pacchetto SetAbility, ma nessuno script lo gestiva: cliccarle non faceva nulla.
	/// Qui lo colleghiamo allo stesso meccanismo usato dalla barra personalizzata e dai comandi [Set1..[Set5.
	/// Se l'arma non possiede la mossa richiesta, la convalida standard la rifiuta con il suo messaggio.
	/// </summary>
	public static class ClientAbilityIcons
	{
		public static void Initialize()
		{
			EventSink.SetAbility += new SetAbilityEventHandler( OnSetAbility );
		}

		private static void OnSetAbility( SetAbilityEventArgs e )
		{
			if ( e.Mobile == null )
				return;

			CustomWeaponAbilities.ServerSideSetAbility( e.Mobile, e.Index );
		}
	}
}
