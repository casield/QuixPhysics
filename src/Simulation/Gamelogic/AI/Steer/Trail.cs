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
        public Vector3 targetExtend = new Vector3(500);

        public int MinPathSizeToChange = 1;
        public int PathPointReloadTime = 1000;
    }
    public delegate void TrailAction();
    public class Trail
    {
        private Simulator simulator;
        public Path path;
        private PhyObject obj;

        private Vector3 target;
        private NavPoint startPoint;
        private NavPoint endPoint;
        private bool active = false;

        private Vector3 nextPoint;
        private NavMeshQuery query;
        private int pathPosition = 0;
        public TrailProps props = new TrailProps();
        public bool hasFinished = false;

        public event TrailAction OnLastPoint;
        private PhyWorker worker;


        private PhyWaiter pointWaiter;

        private List<NavPolyId> visited = new List<NavPolyId>();
        private NavQueryFilter navQueryFilter;

        public Trail(Simulator simulator, PhyObject obj, NavMeshQuery query)
        {

            this.simulator = simulator;
            path = new Path();
            this.obj = obj;
            this.query = query;
            this.query = query;
            navQueryFilter = new NavQueryFilter();
            pointWaiter = new PhyWaiter(props.PathPointReloadTime);
        }
        public void Start()
        {
            active = true;
            if (worker != null)
            {
                worker.Destroy();
            }
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

        public bool IsOnPoly(NavPolyId polyId, Vector3 position, Vector3 extends)
        {
            
            List<NavPolyId> polys = new List<NavPolyId>(1);

            var found = query.QueryPolygons(ref position, ref extends, polys);
            return polys.Contains(polyId);
        }

        public bool IsOnLastPosition(Vector3 position, Vector3 extends)
        {
            return IsOnPoly(path[path.Count - 1], position, extends);
        }
        public NavPoint GetRandomPoint(Vector3 position)
        {
            var nearest = query.FindNearestPoly(position, new Vector3(100, 100, 100));
            query.FindRandomConnectedPoint(ref nearest, out NavPoint randomPoint);
            float height = 0;
            query.GetPolyHeight(nearest.Polygon, position, ref height);
            randomPoint.Position.Y += height;
            return randomPoint;
        }

        private void Update()
        {
            if (target != null)
            {
                //Check if phyobject has arrived to the nextPoint
                if (pathPosition < path.Count)
                {


                    if (IsOnPoly(path[pathPosition], obj.GetPosition(), GetExtend()))
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

            pointWaiter.Reset();
            var couldset = SetPoint(pathPosition + 1);
            if (couldset)
            {
                pathPosition++;
            }

        }

        private bool SetPoint(int newpoint)
        {
            if (newpoint < path.Count)
            {
                Vector3 closest = new Vector3();
                query.ClosestPointOnPoly(path[newpoint], obj.GetPosition(), ref closest);

                // query.
                var position = obj.GetPosition();

                Vector3 correctedPoint = new Vector3();
                NavPoint navStart = new NavPoint(path[newpoint], obj.GetPosition());
                var couldmove = query.MoveAlongSurface(ref navStart, ref closest, out correctedPoint, visited);
                if (couldmove)
                {
                    nextPoint = correctedPoint;
                }
                else
                {
                    ResetPath();
                }

               // QuixConsole.Log("Next point", nextPoint, " ( " + pathPosition + " ) Path size:", path.Count, "Visited size:", visited.Count);
                return couldmove;

            }
            else
            {
                LastPoint();
                return true;
            }

        }
        private void ResetPath()
        {
            if (!IsOnPoly(endPoint.Polygon, target, props.targetExtend))
            {
                SetTarget(endPoint.Position);
            }

            QuixConsole.Log("Resetting path");
        }
        private void LastPoint()
        {
            nextPoint = endPoint.Position;
           //QuixConsole.Log("Last Point");
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
                return new Vector3(st.radius * 2);
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

        public static bool CheckPathValidity(Path path, NavPoint lastPoint)
        {

            return path[path.Count - 1] == lastPoint.Polygon;
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



                    startPoint = query.FindNearestPoly(obj.GetPosition(), GetExtend());
                    endPoint = query.FindNearestPoly(target, props.targetExtend);
                    if (path == null)
                    {
                        path = new Path();
                    }

                    bool couldFind = query.FindPath(ref startPoint, ref endPoint, navQueryFilter, path);

 
                    Reset();
                    GoNextPoint();
                    return true;


                }
                else
                {
                    QuixConsole.Log("Polys not found", polyArounObj.Count, polyAroundTarget.Count);
                    return false;
                }
            }
            else
            {
                throw new Exception("Trail is not Active");
            }

        }
        private void Reset()
        {
            visited.Clear();
            pathPosition = 0;
            hasFinished = false;
        }
    }
}