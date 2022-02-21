
using System.Numerics;

namespace QuixPhysics.Player
{
    public class DamageAction : PlayerAction
    {
        private int nextDamage;

        private float savedSpeed = 0;

        public DamageAction(Player2 player) : base(player)
        {
        }

        public void SetDamage(int damage){
            nextDamage = damage;
        }

        public override void OnActivate()
        {
            savedSpeed = player.playerStatsInitial.speed;
            int gems = (int)player.user.gems.value;
            player.user.gems.Update(gems-nextDamage);
            SetDamage(0);
            var gem = new Gem();
            gem.InstantiateFromPlayer(player.room,player);

            player.playerStats.speed = 0;
        }

        public override void OnUpdate()
        {
            if(player.playerStats.speed<savedSpeed){
                player.playerStats.speed+=.0001f;
            }else{
                player.playerStats.speed = savedSpeed;
                Remove();
            }
        }
    }
}