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
        
        public JumpAction(Player2 player) : base(player)
        {
            jumpWaiter = new PhyWaiter(300);
            jumpsLimitWaiter = new PhyWaiter(1000);
            player.ContactListeners+=OnContact;
        }

        private void OnContact(PhyObject obj){
            
            if(jumps==0 && lastContacted != obj){
                canJump = true;
                jumps=1;
                lastContacted=obj;
            }
        }

        public override void OnActivate()
        {
            if (canJump && jumps>0)
            {

                player.bodyReference.Awake = true;

                player.bodyReference.Velocity.Linear.Y += player.playerStats.force;
                canJump = false;
                jumps--;

            }


        }

        public override void OnUpdate()
        {
            if(!canJump){
                if(jumpWaiter.Tick()){
                    jumpWaiter.Reset();
                    canJump=true;
                    lastContacted = null;
                }
                
            }
            if(jumps<maxJumps){
                if(jumpsLimitWaiter.Tick()){
                    jumps++;
                }
            }
        }
    }
}