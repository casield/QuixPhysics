using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics.Constraints;
using SharpNav;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public struct KnowledgeInfo
    {
        public Vector3 found_position;
        public DateTime found_time;
        public PhyObject found_object;
        public static bool operator ==(KnowledgeInfo c1, KnowledgeInfo c2)
        {
            return c1.found_object == c2.found_object;
        }
        public static bool operator !=(KnowledgeInfo c1, KnowledgeInfo c2)
        {
            return c1.found_object != c2.found_object;
        }

        public override bool Equals(object obj)
        {
            return (KnowledgeInfo)obj == this;
        }
    }
    /// <summary>
    /// This class represents the information that this Entity can Know. It's fullfilled while this entity walks around using the vehicle and the raycast.
    /// </summary>
    public class EntityKnowledge
    {
        public List<Type> interestingTypes = new List<Type>();
        public List<KnowledgeInfo> knowledgeList = new List<KnowledgeInfo>();
        private Entity entity;

        public EntityKnowledge(Entity entity)
        {
            this.entity = entity;
            interestingTypes.Add(typeof(Player2));
            interestingTypes.Add(typeof(Entity));
        }
        /// <summary>
        /// Checks if this entity knows a certain object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public KnowledgeInfo KnownsThisObject(PhyObject obj)
        {
            return knowledgeList.Find(en => en.found_object == obj);
        }
        /// <summary>
        /// Check if the object is on the interesting types. If true it calls OnFoundInterestingObj.
        /// </summary>
        /// <param name="obj"></param>
        public void CheckObject(PhyObject obj)
        {

            for (int i = 0; i < interestingTypes.Count; i++)
            {
                Type ty = interestingTypes[i];
                if (obj.GetType().Name == ty.Name)
                {
                    OnFoundInterestingObject(obj, ty);
                }
            }
        }
        /// <summary>
        /// Deletes the found object.
        /// </summary>
        /// <param name="obj"></param>
        public void DeleteFoundObject(PhyObject obj)
        {
            var kw = knowledgeList.Find(e => e.found_object == obj);
            knowledgeList.Remove(kw);
        }
        /// <summary>
        /// Adds the kwnoledge info to the list and creates a listener to object deletion.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        public void OnFoundInterestingObject(PhyObject obj, Type type)
        {
            obj.OnDelete += DeleteFoundObject;
            KnowledgeInfo info = new KnowledgeInfo() { found_object = obj, found_position = obj.GetPosition(), found_time = DateTime.Now };
            if (knowledgeList.Contains(info))
            {
                var index = knowledgeList.IndexOf(info);
                knowledgeList[index] = info;
                OnKnowledgeUpdate(info);
            }
            else
            {
                knowledgeList.Add(info);
                OnKnowledgeAdd(info);
            }

        }
        /// <summary>
        /// This method is called when an existing kwnoledge is found.
        /// </summary>
        /// <param name="info">The updated knowledge</param>
        public virtual void OnKnowledgeUpdate(KnowledgeInfo info)
        {

        }
        /// <summary>
        /// This method is called when a new knowledge is achived.
        /// </summary>
        /// <param name="info"></param>
        public virtual void OnKnowledgeAdd(KnowledgeInfo info)
        {

        }
    }
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

        void OnFall();

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
        public float vision = 1000;
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
        public EntityKnowledge knowledge;
        private PhyWaiter stuckWaiter;
        private Raycast raycast;

        public Entity()
        {
            SetProps();
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            arena = (Arena)room.gamemode;
            stuckWaiter = new PhyWaiter(1000);
            CreateRayCast();
            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(1000f, 100f);
        }
        /// <summary>
        /// Creates the stats and the entity kwnloedg. Should be overrided to change any of this.
        /// </summary>
        public virtual void SetProps()
        {
            stats = new EntityStats(this);
            knowledge = new EntityKnowledge(this);
        }
        public virtual void CreateRayCast()
        {
            raycast = new Raycast(simulator, room);
            raycast.distance = 1000;
            raycast.debugRayShape = true;
            raycast.SetRayShape(new SpiralRayShape(25, 20, raycast));
            raycast.ObjectHitListeners += OnRayCastHit;
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
        /// <summary>
        /// Follows a target. Starts the trail.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool FollowTarget(PhyObject target)
        {
            if (NavQueryExists() && target != null)
            {
                trail.Start();
                var setTarget = trail.SetTarget(target.GetPosition());
                if (setTarget)
                {
                    this.target = target;
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
        /// Main loop of the entity, it searchs if the navquery exists and then makes all the operations for the Trail.
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
                UpdateRaycast();
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
        /// <summary>
        /// Updates the raycast
        /// </summary>
        internal virtual void UpdateRaycast()
        {
            if (bodyReference.Exists && raycast != null)
            {
                var pos = GetPosition();
                raycast.Update(GetPosition() + new Vector3(0, 25, 0), GetForward(), state.quaternion);
            }

        }
        private void CheckPositionForStuck()
        {
            var distance = Distance(lastPosition);
            if (distance < 1)
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

        internal Vector3 GetForward()
        {
            var v = Vector3.Normalize(GetVelocity());
            v.Y = 0;
            return v;
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
                var gem = new Gem();
                gem.Drop(room, GetPosition());
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
        /// When any of the raycast get hitted. The child of this class should fullfil the knowledge overriding this method.
        /// </summary>
        /// <param name="obj"></param>
        internal abstract void OnRayCastHit(PhyObject obj, Vector3 normal);

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