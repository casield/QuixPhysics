using System.Numerics;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class GenerateMapCommand : Command
    {
        public GenerateMapCommand(Simulator _simulator) : base(_simulator)
        {

        }
        public override void OnRead(JObject message, Room room)
        {
           GenerateMap(message["name"].ToString(),room);
        }
        public void GenerateMap(string name,Room room)
        {
            var map = this.simulator.server.dataBase.GetMap(name);
            room.map = map;

            foreach (var obj in map.objects)
            {

                //obj.ToJson();
                if (obj.Contains("halfSize"))
                {

                    obj["halfSize"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<BoxState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());

                    this.simulator.Create(stri,room);
                }
                if (obj.Contains("radius"))
                {

                    // obj["radius"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<SphereState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());

                    this.simulator.Create(stri,room);
                }
            }
        }
    }
}