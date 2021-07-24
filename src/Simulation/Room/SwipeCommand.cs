using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class SwipeCommand : Command
    {
        public SwipeCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message)
        {
             SwipeMessage j2 = message.ToObject<SwipeMessage>();
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.Swipe(j2.degree, j2.direction);
        }
    }
}