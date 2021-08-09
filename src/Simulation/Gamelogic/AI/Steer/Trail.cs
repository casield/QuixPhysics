using System;
using System.Numerics;
using SharpNav;
using SharpNav.Pathfinding;

namespace QuixPhysics
{

    public class TrailProps
    {
        public float ArriveDistance = 300;
    }
    public delegate void TrailAction();
    public class Trail
    {
        private Simulator simulator;
        private Path path;
        private PhyObject obj;

        private Vector3 target;
        private bool active = false;

        private Vector3 nextPoint;
        private NavMeshQuery query;
        private int pathPosition = 0;
        public TrailProps props = new TrailProps();
        public bool hasFinished = false;

        public event TrailAction OnLastPoint;
        public Trail(Simulator simulator, PhyObject obj, NavMeshQuery query)
        {
            this.simulator = simulator;
            path = new Path();
            this.obj = obj;
            this.query = query;
            this.query = query;
        }
        public void Start()
        {
            active = true;
            obj.AddWorker(new PhyInterval(1, simulator)).Tick += Update;
            GoNextPoint();
        }

        private void Update()
        {
            if (target != null)
            {
                //Check if phyobject has arrived to the nextPoint
                //QuixConsole.Log("",(Vector3.Distance(obj.GetPosition(), nextPoint)));
                if (pathPosition < path.Count)
                {
                    var distance = Vector3.Distance(obj.GetPosition(), nextPoint);
                    if (distance <= props.ArriveDistance)
                    {
                        //Arrived
                        //Set NextPoint

                        GoNextPoint();

                    }else{
                         //QuixConsole.Log("Distance",distance);
                    }
                   
                
                }
                


                //Check if phyobject is near the point, if not find path to the point
            }
            else
            {
                QuixConsole.Log("Target is null");
            }
        }

        private void GoNextPoint()
        {
            pathPosition++;
            SetNextPoint();

        }
        private void SetNextPoint()
        {
            QuixConsole.Log("ASKASD", pathPosition, path.Count);
            if (pathPosition < path.Count)
            {
                Vector3 closest = new Vector3();
                query.ClosestPointOnPoly(path[pathPosition], obj.GetPosition(), ref closest);
                nextPoint = closest;
                QuixConsole.Log("Next point", nextPoint);

            }
            else
            {
                LastPoint();
            }

        }
        private void LastPoint()
        {
            QuixConsole.Log("Last Point");
            OnLastPoint?.Invoke();
            //hasFinished = true;
        }
        public Vector3 GetPoint()
        {
            return nextPoint;
        }
        public Vector3 GetExtend()
        {
            if (obj.state is SphereState)
            {
                SphereState st = (SphereState)obj.state;
                return new Vector3(st.radius * 2);
            }
            else
            {
                BoxState st = (BoxState)obj.state;
                return st.halfSize * 2;
            }
        }
        public bool SetTarget(Vector3 target)
        {

            
            this.target = target;

            QuixConsole.Log("Target", target);

            NavPoint startPoint = query.FindNearestPoly(obj.GetPosition(), GetExtend());
            NavPoint endPoint = query.FindNearestPoly(target, new Vector3(10, 10, 10));

            Path newPath = new Path();
            bool couldFind = query.FindPath(ref startPoint, ref endPoint, new NavQueryFilter(), newPath);
            path = newPath;
            QuixConsole.Log("Path size", path.Count,"Found: ",couldFind);
            Reset();
            return couldFind;
        }
        public void Reset()
        {
            pathPosition = 0;
            //
            hasFinished = false;
            GoNextPoint();
            //SetNextPoint();
        }
    }
}