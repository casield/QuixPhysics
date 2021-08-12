using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
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
           //GenerateMap(message["name"].ToString(),room);
        }
        public List<PhyObject> GenerateMap(string name,Room room)
        {
            List<PhyObject> objects = new List<PhyObject>();
            var map =  this.simulator.server.dataBase.GetMap(name).ContinueWith(a=>{
                room.map = a.Result;

            

            foreach (var obj in room.map.objects)
            {

                //obj.ToJson();
                if (obj.Contains("halfSize"))
                {

                    obj["halfSize"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<BoxState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());
                    var phy = room.Create(stri);
                     objects.Add(phy);
                }
                if (obj.Contains("radius"))
                {

                    // obj["radius"].AsBsonDocument.Remove("__refId");
                    obj.Remove("_id");
                    var stri = JsonConvert.DeserializeObject<SphereState>(obj.ToJson());
                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());
                    var phy = room.Create(stri);
                     objects.Add(phy);
                }
            }

            
            });
            map.Wait();
            return objects;
        }
    }
}