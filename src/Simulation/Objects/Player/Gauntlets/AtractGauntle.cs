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
        Boolean hasStopped = false;
        public void Activate(bool active)
        {
            if (!isInit())
            {
                var timer = new PhyInterval(1, Player.simulator);
                timer.Completed += OnTick;
                golfBallRef = Player.golfball.GetReference();
                vehicle = new Vehicle(Player.golfball);
                Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
            }
            if (isInit())
            {
                //golfBallRef.
                QuixConsole.Log("Gauntlet", active);
                vehicle.isActive = active;
                Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
            }
            if(!hasStopped){
               // Player.golfball.Stop();
                hasStopped = true;
            }
            if(!active){
                hasStopped = false;
            }
            
        }
        public void Swipe(double degree,Vector3 dir)
        {
            if (isInit())
            {
                Player.simulator.Simulation.Awakener.AwakenBody(golfBallRef.Handle);
                float force = 70;
                QuixConsole.Log("Swipé log", degree);
                Vector2 rot2d = Player.GetXYRotation();
                Vector3 rot = new Vector3(rot2d.X,0,rot2d.Y) ;
               /* golfBallRef.Velocity.Linear.Y += force;
                golfBallRef.Velocity.Linear.X -= rot.X*force;
                golfBallRef.Velocity.Linear.Z -= rot.Y*force;*/

                Quaternion quat = QuaternionEx.CreateFromAxisAngle( rot,(float)degree);
                Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(quat);

                Vector3 result = Vector3.Transform(rot,matrix);

                QuixConsole.Log("direction",dir);

                dir.X *=-1;
                dir.Y *=-1;
                dir.Z *=-1;

                golfBallRef.Velocity.Linear.Y += dir.Y*force;
                golfBallRef.Velocity.Linear.X += dir.X*force;
                golfBallRef.Velocity.Linear.Z += dir.Z*force;

                QuixConsole.Log("Result",result);
                

                

               
            }

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