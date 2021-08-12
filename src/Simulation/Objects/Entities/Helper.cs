using System;
using System.Numerics;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public class HelperStats
    {
       public float force;

       public HelperItem[] items = new HelperItem[3];
       public void SetItem(HelperItem item,int index){
           items[index] = item;
       }
    }
    public class Helper : Entity
    {
        private bool shouldLook;
        public HelperStats helperStats;
        public HelperItem activeItem;
        private Random random = new Random();

        public override void Init()
        {
            base.Init();
            SetStats();
            SelectItem();
            
        }

        private void SelectItem(){
            //TODO Better seleccion of items;
           /* var r = random.Next(0,2);
            var item =  helperStats.items[r];
            if(item != null){
                helperStats.items[r].OnActivate();
            }*/

             helperStats.items[0].Activate();
            
        }

        private void SetStats()
        {
            helperStats = new HelperStats();
            helperStats.SetItem(new HI_GemFraction(this),0);
        }

        public override bool OnLastPolygon()
        {

            if(activeItem != null){
              return activeItem.OnLastPolygon();
           }
            return true;
        }

        public override void OnTrailInactive()
        {
           if(activeItem != null){
               activeItem.OnTrailInactive();
           }
        }

        public override void OnTrailActive()
        {
            if(activeItem != null){
               activeItem.OnTrailActive();
           }
        }
    }
}