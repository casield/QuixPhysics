using System;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public interface IPhyObject
    {
        ObjectState state { get; set; }
        Simulator simulator { get; set; }
        bool updateRotation { get; set; }
        ConnectionState connectionState { get; set; }
        SimpleMaterial material { get; set; }
        BodyReference reference { get; set; }
        Guid guid { get; set; }

        Handle handle { get; set; }
        void Load(Handle handle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid);
        BodyReference GetReference();
    }
    public struct Handle
    {
        public BodyHandle bodyHandle { get; set; }
        public StaticHandle staticHandle { get; set; }
    }
    public class PhyObject
    {
        public ObjectState state;
        public Handle bodyHandle;
        internal Simulator simulator;
        internal bool updateRotation = true;
        internal ConnectionState connectionState;
        public SimpleMaterial material;
        public BodyReference reference;
        public Guid guid;

        public Room room;

        public TypedIndex shapeIndex;

        public bool needUpdate = false;
        public PhyObject()
        {

        }
        public virtual void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            this.connectionState = connectionState;
            this.bodyHandle = bodyHandle;
            this.state = state;
            this.simulator = simulator;
            this.reference = GetReference();
            this.guid = guid;
            this.room = room;

            SendCreateMessage();
        }

        public BodyReference GetReference()
        {
            Debug.Assert(state.mass==0,"This phyobject is not dynamic");
            return simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
        }

        public StaticReference GetStaticReference(){
            Debug.Assert(state.mass!=0,"This phyobject is not static");
             return simulator.Simulation.Statics.GetStaticReference(bodyHandle.staticHandle);
        }

        

        public void Stop()
        {

            GetReference().Velocity.Linear = Vector3.Zero;
        }



        internal void SendCreateMessage()
        {
            if (state.instantiate)
            {
                simulator.SendMessage("create", getJSON(), connectionState.workSocket);
            }

        }

        public virtual void SetMeshRotation()
        {
            if (state.isMesh)
            {

                var newq = Quaternion.CreateFromYawPitchRoll(-1.57f, 0, 0);

                //state.quaternion = newq*state.quaternion;
            }
            needUpdate = true;

        }
        internal void SendObjectMessage(string data)
        {
            JObject m = new JObject();
            m["uID"] = state.uID;
            m["data"] = data;
            simulator.SendMessage("objectMessage", m.ToString(), connectionState.workSocket);

        }


        public virtual string getJSON()
        {

            if (state.mass !=0)
            {
                BodyDescription description;
                simulator.Simulation.Bodies.GetDescription(bodyHandle.bodyHandle, out description);

                state.position = description.Pose.Position;
                if (updateRotation)
                {
                    state.quaternion = description.Pose.Orientation;

                }
                SetMeshRotation();

            }
            
            if (state.mass ==0)
            {
                StaticDescription description;
                simulator.Simulation.Statics.GetDescription(bodyHandle.staticHandle, out description);

                state.position = description.Pose.Position;
                if (updateRotation)
                {
                    state.quaternion = description.Pose.Orientation;
                }
                SetMeshRotation();
            }

            return state.getJson();
        }
        public void getDescription()
        {

        }


        public static string createUID()
        {

            return Guid.NewGuid().ToString().GetHashCode().ToString(); ;
        }


        public virtual void OnContact(PhyObject obj)
        {

        }
    }

}