using System;
using System.Numerics;
using SharpNav.Pathfinding;

namespace QuixPhysics
{

    public class Helper : Entity
    {
        private bool shouldLook;
        public HelperItem activeItem;
        private Random random = new Random();

        private EntityLifeLoop currentLoop;
        private User owner;


        public override void Init()
        {

            base.Init();
            SetOwner();
            SetItems();

          //  SetPosition(trail.GetRandomPoint(owner.player.GetPosition()).Position);
          SetPosition(owner.player.GetPosition());

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
                if(item!=null){
                     if(item.ShouldActivate()){
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
                if(r){
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
            }else{
                if(!SelectItem()){
                    ChangeLoop(new HelperAction(this));
                }
            }
        }

        public override void OnTrailActive()
        {
            if (currentLoop != null)
            {
                currentLoop.OnTrailActive();
            }
        }
        public override void OnFall()
        {
            base.OnFall();

        }

        #region Helper Actions
        public void WonderAround()
        {

        }
        #endregion
    }
}