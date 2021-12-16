using System;
using System.Collections.Generic;
using System.Numerics;
using BepuUtilities;
using Newtonsoft.Json;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public class HelperMessage
    {
        public List<Vector3> positions;
    }
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
        private HelperAction helperAction;
        private bool canJump = true;
        private Dummy dummy;

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(this.guid, this);
           // dummy = (Dummy)room.factory.Create(Dummy.Build(), room,null,false);
           // dummy.AddToObject(this);
        }

        public override void Init()
        {

            base.Init();
            SetOwner();
            SetItems();
            var randompoint = room.GetGameMode<Arena>().hextilesAddon.GetRandomHextile().GetPosition();
            QuixConsole.Log("Random point", randompoint);
            var planeVelocity = .04f;
            vehicle.props.maxSpeed = new Vector3(planeVelocity, .01f, planeVelocity);
            helperAction = new HelperAction(this);


            SetPosition(randompoint);
            stats.vision = 30000;

            trail.OnPointChangeListener += OnPointChange;


           //  SetMoveActive(false);


        }

        public void SetMoveActive(bool active)
        {
            this.trail.SetActive(active);
            this.vehicle.isActive = active;
        }

        public void Jump()
        {
            if (!canJump) return;

            QuixConsole.Log("Jump!");
            GetBodyReference().ApplyLinearImpulse(new Vector3(0, state.mass * 100, 0));
            canJump = false;
        }

        private void OnPointChange(Vector3 point)
        {
            Stop();
            List<Vector3> list = new List<Vector3>();
            list.Add(point);
            list.Add(trail.target);
            var send = JsonConvert.SerializeObject(new HelperMessage() { positions = list });
            SendObjectMessage(send);
        }

        public override void SetProps()
        {
            base.SetProps();
            knowledge = new HelperKnowledge(this);


        }
        internal override void OnRayCastHit(PhyObject obj, Vector3 normal)
        {
            knowledge.CheckObject(obj);
            if (obj is Hexagon)
            {
                Hexagon hexagon = (Hexagon)obj;
                if (Distance(hexagon.GetPosition()) < 50 && hexagon.hextile.GetPosition().Y > GetPosition().Y + 50)
                {
                    Jump();
                }
            }
        }
        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            if (!canJump)
            {
                canJump = true;
            }
        }

        public override void ChangeStateBeforeSend()
        {
            base.ChangeStateBeforeSend();
            var velocityDirection = GetForward();
            var angle = (float)Math.Atan2(velocityDirection.Z, velocityDirection.X);
            state.quaternion = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -angle); ;
        }

        private void SetOwner()
        {
            if (state.owner != null)
            {
                owner = room.users[state.owner];
                knowledge.CheckObject(owner.player);
            }
            else
            {
                throw new Exception("This helper does not have an owner");
            }

        }

        private void SetItems()
        {
            AddItemToStats(new HI_GemFraction(this));
            AddItemToStats(new HI_GolfHat(this));
        }
        private void AddItemToStats(HelperItem item)
        {
            item.Constructor(room.connectionState, room.simulator, room);
            stats.SetItem(item, 0);
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
        /// <summary>
        /// Changes actual Loop.
        /// Set to null to restart flow.
        /// </summary>
        /// <param name="loop"></param>
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
            //
            // var randompoint = ((Arena)room.gamemode).GetRandomPoint(owner.player.GetPosition(), new Vector3(500, 500, 500)).Position;
            if (room.GetGameMode<Arena>().hextilesAddon != null)
            {
                var randompoint = room.GetGameMode<Arena>().hextilesAddon.GetRandomHextile().GetPosition();


                SetPosition(randompoint);

            }
            if (currentLoop == null) return;
            currentLoop.OnFall();

        }
        public override void OnStuck()
        {
            base.OnStuck();
            if (currentLoop != null)
            {
                bodyReference.ApplyLinearImpulse(-GetVelocity() * 3);
                currentLoop.OnStuck();
            }
        }
    }
}