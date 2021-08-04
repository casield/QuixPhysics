using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class LookObject : PhyObject
    {
        private Player2 player2;
        private StaticReference staticReference;

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle,connectionState,simulator,state,guid,room);
            staticReference = GetStaticReference();
        }

        public void SetPlayer(Player2 player2){
            this.player2 = player2;
            QuixConsole.Log("Position",player2.reference.Pose.Position);
        
        }

        public void FollowBall(){
             staticReference.Pose.Position = new Vector3(player2.golfball.reference.Pose.Position.X,player2.golfball.reference.Pose.Position.Y,player2.golfball.reference.Pose.Position.Z);
            
           // QuixConsole.Log("Posi",staticReference.Pose.Position);
            needUpdate=true;
        }
    }
}