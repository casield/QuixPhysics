using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    class MoveCommand : Command
    {
        public MoveCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message)
        {
           // base.OnRead(message);
            MoveMessage j = message.ToObject<MoveMessage>();
            Player2 onb = (Player2)simulator.users[j.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb.Move(j);
        }
    }
}