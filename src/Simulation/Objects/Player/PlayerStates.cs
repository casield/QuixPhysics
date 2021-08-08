using System;
using System.Numerics;
using BepuUtilities;

namespace QuixPhysics
{

    class SnappedState : AgentState
    {
        private Player2 player;

        public SnappedState(Player2 player)
        {
            this.player = player;
        }
        public void OnActivate()
        {
            // throw new NotImplementedException();
            PhyTimeOut timeOut = new PhyTimeOut(10, player.simulator,true);
            timeOut.Completed += SendMessage;
            Snap();

        }

        private void SendMessage()
        {
            player.SendObjectMessage("Snap_true");
        }

        private void Snap()
        {
           // player.collidable = false; 
           player.SetPositionToBall();
            player.Stop();
            player.golfball.Stop();
           
        }

        public void OnDesactivate()
        {
            //  throw new NotImplementedException();
        }

        public void OnRepeat()
        {
            Snap();
        }

        public void Tick()
        {
            Snap();
        }
    }
    class Not_SnappedState : AgentState
    {
        private Player2 player;

        public Not_SnappedState(Player2 player)
        {
            this.player = player;
        }
        public void OnActivate()
        {

            SendMessage();
        }

        private void SendMessage()
        {
           // player.collidable = true;
            player.SendObjectMessage("Snap_false");
            // Console.WriteLine("Sending");
        }


        public void OnDesactivate()
        {
            //  throw new NotImplementedException();
        }

        public void OnRepeat()
        {
            // throw new NotImplementedException();
            SendMessage();

            
        }

        public void Tick()
        {
            //  throw new NotImplementedException();
        }
    }

    class JumpState : AgentState
    {
        private Player2 phy;

        public JumpState(Player2 phy)
        {
            this.phy = phy;
            phy.ContactListeners += OnContact;
        }
        private void OnContact(PhyObject obj)
        {
            //phy.Agent.Unlock();
        }
        public void OnActivate()
        {
            
            Jump();
        }
        private void Jump()
        {
           
            phy.bodyReference.Awake = true;
            phy.bodyReference.Velocity.Linear.Y += 50;
        }

        public void OnDesactivate()
        {
            // throw new NotImplementedException();
        }

        public void OnRepeat()
        {
            //throw new NotImplementedException();
            Jump();
        }

        public void Tick()
        {
            //throw new NotImplementedException();
        }
    }
    class ShootState : AgentState
    {
        internal ShootMessage message;
        private Player2 player;
        private PhyTimeOut setCollidedTimeout;

        public ShootState(Player2 player2)
        {
            this.player = player2;

        }

        private void SetPlayerCollided()
        {
          //  player.collidable = true;
        }

        public void OnActivate()
        {
            // throw new System.NotImplementedException();
            Shoot();
        }

        public void Shoot()
        {
   
            var radian = (player.rotationController);
            var x = (float)Math.Cos(radian);
            var y = (float)Math.Sin(radian);
            double ximp = -(x * player.playerStats.force) * message.force;
            double zimp = -(y * player.playerStats.force) * message.force;
            double yimp = (player.playerStats.force * .6f) * message.force;

            QuixConsole.Log("Force",message.force);


            Vector3 imp = new Vector3((float)ximp,(float)yimp,(float)zimp);
            player.golfball.GetBodyReference().ApplyLinearImpulse(imp);
            SetPlayerCollided();
            player.SetNotSnapped();
        }

        public void OnDesactivate()
        {
            //throw new System.NotImplementedException();
        }

        public void OnRepeat()
        {
            // throw new System.NotImplementedException();
            Shoot();
        }

        public void Tick()
        {

            // throw new System.NotImplementedException();
        }
    }
}