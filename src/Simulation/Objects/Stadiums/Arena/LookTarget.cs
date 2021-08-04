using System;
using System.Numerics;

namespace QuixPhysics{
    public class LookTarget:PhyObject {
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
        }
        internal override void OnObjectMessage(string data,string clientId,string roomId)
        {
            base.OnObjectMessage(data,clientId,roomId);
            QuixConsole.Log("LookTarget",data,clientId,roomId);
            Vector3 pos =  GetStaticReference().Pose.Position;
            room.users[clientId].player.lookObject.ChangeWatching(this);
        }
    }
}