using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics{
    public class CreateCommand : Command
    {
        private JsonSerializer serializer;

        public CreateCommand(Simulator _simulator) : base(_simulator)
        {
            serializer = new JsonSerializer();
        }
        public override void OnRead(JObject message)
        {
            QuixConsole.Log("Message",message);
            if (message["halfSize"] != null)
            {
                 
                BoxState ob = message.ToObject<BoxState>();

                simulator.Create(ob);
            }

            if (message["radius"] != null)
            {
                
                SphereState ob = message.ToObject<SphereState>();

                simulator.Create(ob);
                
            }
        }
    }
}