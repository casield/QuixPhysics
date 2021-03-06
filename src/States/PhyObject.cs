using System;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class PhyObject
    {
        public string uID;
        public BodyHandle bodyHandle;
        public BodyDescription description;
        private Simulator simulator;

        public PhyObject(BodyHandle bodyHandle, BodyDescription description, StateObject state,Simulator simulator)
        {
            this.bodyHandle = bodyHandle;
            
            this.description = description;
            this.simulator = simulator;
            uID = createUID();

            simulator.SendMessage("createBox",getJSON());
        }

        public JObject getJSON()
        {
            simulator.Simulation.Bodies.GetDescription(bodyHandle, out description);
            JObject o = new JObject();
            o["uID"] = uID;
            o["position"] = JsonConvert.SerializeObject(description.Pose.Position);

            return o;
        } 

        public string createUID()
        {
            var bytes = new byte[5];
            var random = new Random();
            random.NextBytes(bytes);
            var idStr = (Math.Floor((double)(random.Next() * 25)) + 10).ToString() + "_";
            // add a timestamp in milliseconds (base 36 again) as the base
            idStr += (Math.Floor((double)(random.Next() * 25)) + 10).ToString();

            return idStr;
        }
    }
}