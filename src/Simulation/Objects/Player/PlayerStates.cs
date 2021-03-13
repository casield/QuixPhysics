using System;
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


            PhyTimeOut timeOut = new PhyTimeOut(10, player.simulator);
            timeOut.Completed += SendMessage;
            Snap();
        }

        private void SendMessage()
        {
            player.SendObjectMessage("Snap_true");
        }

        private void Snap()
        {
            player.collidable = false;
            player.Stop();
            player.golfball.Stop();
            player.SetPositionToBall();
        }

        public void OnDesactivate()
        {
            //  throw new NotImplementedException();
        }

        public void OnRepeat()
        {
            // throw new NotImplementedException();
            Snap();
        }

        public void Tick()
        {
            //  throw new NotImplementedException();
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
            player.collidable = true;
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
            phy.reference.Awake = true;
            phy.reference.Velocity.Linear.Y += 50;
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
            player.collidable = true;
        }

        public void OnActivate()
        {
            // throw new System.NotImplementedException();
            Shoot();
        }

        public void Shoot()
        {
            // Console.WriteLine("Shoooot");
            // player.golfball.GetReference().Velocity.Linear.X -= 30;
            setCollidedTimeout = new PhyTimeOut(50, player.simulator);
            setCollidedTimeout.Completed += SetPlayerCollided;

            var radian = (player.rotationController);
            var x = (float)Math.Cos(radian);
            var y = (float)Math.Sin(radian);

            player.golfball.GetReference().Velocity.Linear.X -= (x * player.shootForce) * message.force;
            player.golfball.GetReference().Velocity.Linear.Z -= (y * player.shootForce) * message.force;
            player.golfball.GetReference().Velocity.Linear.Y += (player.shootForce * .34f) * message.force;
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