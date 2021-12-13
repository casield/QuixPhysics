using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace QuixPhysics
{
    public class AtractGauntlet : Gauntlet
    {
        BodyReference golfBallRef;
        Vehicle vehicle;
        private PhyTimeOut shootInterval;

        private bool hasCharge = false;

        public bool infinite = true;
        private byte maxForce = 8;


        public AtractGauntlet()
        {
            name = "atract";
        }

        public override void Init()
        {
            if (player.golfball != null)
            {

                golfBallRef = player.golfball.GetBodyReference();
                vehicle = new Vehicle(player.golfball, new VehicleProps() { maxSpeed = new Vector3(.4f, .7f, .4f) });
                vehicle.isActive = false;
                player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
                player.actionsManager.shootAction.ShootListeners += OnShoot;
            }

        }


        public override void Activate(bool active)
        {
            
                if (hasCharge || infinite)
                {


                    CreateInterval();
                    isActive = true;
                    hasCharge = false;


                }
                if (isActive)
                {
                    vehicle.isActive = active;
                    if (active)
                    {
                        player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
                    }

                    if (!infinite)
                    {
                        player.cameraLocked = true;
                    }

                }
            
        }
        public override void Swipe(double degree, Vector3 dir)
        {
            if (isActive)
            {

                hasCharge = false;
                player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
                float force = 40;
                QuixConsole.Log("Swipé log", degree);

                dir.X *= -1;
                dir.Y *= -1;
                dir.Z *= -1;

                golfBallRef.Velocity.Linear.Y += dir.Y * (force);
                golfBallRef.Velocity.Linear.X = dir.X * force;
                golfBallRef.Velocity.Linear.Z = dir.Z * force;


            }

        }

        private void OnShoot(ShootMessage message)
        {
            isActive = true;
            hasCharge = true;
            if (!(player.lookObject.watching is Player2))
            { player.cameraLocked = true; }
            CreateInterval();


        }

        private void CreateInterval()
        {
            if (shootInterval == null)
            {
                shootInterval = new PhyTimeOut(2000, player.simulator, true);
                shootInterval.Completed += OnShooTimeCompleted;
            }
            else
            {
                if (hasCharge)
                {
                    shootInterval.Reset();
                }

            }
        }

        private void OnShooTimeCompleted()
        {
            if (!infinite)
            {
                isActive = false;
                vehicle.isActive = false;

            }
            shootInterval = null;
            player.cameraLocked = false;
        }

        private bool isInit()
        {
            return player != null && vehicle != null;
        }
        private bool IsGrabbing()
        {
            return (player.actionsManager.grabBallAction.IsGrabbing);

        }

        public override void Update()
        {
            if (player != null && player.bodyReference.Exists)
            {
                PhyObject whoToLook = player;
                vehicle.Arrive(whoToLook.GetPosition());
                vehicle.Update();
                if (IsGrabbing())
                {

                    vehicle.isActive = false;
                }
            }
        }

        public override void OnActivate()
        {
            AddUpdateWorker();
        }

        public override void OnChange()
        {
            RemoveUpdateWorker();
        }
    }
}