using System.Collections.Generic;

namespace QuixPhysics
{
    public class Shop
    {
        private Player2 player;

        private List<Item> boughtItems= new List<Item>();

        public Shop(Player2 player)
        {
            this.player = player;
        }

        public void BuyItem()
        {
            // Create a Town
            var town = new Town(player.user);
            boughtItems.Add(town);
            var pos = player.room.GetGameMode<Arena>().hextilesAddon.GetHextilPosition(player.GetPosition());
            QuixConsole.Log("HexPlayerPos",pos);
            town.Instantiate(player.room,pos);

        }
    }
}