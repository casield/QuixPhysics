using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class ShootCommand : Command
    {
        public ShootCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message, Room room)
        {
            ShootMessage j2 = message.ToObject<ShootMessage>();
            //objects[]
            Player2 onb2 = (Player2)room.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.actionsManager.Shoot(j2);
        }
    }
}