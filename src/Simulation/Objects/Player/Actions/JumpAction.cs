namespace QuixPhysics.Player
{
    public class JumpAction : PlayerAction
    {
        public JumpAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {
            player.bodyReference.Awake = true;

            player.bodyReference.Velocity.Linear.Y += 10;
        }

        public override void OnUpdate()
        {
           
        }
    }
}