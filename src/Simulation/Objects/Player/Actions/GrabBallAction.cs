using System;
using System.Numerics;

namespace QuixPhysics.Player
{
    public class GrabBallAction : PlayerAction
    {
        public bool IsGrabbing = false;
        public  delegate void GrabAction();
        public event GrabAction GrabListeners;
        public GrabBallAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {
            player.actionsManager.shootAction.ShootListeners += OnShoot;
        }

        private void OnShoot(ShootMessage message)
        {
            QuixConsole.Log("Is grabbing false");
            IsGrabbing = false;
        }

        /// <summary>
        /// Sets the ball around the player
        /// </summary>

        public void GrabBall()
        {
            player.golfball.Awake();
            float distance = 20 - .5f;
            var newPos = player.GetPosition();
            var x = -(float)Math.Cos(player.rotationController);
            var y = -(float)Math.Sin(player.rotationController);

            newPos.X += x * distance;
            newPos.Z += y * distance;
            newPos.Y +=player.playerStats.height;

            newPos = Vector3.Lerp(player.golfball.GetPosition(),newPos,.9f);

            player.golfball.SetPosition(newPos);

            player.golfball.Stop();
            IsGrabbing=true;
        }

        public override void OnUpdate()
        {
            if (!IsGrabbing)
            {
                if (Vector3.Distance(player.GetPosition(), player.golfball.GetPosition()) <= player.playerStats.maxDistanceWithBall)
                {
                    IsGrabbing = true;
                     GrabListeners?.Invoke();
                }
            }
            else
            {
                GrabBall();
            }

        }
    }
}