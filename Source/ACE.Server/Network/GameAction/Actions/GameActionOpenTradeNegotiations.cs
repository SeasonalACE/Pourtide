using ACE.Server.Managers;

namespace ACE.Server.Network.GameAction.Actions
{
    public static class GameActionOpenTradeNegotiations
    {
        [GameAction(GameActionType.OpenTradeNegotiations)]
        public static void Handle(ClientMessage message, Session session)
        {
            var tradePartnerGuid = message.Payload.ReadGuid(session);

            session.Player.HandleActionOpenTradeNegotiations(tradePartnerGuid, true);
        }
    }
}
