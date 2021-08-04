using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    class ObjectMessageCommand : Command
    {
        public ObjectMessageCommand(Simulator _simulator) : base(_simulator)
        {
        }

        public override void OnRead(JObject message, Room room)
        {
           string uID = message["uID"].ToString();
           string mess = message["message"].ToString();
           string clientId = message["clientId"].ToString();
           string roomId = message["roomId"].ToString();
           
           PhyObject p = simulator.objects[uID];
           QuixConsole.Log("OnRead",p.state.type);

           simulator.objects[uID].OnObjectMessage(mess,clientId,roomId);
        }
    }
}