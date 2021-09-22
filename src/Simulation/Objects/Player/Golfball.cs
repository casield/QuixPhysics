using System;
using BepuPhysics;
using BepuPhysics.Constraints;

namespace QuixPhysics
{
    public class GolfBall2 : PhyObject
    {

        private Player2 player;


        public GolfBall2()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
             simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(10000f, 1000f);


            bodyReference = GetBodyReference();
        }
        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
            base.OnObjectMessage(data, clientId, roomId);
            QuixConsole.Log("Mensaje para bola");
            player.lookObject.ChangeWatching(this);

        }
        public void SetPlayer(Player2 player2)
        {
            this.player = player2;
        }
    }
}