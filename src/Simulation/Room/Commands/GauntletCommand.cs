using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class GauntletCommand : Command
    {
        public GauntletCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message)
        {
            
            GauntletMessage j2 = message.ToObject<GauntletMessage>();
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.clientId].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.UseGauntlet(j2.active);
        }
    }
}