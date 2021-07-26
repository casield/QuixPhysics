using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace QuixPhysics
{
    public class AtractGauntlet : IGauntlet
    {
        Player2 Player;
        BodyReference golfBallRef;
        Vehicle vehicle;
        private PhyTimeOut shootInterval;
        private bool canUse = false;

        private bool hasCharge = false;

        public void Init()
        {
            var timer = new PhyInterval(1, Player.simulator);
            timer.Completed += OnTick;
            golfBallRef = Player.golfball.GetReference();
            vehicle = new Vehicle(Player.golfball);
            vehicle.isActive = false;
            Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
            Player.ShootListeners += OnShoot;
        }
        public void Activate(bool active)
        {
            if (hasCharge)
            {
                
                
                CreateInterval();
                canUse = true;
                hasCharge = false;
                
                
            }
            if (canUse)
            {
                vehicle.isActive = active;
                if (active)
                {
                    Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
                }
                Player.cameraLocked = true;
            }


        }
        public void Swipe(double degree, Vector3 dir)
        {
            if (canUse)
            {
                hasCharge = false;
                Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
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
            canUse = true;
            hasCharge = true;
            Player.cameraLocked = true;
            CreateInterval();
            

        }

        private void CreateInterval()
        {
            if (shootInterval == null)
            {
                shootInterval = new PhyTimeOut(2000, Player.simulator, true);
                shootInterval.Completed += OnShooTimeCompleted;
            }else{
                if(hasCharge){
                    shootInterval.Reset();
                }
                
            }
        }

        private void OnShooTimeCompleted()
        {
            canUse = false;
            shootInterval = null;

            vehicle.isActive = false;
            Player.cameraLocked = false;
        }

        private bool isInit()
        {
            return Player != null && vehicle != null;
        }

        private void OnTick()
        {
            if (Player != null)
            {

                vehicle.Seek(Player.reference.Pose.Position);
                vehicle.Update();
                if (Player.IsSnapped())
                {

                    vehicle.isActive = false;
                }
            }
        }

        public void AddPlayer(Player2 player)
        {
            this.Player = player;
        }
    }
}