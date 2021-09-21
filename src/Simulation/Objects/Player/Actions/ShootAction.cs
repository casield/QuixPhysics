using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class ShootAction : PlayerAction
    {
        private byte maxForce = 8;

        internal delegate void OnShootAction(ShootMessage message);

        internal event OnShootAction ShootListeners;

        public ShootAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {

        }

        public void Shoot(ShootMessage message)
        {
            if (player.golfball.isSnapped)
            {
                player.golfball.Awake();
                message.force = Math.Clamp(message.force, 0f, maxForce);
                Vector3 directionToLookObj = Vector3.Normalize(player.golfball.GetPosition() - player.lookObject.GetPosition()) * new Vector3(-1);
                var force = new Vector3(player.playerStats.force * message.force, player.playerStats.force * message.force, (player.playerStats.force) * message.force);
                var impulse = (directionToLookObj * force);
                player.golfball.GetBodyReference().ApplyLinearImpulse(impulse);
                ShootListeners?.Invoke(message);
            }
        }

        public override void OnUpdate()
        {

        }
    }
}