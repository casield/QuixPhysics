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
            if (LastPointIsClose())
            {
                return true;
            }
            helper.vehicle.Arrive(helper.trail.GetPoint());



            return false;
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
            QuixConsole.Log("Trail inactive");
            GoRandomPoint();
        }

        public void OnStuck()
        {
            //throw new System.NotImplementedException();
            QuixConsole.Log("Stuck in HelperAction");
            GoRandomPoint();
        }

        public void OnFall()
        {
            this.helper.ChangeLoop(null);
            GoRandomPoint();
        }
    }

}