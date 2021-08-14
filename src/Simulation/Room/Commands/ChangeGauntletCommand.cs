using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class ChangeGauntletCommand : Command
    {
        public ChangeGauntletCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message, Room room)
        {
            
            ChangeGauntletMessage j2 = message.ToObject<ChangeGauntletMessage>();
            //objects[]
            Player2 onb2 = (Player2)room.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.ChangeGauntlet(j2.type);
        }
    }
}