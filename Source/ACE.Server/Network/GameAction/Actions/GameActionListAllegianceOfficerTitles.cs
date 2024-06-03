namespace ACE.Server.Network.GameAction.Actions
{
    public static class GameActionListAllegianceOfficerTitles
    {
        [GameAction(GameActionType.ListAllegianceOfficerTitles)]
        public static void Handle(ClientMessage message, ISession session)
        {
            session.Player.HandleActionListAllegianceOfficerTitles();
        }
    }
}
