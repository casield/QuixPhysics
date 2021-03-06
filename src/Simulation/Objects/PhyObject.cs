using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
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
        public Handle handle;
        internal Simulator simulator;
        /// <summary>
        /// This variable is used to know if the simulation quaternion should be adden when sending JSON to the server
        /// </summary>
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
        public bool Destroyed = false;
        public bool isDestroying = false;


        public PhyObject()
        {

        }
        /// <summary>
        /// This method is called before the creation of the body in the simulation. It should be used to change the state before initialization.
        /// </summary>
        /// <param name="state"></param>
        public virtual void BeforeLoad(ObjectState state)
        {

        }
        /// <summary>
        /// This method is called when the object is instantiated on the factory.
        /// </summary>
        /// <param name="bodyHandle"></param>
        /// <param name="connectionState"></param>
        /// <param name="simulator"></param>
        /// <param name="state"></param>
        /// <param name="guid"></param>
        /// <param name="room"></param>
        public virtual void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            Constructor(connectionState, simulator, room);
            needUpdate = true;
            this.handle = bodyHandle;
            this.state = state;
            this.guid = guid;

            SetReference();
            SetDescription();


            SendCreateMessage();
        }

        /// <summary>
        /// This method intitiates the PhyObject. It should be called when an instance of the phyobject is active and the factory instantiates it.
        /// </summary>
        /// <param name="connectionState"></param>
        /// <param name="simulator"></param>
        /// <param name="state"></param>
        /// <param name="guid"></param>
        /// <param name="room"></param>
        public virtual void Constructor(ConnectionState connectionState, Simulator simulator, Room room)
        {
            this.connectionState = connectionState;


            this.simulator = simulator;


            this.room = room;
        }
        /// <summary>
        /// Set collision detections for this object
        /// </summary>
        /// <param name="collidable"></param>
        public void SetCollidable(bool collidable)
        {
            if (state.mass != 0)
            {
                simulator.collidableMaterials[handle.bodyHandle].collidable = collidable;
            }
            else
            {
                simulator.collidableMaterials[handle.staticHandle].collidable = collidable;
            }

        }

        private void SetDescription()
        {
            if (state.mass == 0)
            {
                StaticDescription description;
                simulator.Simulation.Statics.GetDescription(handle.staticHandle, out description);
                staticDescription = description;
            }
            else
            {
                BodyDescription description;
                simulator.Simulation.Bodies.GetDescription(handle.bodyHandle, out description);
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

        public void AddToContactListener()
        {
            if (room.factory != null)
            {
                room.factory.OnContactListeners.Add(guid, this);
            }
        }

        public void SetPosition(Vector3 position)
        {

            if (state.mass == 0)
            {

                staticDescription.Pose.Position = position;
                simulator.Simulation.Statics.ApplyDescription(handle.staticHandle, staticDescription);
                if (state.instantiate)
                {
                    needUpdate = true;
                }

            }
            else
            {
                bodyReference.Pose.Position = position;
            }
        }

        public void SetQuaternion(Quaternion quaternion)
        {

            if (state.mass == 0)
            {

                staticDescription.Pose.Orientation = quaternion;
                simulator.Simulation.Statics.ApplyDescription(handle.staticHandle, staticDescription);
                needUpdate = true;
            }
            else
            {
                bodyReference.Pose.Orientation = quaternion;
            }
        }
        public Vector3 GetPosition()
        {
            if (state.mass == 0)
            {
                if (staticReference.Exists)
                {
                    return staticReference.Pose.Position;
                }


            }
            else
            {
                if (bodyReference.Exists)
                {
                    return bodyReference.Pose.Position;
                }

            }
            return Vector3.Zero;
        }
        public Vector3 GetVelocity()
        {

            if (state.mass != 0)
            {
                if (bodyReference.Exists)
                {
                    return bodyReference.Velocity.Linear;
                }
            }
            return Vector3.Zero;
        }

        internal Vector3 GetForward()
        {
            var v = Vector3.Normalize(GetVelocity());
            v.Y = 0;
            return v;
        }

        public float GetHeight()
        {
            if (state is SphereState)
            {
                return ((SphereState)state).radius * 2;
            }
            else
            {
                return ((BoxState)state).halfSize.Y * 2;
            }
        }


        public BodyReference GetBodyReference()
        {
            if (state.mass == 0)
            {
                throw new Exception(state.type + " is not dynamic");
            }
            return simulator.Simulation.Bodies.GetBodyReference(handle.bodyHandle);
        }

        public StaticReference GetStaticReference()
        {
            if (state.mass != 0)
            {
                throw new Exception(state.type + " is not static");
            }
            return simulator.Simulation.Statics.GetStaticReference(handle.staticHandle);
        }



        public void Stop()
        {

            GetBodyReference().Velocity.Linear = Vector3.Zero;
        }
        public void Awake()
        {
            if (state.mass != 0)
            {
                simulator.Simulation.Awakener.AwakenBody(handle.bodyHandle);
            }
        }



        internal void SendCreateMessage()
        {
            if (state.instantiate)
            {
                simulator.SendMessage("create", state.getJson(), connectionState.workSocket);
            }

        }
        /// <summary>
        /// This method should be used to change the state before sending it to the Golf-Server
        /// </summary>
        public virtual void ChangeStateBeforeSend()
        {

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

        public Quaternion GetQuaternion()
        {
            if (state.mass != 0)
            {
                return bodyReference.Pose.Orientation;
            }
            else
            {
                return staticReference.Pose.Orientation;
            }
        }




        public virtual string getJSON()
        {

            state.position = GetPosition();
            if (updateRotation)
            {
                state.quaternion = GetQuaternion();
            }
            ChangeStateBeforeSend();
            return state.getJson();
        }




        public static string createUID()
        {

            return Guid.NewGuid().ToString().GetHashCode().ToString();
        }

        /// <summary>
        /// On contact is called if it's registred on Rooms.Factory OnContact listener.
        /// </summary>
        public virtual void OnContact<TManifold>(PhyObject obj, TManifold manifold) where TManifold : struct, IContactManifold<TManifold>
        {
            throw new NotImplementedException("Type :" + obj.state.type);
        }
        /// <summary>
        /// Destroys this object in the next simulation tick
        /// </summary>
        public void DestroyOnTime(int time)
        {
            if(isDestroying)return;
            AddWorker(new PhyTimeOut(time, simulator, true)).Completed += () =>
            {
                Destroy();
            };
            isDestroying = true;
        }

        public virtual void Destroy()
        {
            if(Destroyed)return;
            if (state.mass == 0)
            {
                room.factory.staticObjectsHandlers.Remove(handle.staticHandle);
                simulator.Simulation.Statics.Remove(handle.staticHandle);
            }
            else
            {
                room.factory.objectsHandlers.Remove(handle.bodyHandle);
                simulator.Simulation.Bodies.Remove(handle.bodyHandle);
            }
            room.factory.objects.Remove(state.uID);

            JObject j = new JObject();
            j["uID"] = state.uID;
            if (state.instantiate)
            {
                simulator.SendMessage("delete", j, connectionState.workSocket);
            }



            foreach (var item in workers)
            {
                item.Destroy();
            }
            OnDelete?.Invoke(this);
            Destroyed = true;

        }


    }

}