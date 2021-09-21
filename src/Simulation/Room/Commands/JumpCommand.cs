using Newtonsoft.Json.Linq;

namespace QuixPhysics{
    class JumpCommand : Command
    {
        public JumpCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message, Room room)
        {
            XYMessage j2 =message.ToObject<XYMessage>();
            //objects[]
            Player2 onb2 = (Player2)room.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.actionsManager.Jump(j2);
        }
    }
}