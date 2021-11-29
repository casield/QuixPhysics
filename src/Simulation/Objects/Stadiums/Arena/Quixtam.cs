using System;
using System.Numerics;

namespace QuixPhysics{
    public class QuixtamHitpoint:PhyObject{

        private Quixtam quixtam;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(guid,this);
            SetCollidable(false);
        }

        public void SetQuixtam(Quixtam quixtam){
            this.quixtam = quixtam;
        }

        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
           if(obj is GolfBall2){
               QuixConsole.Log("Touched a ball!");
               
           }
        }
    }
    public class Quixtam:MeshBox {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(0,.1f,0);
        Quaternion q2 = Quaternion.CreateFromYawPitchRoll(0,0f,.1f);
        float rotation = 0;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            QuixConsole.Log("Quixtam Created");
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(50,simulator)).Completed+=Update;
            AddHitpoint();
        }

        public void AddHitpoint(){
            var state = new SphereState(){
                radius=50,
                position=GetPosition(),
                instantiate=true,
                type="QuixtamHitpoint"
            };
           QuixtamHitpoint hitpoint= (QuixtamHitpoint)room.factory.Create(state,room);
           hitpoint.SetQuixtam(this);
        }
        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
            QuixConsole.Log("Quixtam", data, clientId, roomId);
            Vector3 pos = GetStaticReference().Pose.Position;
            room.users[clientId].player.lookObject.ChangeWatching(this);
        }

        public static BoxState Build(Vector3 pos){
            var state = new BoxState(){
                instantiate=true,
                position=pos,
                halfSize=new Vector3(100),
                type="Quixtam",
                mesh="Objects/Quixtam/Quixtam"
            };
            return state;
        }

        private void Update()
        {
            if(staticReference.Exists){
               staticDescription.Pose.Orientation *=q*q2 ;
               //staticDescription.Pose.Orientation = Quaternion.Normalize(staticDescription.Pose.Orientation);
                needUpdate=true;
                simulator.Simulation.Statics.ApplyDescription(handle.staticHandle,staticDescription);
            }
        }
    }
}