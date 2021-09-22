using System;
using System.Numerics;

namespace QuixPhysics
{
    /// <summary>
    /// With this item the helper will go and throw the ball to the player
    /// </summary>
    public class HI_GolfHat : HelperItem
    {
        Helper helper;
        private GolfBall2 golfBall;
        private Player2 player;
        private bool canThrow = true;

        public HI_GolfHat(Helper helper) : base(helper)
        {
            this.helper = helper;
            helper.owner.player.actionsManager.grabBallAction.GrabListeners += OnGrab;
        }

        private void OnGrab()
        {
            QuixConsole.Log("OnGrab");
            canThrow = true;
        }

        private bool GolfBallIsClose()
        {
            return (helper.Distance(golfBall.GetPosition()) < 100);
        }
        private void ThrowBallToPlayer()
        {
            float distance = Vector3.Distance(golfBall.GetPosition(),player.GetPosition())/8;
            Vector3 impulse = Vector3.Normalize( player.GetPosition()-golfBall.GetPosition());
            impulse.Y += .1f;
            golfBall.bodyReference.ApplyLinearImpulse((impulse) * distance);
            QuixConsole.Log("Throw ball",distance);
            canThrow = false;
            Desactivate();
        }
        public void GrabBall()
        {
            player.golfball.Awake();
            float distance = 60 - .5f;
            var newPos = helper.GetPosition();
            var x = -(float)Math.Cos(1);
            var y = -(float)Math.Sin(1);

            newPos.X += x * distance;
            newPos.Z += y * distance;
            newPos.Y += player.playerStats.height;

            newPos = Vector3.Lerp(player.golfball.GetPosition(), newPos, .9f);

            player.golfball.SetPosition(newPos);

            player.golfball.Stop();
        }

        public override void Activate()
        {
            FollowGolfBall();
        }

        public override void Instantiate(Room room, Vector3 position)
        {

        }

        public override bool OnLastPolygon()
        {
            helper.vehicle.SeekFlee(golfBall.GetPosition(), true);
            if (GolfBallIsClose())
            {
                GrabBall();
                ThrowBallToPlayer();
                return true;
            }

            return false;
        }

        public override void OnStuck()
        {
            QuixConsole.Log("Stuck in GolfHat");
            FollowGolfBall();
        }
        private void FollowGolfBall()
        {
            helper.FollowTarget(golfBall);
        }
        private bool IsGrabbed()
        {
            return helper.owner.player.actionsManager.grabBallAction.IsGrabbing;
        }

        public override void OnTrailActive()
        {
            if (IsGrabbed())
            {
                Desactivate();
            }

            if (Vector3.Distance(helper.trail.GetLastPoint().Position, golfBall.GetPosition()) > 100)
            {
                FollowGolfBall();
            }
            helper.vehicle.Arrive(helper.trail.GetPoint());
        }

        public override void OnTrailInactive()
        {
            FollowGolfBall();
        }

        public override bool ShouldActivate()
        {
            if (canThrow)
            {


                KnowledgeInfo info = this.helper.knowledge.KnownsThisObject(helper.owner.player);
                if (info.found_object != null)
                {
                    golfBall = helper.owner.player.golfball;
                    player = helper.owner.player;
                    if (!player.actionsManager.grabBallAction.IsGrabbing)
                    {
                        if (Vector3.Distance(helper.owner.player.GetPosition(), golfBall.GetPosition()) > 100)
                        {
                            QuixConsole.Log("Going for the ball");
                            return true;
                        }
                    }


                }
            }

            return false;
        }
    }
}