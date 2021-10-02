using System;
using System.Numerics;

namespace QuixPhysics{
    public class Quixtam:MeshBox {
        Quaternion q = new Quaternion(new Vector3(0,0,.01f),.1f);
        float rotation = 0;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            QuixConsole.Log("Quixtam Created");
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(30,simulator)).Completed+=Update;
        }
        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
            QuixConsole.Log("Quixtam", data, clientId, roomId);
            Vector3 pos = GetStaticReference().Pose.Position;
            room.users[clientId].player.lookObject.ChangeWatching(this);
        }

        private void Update()
        {
            if(staticReference.Exists){
               staticDescription.Pose.Orientation =Quaternion.Normalize(staticDescription.Pose.Orientation*q) ;
               //staticDescription.Pose.Orientation = Quaternion.Normalize(staticDescription.Pose.Orientation);
                needUpdate=true;
                simulator.Simulation.Statics.ApplyDescription(handle.staticHandle,staticDescription);
                rotation+=.1f;
            }
        }
    }
}