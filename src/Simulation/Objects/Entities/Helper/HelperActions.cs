using System.Numerics;

namespace QuixPhysics
{

    public class HelperAction : EntityLifeLoop
    {
        Helper helper;
        public HelperAction(Helper helper)
        {
            this.helper = helper;
        }
        public bool OnLastPolygon()
        {
            GoRandomPoint();

            return true;
        }

        private void GoRandomPoint()
        {
            helper.trail.Start();
            var randNav = helper.trail.GetRandomPoint(helper.GetPosition(),new Vector3(500, 500, 500));
            helper.trail.SetTarget(randNav.Position);
        }

        public void OnTrailActive()
        {
            helper.vehicle.SeekFlee(helper.trail.GetPoint(), true);
        }

        public void OnTrailInactive()
        {
            if (helper.trail.PolysAround(helper.GetPosition(), new System.Numerics.Vector3(50, 50, 50)).Count > 0)
            {
                GoRandomPoint();
            }else{
                QuixConsole.Log("No");
            }

        }
    }

}