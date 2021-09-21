using System;
using System.Collections.Generic;
using System.Numerics;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public class HelperKnowledge : EntityKnowledge
    {
        public List<Gem> gems = new List<Gem>();
        public HelperKnowledge(Entity entity) : base(entity)
        {
            interestingTypes.Add(typeof(Gem));
        }

        public override void OnKnowledgeAdd(KnowledgeInfo info)
        {
            var obj = info.found_object;
            if (obj is Gem)
            {
                gems.Add((Gem)obj);
                obj.OnDelete += DeleteGem;
                QuixConsole.Log("Found gem!");
            }
        }

        private void DeleteGem(PhyObject obj)
        {
            gems.Remove((Gem)obj);
        }
    }

    public class Helper : Entity
    {
        private bool shouldLook;
        public HelperItem activeItem;
        private Random random = new Random();

        private EntityLifeLoop currentLoop;
        public User owner;
        private Vector3 lastVelocity = new Vector3();
        private HelperAction helperAction;

        public override void Init()
        {

            base.Init();
            SetOwner();
            SetItems();
            var randompoint = ((Arena)room.gamemode).GetRandomPoint(owner.player.GetPosition(), new Vector3(500, 500, 500)).Position;
            QuixConsole.Log("Random point", randompoint);
            vehicle.props.maxSpeed = new Vector3(.1f, .1f, .1f);
            helperAction=new HelperAction(this);


            SetPosition(randompoint);
            stats.vision = 30000;

        }
        public override void SetProps()
        {
            base.SetProps();
            knowledge = new HelperKnowledge(this);
        }
        internal override void OnRayCastHit(PhyObject obj)
        {
            knowledge.CheckObject(obj);
        }
        public override void ChangeStateBeforeSend()
        {
            base.ChangeStateBeforeSend();
            var velocityDirection = GetForward();
            var x = MathF.Cos(velocityDirection.X);
            var z = MathF.Sin(velocityDirection.Z);
            var angle = MathF.Atan2(z, x);

            var nextPoint = trail.GetPoint();
            var angleToNextPoint = MathF.Atan2(MathF.Sin(nextPoint.Z), MathF.Cos(nextPoint.X));
            state.quaternion = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), angleToNextPoint - angle) * new Quaternion(0, 0.707f, 0, 0.707f);


            lastVelocity = bodyReference.Velocity.Linear;
        }

        private void SetOwner()
        {
            if (state.owner != null)
            {
                owner = room.users[state.owner];
            }
            else
            {
                throw new Exception("This helper does not have an owner");
            }

        }

        private void SetItems()
        {
            var gemfrac = new HI_GemFraction(this);
            gemfrac.Constructor(room.connectionState, room.simulator, room);
            stats.SetItem(gemfrac, 0);
        }
        /// <summary>
        /// This method is called every time in the update, checks if any item should be activated. Then sets the first one to the activeItem.
        /// </summary>
        /// <return>Returns true if any item was activated</return>
        private bool SelectItem()
        {
            //TODO Better seleccion of items;
            for (int i = 0; i < stats.items.Length; i++)
            {
                var item = stats.items[i];
                if (item != null)
                {
                    if (item.ShouldActivate())
                    {
                        activeItem = item;
                        ChangeLoop(activeItem);
                        activeItem.Activate();

                        return true;
                    }
                }


            }
            return false;

        }
        public void ChangeLoop(EntityLifeLoop loop)
        {
            currentLoop = loop;
        }


        public override bool OnLastPolygon()
        {

            if (currentLoop != null)
            {
                var r = currentLoop.OnLastPolygon();
                if (r)
                {
                    ChangeLoop(null);

                }
                return r;
            }
            return true;
        }

        public override void OnTrailInactive()
        {
            if (currentLoop != null)
            {
                currentLoop.OnTrailInactive();
            }
            else
            {
                if (!SelectItem())
                {
                    ChangeLoop(helperAction);
                }
            }
        }

        public override void OnTrailActive()
        {
            if (currentLoop != null)
            {
                currentLoop.OnTrailActive();
            }
            if (currentLoop is HelperAction)
            {
                SelectItem();
            }
        }
        public override void OnFall()
        {
            stats.DamageEntity(stats.life);

        }
        public override void OnStuck()
        {
            base.OnStuck();
            if (currentLoop != null)
            {
                currentLoop.OnStuck();
            }
        }
    }
}