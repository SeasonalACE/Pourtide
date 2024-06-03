namespace ACE.Server.Network.GameAction.Actions
{
    public static class GameActionQueryMotd
    {
        [GameAction(GameActionType.QueryMotd)]
        public static void Handle(ClientMessage message, ISession session)
        {
            session.Player.HandleActionQueryMotd();
        }
    }
}
