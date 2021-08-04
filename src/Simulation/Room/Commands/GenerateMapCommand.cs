using System.Collections.Generic;
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
        public List<PhyObject> GenerateMap(string name,Room room)
        {
            var map = this.simulator.server.dataBase.GetMap(name);
            room.map = map;

            List<PhyObject> objects = new List<PhyObject>();

            foreach (var obj in map.objects)
            {

                //obj.ToJson();
                if (obj.Contains("halfSize"))
                {

                    obj["halfSize"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<BoxState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());
                    var phy = this.simulator.Create(stri,room);
                     objects.Add(phy);
                }
                if (obj.Contains("radius"))
                {

                    // obj["radius"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<SphereState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());
                    var phy = this.simulator.Create(stri,room);
                     objects.Add(phy);
                }
            }

            return objects;
        }
    }
}