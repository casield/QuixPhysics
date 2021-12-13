using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class ShootAndGrabAction : PlayerAction
    {
        private byte maxForce = 8;
        private Vehicle vehicle;

        internal delegate void OnShootAction(ShootMessage message);

        internal event OnShootAction ShootListeners;

        private bool IsAtracting = false;

        public ShootAndGrabAction(Player2 player) : base(player)
        {


        }

        public override void OnActivate()
        {
            if (vehicle == null && player.golfball != null)
            {
                vehicle = new Vehicle(player.golfball, new VehicleProps() { maxSpeed = new Vector3(.4f, .7f, .4f) });
                vehicle.isActive = true;
            }

            if(!player.actionsManager.grabBallAction.IsGrabbing && !IsAtracting){
                IsAtracting = true;
            }   

        }

        public void Shoot(ShootMessage message)
        {
            if (player.actionsManager.grabBallAction.IsGrabbing)
            {
                player.golfball.Awake();
                message.force = Math.Clamp(message.force, 0f, maxForce);
                Vector3 directionToLookObj = Vector3.Normalize(player.golfball.GetPosition() - player.lookObject.GetPosition()) * new Vector3(-1);
                var force = new Vector3(player.playerStats.force) * message.force;
                var impulse = (directionToLookObj * force);
                player.golfball.GetBodyReference().ApplyLinearImpulse(impulse);
                ShootListeners?.Invoke(message);
                IsAtracting = false;
            }
        }

        public override void OnUpdate()
        {
            if(!player.actionsManager.grabBallAction.IsGrabbing && IsAtracting){
                 QuixConsole.Log("Not Grabbing");
                if (player.golfball != null)
                {
                    player.simulator.Simulation.Awakener.AwakenBody(player.golfball.handle.bodyHandle);
                    vehicle.isActive = true;
                    vehicle.Arrive(player.GetPosition());
                    vehicle.Update(); 
                }
            }

        }
    }
}