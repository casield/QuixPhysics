using System.Numerics;

namespace QuixPhysics
{
    public class PunchGauntle : Gauntlet
    {
        public PunchGauntle()
        {
            name = "punch";
        }
        public override void Init()
        {

        }

        public override void OnActivate()
        {

        }
        public override void Activate(bool active)
        {
            if(!active)return;
            QuixConsole.Log("Activate punch");
            player.dummy.dummyBody.arm.animation.RestartAnimation();
        }

        public override void OnChange()
        {

        }

        public override void Swipe(double degree, Vector3 direction)
        {

        }
    }
}