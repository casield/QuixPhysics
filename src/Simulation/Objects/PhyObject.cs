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
        internal Simulator simulator;
        internal bool updateRotation = true;
        internal ConnectionState connectionState;
        public SimpleMaterial material;

        public PhyObject()
        {

        }
        public virtual void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            this.connectionState = connectionState;
            this.bodyHandle = bodyHandle;
            this.state = state;
            this.simulator = simulator;

            SendCreateMessage();
        }



        internal void SendCreateMessage(){
            if(state.instantiate){
               simulator.SendMessage("create", getJSON(), connectionState.workSocket); 
            }
            
        }


        public string getJSON()
        {
            BodyDescription description;
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

        public virtual void Move(MoveMessage message){

        }
    }

    public class StaticPhyObject:PhyObject{
       new StaticHandle bodyHandle;
       public virtual void Load(StaticHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state){
            this.connectionState = connectionState;
            this.bodyHandle = bodyHandle;
            this.state = state;
            this.simulator = simulator;

            SendCreateMessage();
       }

        new public string getJSON()
        {
            StaticDescription description;
            simulator.Simulation.Statics.GetDescription(bodyHandle, out description);

            state.position = description.Pose.Position;
            if (updateRotation)
            {
                state.quaternion = description.Pose.Orientation;
            }

            return state.getJson();
        }
        new public void SendCreateMessage(){
              if(state.instantiate){
               simulator.SendMessage("create", getJSON(), connectionState.workSocket); 
            }
        }

    }
}