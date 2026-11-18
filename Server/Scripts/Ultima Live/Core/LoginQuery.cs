using System;
using Server;
using Server.Mobiles;

namespace UltimaLive
{
  /// <summary>
  /// I client recenti (ClassicUO/TazUO) creano la mappa "persistente" di UltimaLive vuota e la riempiono
  /// con i blocchi inviati dal server. Il server però interrogava il client solo quando il personaggio
  /// cambiava blocco camminando: chi entra fermo (per esempio durante l'introduzione) vedeva tutto nero.
  /// Qui chiediamo l'allineamento della mappa anche all'ingresso nel gioco.
  /// </summary>
  public static class LoginQuery
  {
    public static void Initialize()
    {
      EventSink.Login += new LoginEventHandler(OnLogin);
    }

    private static void OnLogin(LoginEventArgs e)
    {
      Mobile m = e.Mobile;
      if (m == null)
        return;

      Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Query), m);
      Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(Query), m);
    }

    private static void Query(object state)
    {
      Mobile m = state as Mobile;
      if (m == null || m.Deleted || m.NetState == null || m.Map == null || m.Map == Map.Internal)
        return;

      m.Send(new UltimaLive.Network.QueryClientHash(m));
    }
  }
}
