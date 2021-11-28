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
            // GoRandomPoint(); 
            /*QuixConsole.Log("On last polygn");
            if (LastPointIsClose())
            {
                return true;
            }
            helper.vehicle.Arrive(helper.trail.GetPoint());*/



            return true;
        }

        private bool LastPointIsClose()
        {
            return (helper.trail.IsOnLastPosition(helper.GetPosition(), helper.extend));
        }

        private void GoRandomPoint()
        {
            var extend = new Vector3(100);
            if (helper.trail.PolysAround(helper.GetPosition(), extend).Count > 0)
            {
                helper.trail.Start();
                var randNav = helper.trail.GetRandomPoint(helper.GetPosition(),extend);
               var canSet = helper.trail.SetTarget(randNav.Position);
               if(!canSet){
                   helper.trail.Stop();
               }
                QuixConsole.Log("Go to random point", randNav.Position,canSet);
            }

        }

        public void OnTrailActive()
        {
            helper.vehicle.SeekFlee(helper.trail.GetPoint(), true);
        }

        public void OnTrailInactive()
        {
            /*if (helper.trail.PolysAround(helper.GetPosition(), new System.Numerics.Vector3(500)).Count > 0)
            {*/

            QuixConsole.Log("Trail inactive");
            GoRandomPoint();


            // }

        }

        public void OnStuck()
        {
            //throw new System.NotImplementedException();
            QuixConsole.Log("Stuck in HelperAction");
            helper.Jump();
            // GoRandomPoint();
        }

        public void OnFall()
        {
            GoRandomPoint();
        }
    }

}