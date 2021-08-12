using Newtonsoft.Json.Linq;
using OVars;

namespace QuixPhysics
{
    internal class OVarCommand : Command
    {
      
        public OVarCommand(Simulator _simulator) : base(_simulator)
        {
        
        }
        public override void OnRead(JObject message, Room room)
        {
            var m = message.ToObject<OVarMessage>();
            if(m.a == "up"){
                room.oVarManager.OnUpdate(m);
            }
            if(m.a == "add"){
                room.oVarManager.AddedInGolf(m.i,m.v);
            }
        }
    }
}