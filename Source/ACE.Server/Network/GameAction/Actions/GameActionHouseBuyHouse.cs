using System;
using ACE.Server.Managers;
using ACE.Server.Network.Structure;

namespace ACE.Server.Network.GameAction.Actions
{
    /// <summary>
    /// Purchase a house
    /// </summary>
    public static class GameActionHouseBuyHouse
    {
        [GameAction(GameActionType.BuyHouse)]
        public static void Handle(ClientMessage message, Session session)
        {
            //Console.WriteLine("Received 0x21C - BuyHouse");

            var slumlord = message.Payload.ReadGuid(session);
            var items = message.Payload.ReadListUInt32();

            if (HouseManager.ValidatePourHousing(session.Player.Location.LandblockId.Landblock))
                session.Player.HandleActionBuyHouse(slumlord, items);
        }
    }
}
