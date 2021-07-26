using Newtonsoft.Json.Linq;
using OVars;

namespace QuixPhysics
{
    internal class OVarCommand : Command
    {
        OVarManager oVarManager;
        public OVarCommand(Simulator _simulator) : base(_simulator)
        {
            oVarManager= simulator.oVarManager;
        }
        public override void OnRead(JObject message, Room room)
        {
            var m = message.ToObject<OVarMessage>();
            if(m.a == "up"){
                oVarManager.OnUpdate(m);
            }
            if(m.a == "add"){
                oVarManager.AddedInGolf(m.i,m.v);
            }
        }
    }
}