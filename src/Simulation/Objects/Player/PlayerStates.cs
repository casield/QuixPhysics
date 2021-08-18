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
            PhyTimeOut timeOut = new PhyTimeOut(10, player.simulator, true);
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
        private Player2 player;

        public JumpState(Player2 phy)
        {
            this.player = phy;
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

            player.bodyReference.Awake = true;
            
            player.bodyReference.Velocity.Linear.Y+=50;
            //player.lookObject.SetPosition(player.lookObject.GetPosition()+new Vector3(0,1000,0));
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
        private PhyTimeOut CollidedTimeout;
        public float maxForce = 8;

        public ShootState(Player2 player2)
        {
            this.player = player2;

        }

        public void OnActivate()
        {
            // throw new System.NotImplementedException();
            Shoot();
        }

        public void Shoot()
        {
            message.force = Math.Clamp(message.force,0f,maxForce);
            Vector3 directionToLookObj = Vector3.Normalize(player.golfball.GetPosition() - player.lookObject.GetPosition()) * new Vector3(-1);
            var impulse = (directionToLookObj * (player.playerStats.force)) *message.force;
            player.golfball.GetBodyReference().ApplyLinearImpulse(impulse);
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