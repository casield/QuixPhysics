using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    class RotateCommand : Command
    {
        public RotateCommand(Simulator _simulator) : base(_simulator)
        {
        }

        public override void OnRead(JObject message)
        {
            //base.OnRead(message);
            MoveMessage j2 = message.ToObject<MoveMessage>();
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.Rotate(j2);
        }
    }
}