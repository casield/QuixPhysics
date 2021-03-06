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
        public override void OnRead(JObject message, Room room)
        {
            
            if (message["halfSize"] != null)
            {
                 
                BoxState ob = message.ToObject<BoxState>();

                room.Create(ob);
            }

            if (message["radius"] != null)
            {
                
                SphereState ob = message.ToObject<SphereState>();

                room.Create(ob);
                
            }
        }
    }
}