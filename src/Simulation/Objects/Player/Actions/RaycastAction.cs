using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class RaycastAction : PlayerAction
    {
        private Raycast raycast;
        public RaycastAction(Player2 player) : base(player)
        {
            raycast = new Raycast(player.simulator, player.room);
            raycast.SetRayShape(new CircleRayShape(3, 30, raycast));
            raycast.ObjectHitListeners += OnRayCastHit;
        }

        private void OnRayCastHit(PhyObject obj,Vector3 normal)
        {
          
        }

        public override void OnActivate()
        {
        
        }

        public override void OnUpdate()
        {
            var xyrot = player.GetXYRotation();
            // QuixConsole.Log("Rotation player",rotationController);
            var ret = new Vector3(xyrot.X, 0, xyrot.Y);
            raycast.Update(player.lookObject.GetPosition(), ret, player.state.quaternion);
        }
    }
}