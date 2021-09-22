using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class JumpAction : PlayerAction
    {
        private PhyWaiter jumpWaiter;
        private PhyWaiter jumpsLimitWaiter;
        private static int maxJumps = 3;
        private int jumps = maxJumps;
        private bool canJump = true;
        private PhyObject lastContacted;
        public Vector3 lastNormal = normalJump;
        private static Vector3 normalJump = new Vector3(0, 1, 0);

        public JumpAction(Player2 player) : base(player)
        {
            jumpWaiter = new PhyWaiter(300);
            jumpsLimitWaiter = new PhyWaiter(1000);
            player.ContactListeners += OnContact;


        }

        private void OnRaycast(PhyObject obj, Vector3 normal)
        {
            // lastNormal = normal;

        }

        private void OnContact(PhyObject obj, Vector3 normal)
        {

            if (jumps <= 0 && lastContacted != obj)
            {
                canJump = true;
                jumps = 1;
                lastContacted = obj;


            }
            lastNormal = normal;
        }

        internal void ResetNormal()
        {
            lastNormal = normalJump;
        }

        public void Jump(Vector3 direction)
        {
            player.bodyReference.Awake = true;

            if (direction != Vector3.Zero)
            {
                direction = Vector3.Normalize(direction);
            }

            player.bodyReference.Velocity.Linear += direction * player.playerStats.force;
            
            canJump = false;
            jumps--;
            ResetNormal();

        }

        public override void OnActivate()
        {
            if (canJump && jumps > 0)
            {
                Jump(lastNormal);
            }


        }

        public override void OnUpdate()
        {
            if (!canJump)
            {
                if (jumpWaiter.Tick())
                {
                    jumpWaiter.Reset();
                    canJump = true;
                    lastContacted = null;
                    lastNormal = normalJump;
                }

            }
            if (jumps < maxJumps)
            {
                if (jumpsLimitWaiter.Tick())
                {
                    jumps++;
                }
            }
        }
    }
}