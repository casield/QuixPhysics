using System.Numerics;

namespace QuixPhysics
{
    public class Bot
    {
        Arena arena;
        PlayerBot player;
        public Bot(Room room)
        {
            arena = (Arena)room.gamemode;
            room.simulator.SendMessage("createbot","",room.connectionState.workSocket);
        }

        public void OnStart(){
            player = (PlayerBot)arena.users.Find(e=>e.sessionId =="Bot").player;
            player.OnStart();
            
        }
    }
}