using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class MoveAction : PlayerAction
    {
        private XYMessage moveMessage;
        private XYMessage forceMoveMessage;
        private float moveAcceleration = 0;

        public MoveAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {
            forceMoveMessage.x = MathF.Abs(moveMessage.x);
            forceMoveMessage.y = MathF.Abs(moveMessage.y);


            if (moveMessage.x != 0 || moveMessage.y != 0)
            {
                var number_of_chunks = 16;
                var size_of_chunk = (360 / number_of_chunks);

                var angle = (float)Math.Atan2(moveMessage.x, moveMessage.y);
                var fx = (float)MathF.Cos(angle);
                var fy = (float)MathF.Sin(angle);
                moveMessage.x = fx * moveMessage.x;
                moveMessage.y = fy * moveMessage.y;
            }
        }
        public void SetMoveMessage(XYMessage message)
        {
            moveMessage = message;
        }

        public override void OnUpdate()
        {
            if (moveMessage.clientId != null)
            {
                // Console.WriteLine(bb.Velocity.Linear.Y);
                if ((moveMessage.x != 0 || moveMessage.y != 0))
                {
                    
                    var radPad = Math.Atan2(this.moveMessage.x, -(this.moveMessage.y));
                    var radian = (player.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);
                    Vector3 vel = new Vector3(x, 0, y);

                    vel.X *= (float)player.playerStats.speed;
                    vel.Z *= (float)player.playerStats.speed;

                    moveAcceleration += player.overStats.acceleration;

                    moveAcceleration = (float)Math.Clamp(moveAcceleration, 0, player.playerStats.maxSpeed);

                    player.bodyReference.Velocity.Linear.X += ((vel.X) * moveAcceleration);
                    player.bodyReference.Velocity.Linear.Z += ((vel.Z) * moveAcceleration);

                    player.bodyReference.Awake = true;

                }
                else
                {
                    moveAcceleration = 0;
                    player.bodyReference.Velocity.Linear.X *= player.playerStats.friction;
                    player.bodyReference.Velocity.Linear.Z *= player.playerStats.friction;
                }

            }
        }
    }
}