using System;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class PhyObject
    {
        public ObjectState state;
        public BodyHandle bodyHandle;
        public BodyDescription description;
        private Simulator simulator;
        internal bool updateRotation = true;

        public PhyObject()
        {

        }
        public void Load(BodyHandle bodyHandle, BodyDescription description, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            this.bodyHandle = bodyHandle;
            this.state = state;
            this.description = description;
            this.simulator = simulator;

            simulator.SendMessage("create", getJSON(), connectionState.workSocket);
        }

        public string getJSON()
        {
            simulator.Simulation.Bodies.GetDescription(bodyHandle, out description);

            state.position = description.Pose.Position;
            if (updateRotation)
            {
                state.quaternion = description.Pose.Orientation;
            }



            return state.getJson();
        }
        public void getDescription(){

        }

        public static string createUID()
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