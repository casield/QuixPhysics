namespace QuixPhysics.Player
{
    public class FallAction : PlayerAction
    {
        User user;
        public FallAction(Player2 player) : base(player)
        {
            user = player.user;
        }

        private void OnFall()
        {
            if (this.user != null)
            {
                this.user.gems.Update(((int)this.user.gems.value) / 2);
            }

        }

        public override void OnActivate()
        {
           
        }

        public override void OnUpdate()
        {
            if (player.GetPosition().Y < -50)
            {
                player.SetPositionToStartPoint();
                OnFall();
            }

            if (player.golfball != null)
            {
                if (this.player.golfball.GetPosition().Y < -50)
                {
                    player.golfball.SetPosition(player.GetPosition());
                    OnFall();
                }
            }
        }
    }
}