using System;
using System.Numerics;
using SharpNav;
using SharpNav.Pathfinding;
using SharpNav.Geometry;
using System.Collections.Generic;

namespace QuixPhysics
{

    public class TrailProps
    {
        public float arriveDistance = 200;
        public Vector3 targetExtend = new Vector3(100);

        public int MinPathSizeToChange = 3;
        public int PathPointReloadTime = 1000;
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
        public event TrailAction OnPathStuck;
        private PhyWorker worker;
        private int tickReload = 0;

        private Vector3 lastPosition;

        private PhyWaiter pointWaiter;
        public Trail(Simulator simulator, PhyObject obj, NavMeshQuery query)
        {
            this.simulator = simulator;
            path = new Path();
            this.obj = obj;
            this.query = query;
            this.query = query;
            pointWaiter = new PhyWaiter(props.PathPointReloadTime);
        }
        public void Start()
        {
            active = true;
            worker = obj.AddWorker(new PhyInterval(1, simulator));
            worker.Tick += Update;
            //GoNextPoint();
        }
        public void Stop()
        {
            active = false;
            worker.Tick -= Update;
        }
        public void Restart()
        {
            active = true;
            Reset();
        }
        public bool IsActive()
        {
            return active;
        }

        public bool IsOnPointPosition(int pathPosition, Vector3 position, Vector3 extends)
        {

            List<NavPolyId> polys = new List<NavPolyId>(1);

            var found = query.QueryPolygons(ref position, ref extends, polys);
            return polys.Contains(path[pathPosition]);
        }

        public bool IsOnLastPosition(Vector3 position, Vector3 extends)
        {
            return IsOnPointPosition(path.Count - 1, position, extends);
        }

        private void Update()
        {
            if (target != null)
            {
                //Check if phyobject has arrived to the nextPoint
                if (pathPosition < path.Count)
                {


                    if (IsOnPointPosition(pathPosition, obj.GetPosition(), GetExtend()))
                    {
                        //Arrived
                        GoNextPoint();
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
            pointWaiter.Reset();
            SetNextPoint();

        }
        private void SetNextPoint()
        {
            if (pathPosition < path.Count)
            {
                Vector3 closest = new Vector3();
                query.ClosestPointOnPoly(path[pathPosition], obj.GetPosition(), ref closest);
                nextPoint = closest;
                QuixConsole.Log("Next point", nextPoint," ( "+pathPosition+" ) Path size:",path.Count);

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
            hasFinished = true;
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
                return new Vector3(st.radius * 4);
            }
            else
            {
                BoxState st = (BoxState)obj.state;
                return st.halfSize * 4;
            }
        }
        public List<NavPolyId> PolysAround(Vector3 center, Vector3 extend)
        {
            List<NavPolyId> polys = new List<NavPolyId>(128);
            query.QueryPolygons(ref center, ref extend, polys);
            return polys;

        }


        public bool SetTarget(Vector3 target)
        {
            if (active)
            {
                var polyArounObj = PolysAround(obj.GetPosition(), GetExtend());
                var polyAroundTarget = PolysAround(target, props.targetExtend);

                if (polyArounObj.Count > 0 && polyAroundTarget.Count > 0)
                {
                    this.target = target;

                    

                    NavPoint startPoint = query.FindNearestPoly(obj.GetPosition(), GetExtend());
                    NavPoint endPoint = query.FindNearestPoly(target, props.targetExtend);

                    Path newPath = new Path();
                    bool couldFind = query.FindPath(ref startPoint, ref endPoint, new NavQueryFilter(), newPath);
                    if (newPath.Count > props.MinPathSizeToChange)
                    {
                        QuixConsole.Log("New Path", target,newPath.Count);
                        path = newPath;
                        Reset();
                        GoNextPoint();
                        return true;

                    }
                    else
                    {
                        //hasFinished=true;
                        return false;
                    }
                }
            }
            else
            {
                throw new Exception("Trail is not Active");
            }


            return false;

        }
        private void Reset()
        {
            pathPosition = 0;
            hasFinished = false;
        }
    }
}