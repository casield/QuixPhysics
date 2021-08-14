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

        public override void Init()
        {
           
            base.Init();
            SetItems();
            SelectItem();

        }

        private void SetItems()
        {
            var gemfrac = new HI_GemFraction(this);
            gemfrac.Constructor(room.connectionState,room.simulator,room);
            stats.SetItem(gemfrac, 0);
        }

        private void SelectItem()
        {
            //TODO Better seleccion of items;
            activeItem = stats.items[0];
            ChangeLoop(activeItem);
            
            activeItem.Activate();

        }
        public void ChangeLoop(EntityLifeLoop loop){
            currentLoop = loop;
        }


        public override bool OnLastPolygon()
        {

            if (currentLoop != null)
            {
                return currentLoop.OnLastPolygon();
            }
            return true;
        }

        public override void OnTrailInactive()
        {
            if (currentLoop != null)
            {
                currentLoop.OnTrailInactive();
            }
        }

        public override void OnTrailActive()
        {
            if (currentLoop != null)
            {
                currentLoop.OnTrailActive();
            }
        }

        #region Helper Actions
        public void WonderAround(){
            
        }
        #endregion
    }
}