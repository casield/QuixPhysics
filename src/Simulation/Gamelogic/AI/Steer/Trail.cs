using System;
using System.Numerics;
using SharpNav;
using SharpNav.Pathfinding;

namespace QuixPhysics
{

    public class TrailProps
    {
        public float ArriveDistance = 200;
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
            obj.AddWorker(new PhyInterval(1, simulator)).Completed += Update;
            GoNextPoint();
        }

        private void Update()
        {
            if (target != null)
            {
                //Check if phyobject has arrived to the nextPoint

                if (Vector3.Distance(obj.GetPosition(), nextPoint) <= props.ArriveDistance)
                {
                    //Arrived
                    //Set NextPoint
                    GoNextPoint();

                }

                //Check if phyobject is near the point, if not find path to the point
            }
        }

        private void GoNextPoint()
        {
            pathPosition++;
           SetNextPoint();

        }
        private void SetNextPoint()
        {
            if (pathPosition < path.Count)
            {
                Vector3 closest = new Vector3();
                query.ClosestPointOnPoly(path[pathPosition], obj.GetPosition(), ref closest);
                nextPoint = closest;
            }
            else
            {
                OnLastPoint?.Invoke();
                hasFinished = true;
            }
        }
        public Vector3 GetNextPoint()
        {
            return nextPoint;
        }
        public bool SetTarget(Vector3 target)
        {

            this.target = target;
            NavPoint startPoint = query.FindNearestPoly(obj.GetPosition(), new Vector3(100, 100, 100));
            NavPoint endPoint = query.FindNearestPoly(target, new Vector3(100, 100, 100));

            Path newPath = new Path();
            bool couldFind = query.FindPath(ref startPoint, ref endPoint, new NavQueryFilter(), newPath);
            path = newPath;
            QuixConsole.Log("Path size",path.Count);
            return couldFind;
        }
        public void Reset()
        {
            pathPosition = 0;
            SetNextPoint();
        }
    }
}