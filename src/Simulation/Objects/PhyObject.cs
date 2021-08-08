using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public delegate void DestroyAction(PhyObject obj);
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
        public BodyReference bodyReference;
        public StaticReference staticReference;
        public Guid guid;

        public Room room;

        public TypedIndex shapeIndex;

        public bool needUpdate = false;

        private List<PhyWorker> workers = new List<PhyWorker>();

        public event DestroyAction OnDelete;

        public StaticDescription staticDescription;
        public BodyDescription bodyDescription;


        public PhyObject()
        {

        }
        public virtual void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            this.connectionState = connectionState;
            this.bodyHandle = bodyHandle;
            this.state = state;
            this.simulator = simulator;
            
            this.guid = guid;
            this.room = room;

            SetReference();
            SetDescription();



            SendCreateMessage();
        }

        private void SetDescription()
        {
            if (state.mass == 0)
            {
                StaticDescription description;
                simulator.Simulation.Statics.GetDescription(bodyHandle.staticHandle, out description);
                staticDescription = description;
            }
            else
            {
                BodyDescription description;
                simulator.Simulation.Bodies.GetDescription(bodyHandle.bodyHandle, out description);
                bodyDescription = description;
            }
        }
        private void SetReference()
        {
            if (state.mass == 0)
            {
                staticReference = GetStaticReference();
            }
            else
            {
                bodyReference = GetBodyReference();
            }
        }

        public PhyWorker AddWorker(PhyWorker worker)
        {

            workers.Add(worker);
            return worker;
        }

        public void SetPosition(Vector3 position)
        {

            if (state.mass == 0)
            {
               
                staticDescription.Pose.Position = position;
                simulator.Simulation.Statics.ApplyDescription(bodyHandle.staticHandle,staticDescription);
            }else{
                bodyReference.Pose.Position = position;
            }
        }
        public Vector3 GetPosition(){
            if(state.mass==0){
                return staticReference.Pose.Position;
            }else{
                return bodyReference.Pose.Position;
            }
        }

        public BodyReference GetBodyReference()
        {
            if(state.mass == 0){
                throw new Exception(state.type+" is not dynamic");
            }
            return simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
        }

        public StaticReference GetStaticReference()
        {
          if(state.mass != 0){
                throw new Exception(state.type+" is not static");
            }
            return simulator.Simulation.Statics.GetStaticReference(bodyHandle.staticHandle);
        }



        public void Stop()
        {

            GetBodyReference().Velocity.Linear = Vector3.Zero;
        }
        public void Awake(){
            if(state.mass!=0){
                simulator.Simulation.Awakener.AwakenBody(bodyHandle.bodyHandle);
            }
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
        internal virtual void OnObjectMessage(string data, string clientId, string roomId)
        {

        }


        public virtual string getJSON()
        {

            if (state.mass != 0)
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

            if (state.mass == 0)
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

            if (state.quaternion.W == 0)
            {
                QuixConsole.Log("Error", state.type);
            }

            return state.getJson();
        }



        public static string createUID()
        {

            return Guid.NewGuid().ToString().GetHashCode().ToString(); ;
        }


        public virtual void OnContact(PhyObject obj)
        {
            throw new NotImplementedException("Type :" + obj.state.type);
        }

        public virtual void Destroy()
        {
            if (state.mass == 0)
            {
                simulator.staticObjectsHandlers.Remove(bodyHandle.staticHandle);
                simulator.Simulation.Statics.Remove(bodyHandle.staticHandle);
            }
            else
            {
                simulator.Simulation.Bodies.Remove(bodyHandle.bodyHandle);
            }
            simulator.objects.Remove(state.uID);

            JObject j = new JObject();
            j["uID"] = state.uID;

            simulator.SendMessage("delete", j, connectionState.workSocket);

            foreach (var item in workers)
            {
                item.Destroy();
            }
            OnDelete?.Invoke(this);

        }


    }

}