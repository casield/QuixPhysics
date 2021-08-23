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
        void OnStuck();

    }
    public class EntityStats
    {
        public float force;

        public HelperItem[] items = new HelperItem[3];
        /// <summary>
        /// The amount of gems that this entity is carrying
        /// </summary>
        public int gems = 0;
        /// <summary>
        /// This variable changes when the owner gives him attention.
        /// Value is between -10 - 10
        /// </summary>
        public float ownerLove = 10 / 8;

        /// <summary>
        /// Experience is gained when the helper succesfully used an item. Experience is divided by 2 when dies.
        /// </summary>
        public float experience = 1;
        public float life = 10;
        public Entity entity;

        public EntityStats(Entity entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// How far the entity can look
        /// </summary>
        public float vision = 8000;
        public void SetItem(HelperItem item, int index)
        {
            items[index] = item;
        }

        /// <summary>
        /// Damage this entity, it returns true if the entity dies.
        /// </summary>
        /// <param name="damage"></param>
        /// <returns>True if entity dies</returns>
        public bool DamageEntity(float damage)
        {
            life -= damage;
            if (life <= 0)
            {
                entity.OnDead();
                return true;
            }
            return false;
        }

    }

    public abstract class Entity : PhyObject, EntityLifeLoop
    {
        QuixNavMesh navMesh;
        Arena arena;
        internal Vehicle vehicle;
        internal Trail trail;
        public PhyObject target;

        public Vector3 extend { get; set; }
        private Vector3 lastPosition;
        public EntityStats stats;
        private PhyWaiter stuckWaiter;

        public Entity()
        {
            stats = new EntityStats(this);
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            arena = (Arena)room.gamemode;
            stuckWaiter = new PhyWaiter(4000);
        }

        public virtual void Init()
        {

            vehicle = new Vehicle(this, new VehicleProps() { maxSpeed = new Vector3(.3f, .3f, .3f) });
            vehicle.isActive = true;
            if (NavQueryExists())
            {
                trail = new Trail(simulator, this, arena.navMeshQuery);
                trail.OnLastPoint += OnLastPoint;
                AddWorker(new PhyInterval(1, simulator)).Tick += Update;
            }




        }

        public bool FollowTarget(PhyObject target)
        {
            if (NavQueryExists() && target!=null)
            {
                trail.Start();
                var setTarget = trail.SetTarget(target.GetPosition());
                if (setTarget)
                {
                    this.target = target;
                    QuixConsole.Log("Look target");

                }
                else
                {

                    trail.Stop();
                }
                return setTarget;
            }
            else
            {
                return false;
            }

        }

        protected bool NavQueryExists()
        {
            return arena.navMeshQuery != null;
        }

        public float Distance(Vector3 target)
        {
            return Vector3.Distance(GetPosition(), target);
        }
        /// <summary>
        /// Main loop of the entity, it looks if the navquery exists and then makes all the operations for the Trail.
        /// </summary>
        internal virtual void Update()
        {
            if (NavQueryExists())
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
                    
                }

                else
                {
                    OnTrailInactive();
                }


                if (IsFalling())
                {
                    OnFall();
                }
                CheckPositionForStuck();
                vehicle.Update();
                AfterUpdate();
            }

        }

        #region Entity Operators
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

        #endregion

        #region Abstract & virtual classes
        /// <summary>
        /// This method is called when the objects enters to the last Polygon.
        /// </summary>
        /// <param name="point">The last point in the trail</param>
        public virtual void LastPolygon(NavPoint point)
        {

        }
        /// <summary>
        /// When the entity dies it drops all the gems that it's carryng.
        /// </summary>
        public virtual void OnDead()
        {
            for (int i = 0; i < stats.gems; i++)
            {
                var gem=new Gem();
                gem.Drop(room,GetPosition());
            }
            Destroy();
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