using System;
using System.Numerics;
using SharpNav;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public interface EntityLifeLoop
    {
        /// <summary>
        /// It allows to manage the update action after the object has arrive to the last position
        /// </summary>
        /// <returns>Returns true when Trail should stop</returns>
        bool OnLastPolygon();
        /// <summary>
        /// Update method when Trail is inactive
        /// </summary>
        void OnTrailInactive();
        /// <summary>
        /// Update method when Trail is active
        /// </summary>
        void OnTrailActive();

    }

    public abstract class Entity : PhyObject, EntityLifeLoop
    {
        QuixNavMesh navMesh;
        Arena arena;
        internal Vehicle vehicle;
        private NavMeshQuery navMeshQuery;
        internal Trail trail;
        private PhyObject target;

        public Vector3 extend { get; set; }
        private PhyWaiter stuckWaiter;
        private Vector3 lastPosition;


        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            arena = (Arena)room.gamemode;


        }

        public virtual void Init()
        {

            vehicle = new Vehicle(this, new VehicleProps() { maxSpeed = new Vector3(.1f, .1f, .1f) });
            vehicle.isActive = true;

            navMeshQuery = new NavMeshQuery(arena.tiledNavMesh, 2048);

            trail = new Trail(simulator, this, navMeshQuery);
            trail.OnLastPoint += OnLastPoint;
            stuckWaiter = new PhyWaiter(4000);

            AddWorker(new PhyInterval(1, simulator)).Tick += Update;
        }

        public bool FollowTarget(PhyObject target)
        {
            trail.Start();
            var setTarget = trail.SetTarget(target.GetPosition());
            if (setTarget)
            {
                this.target = target;
                QuixConsole.Log("Look target");

            }
            return setTarget;
        }
        public float Distance(Vector3 target)
        {
            return Vector3.Distance(GetPosition(), target);
        }

        private void Update()
        {
            if (trail.IsActive())
            {
                OnTrailActive();
                if (trail.hasFinished)
                {
                    if (OnLastPolygon())
                    {
                        trail.Stop();
                    }
                }
                vehicle.Arrive(trail.GetPoint());
                CheckPositionForStuck();
            }

            else
            {
                OnTrailInactive();
            }


            if (IsFalling())
            {
                OnFall();
            }
            vehicle.Update();
            AfterUpdate();
        }

        private bool IsFalling()
        {
            return GetPosition().Y < -50;
        }

        private void CheckPositionForStuck()
        {
            var distance = Distance(lastPosition);
            if (distance < 5)
            {
                if (stuckWaiter.Tick())
                {
                    //Too many time in one point
                    OnStuck();
                    stuckWaiter.Reset();

                }
            }
            else
            {
                lastPosition = GetPosition();
            }
        }
        internal void OnLastPoint()
        {
            NavPoint np = new NavPoint(trail.path[trail.path.Count - 1], GetPosition());
            LastPolygon(np);

        }

        #region Abstract & virtual classes
        /// <summary>
        /// This method is called when the objects enters to the last Polygon.
        /// </summary>
        /// <param name="point">The last point in the trail</param>
        public virtual void LastPolygon(NavPoint point)
        {

        }
        /// <summary>
        /// Method to manage fall event
        /// </summary>
        public virtual void OnFall()
        {

        }

        /// <summary>
        /// This method is called when the object is in the same position for several updates while Trail is active
        /// </summary>
        public virtual void OnStuck()
        {

        }

        public abstract bool OnLastPolygon();
        public abstract void OnTrailInactive();
        public abstract void OnTrailActive();
        /// <summary>
        /// This method is called when all the computation in Update is done
        /// </summary>
        public virtual void AfterUpdate()
        {

        }

        #endregion
    }

}