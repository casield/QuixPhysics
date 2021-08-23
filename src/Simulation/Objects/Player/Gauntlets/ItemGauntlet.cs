using System;
using System.Numerics;

namespace QuixPhysics
{
    public class ItemGauntlet : Gauntlet
    {
        ShapeShifterItem shifter;
        public ItemGauntlet()
        {
            name = "item";
        }

        public override void Init()
        {
            // throw new System.NotImplementedException();
            

        }
        public Vector3 GetShifterPosition()
        {

            /*float distance = 150;
            float sep = -.2f;
            var newPos = player.GetPosition();
            var x = -(float)Math.Cos(player.rotationController+sep);
            var y = -(float)Math.Sin(player.rotationController+sep);

            newPos.X += (x * distance);
            newPos.Z += y * distance;
            newPos.Y +=distance/3;*/

            return player.lookObject.GetPosition();
        }
        public override void Activate(bool active)
        {
            
            if(active){
                
                Gematorium gematorium = new Gematorium(player.user);
                gematorium.Drop(player.room,shifter.GetPosition()+new Vector3(0,50,0));
            }

        }


        public override void Update()
        {
            if (shifter.isInstantiated)
            {
            
                shifter.SetPosition(GetShifterPosition());
            }

        }

        public override void Swipe(double degree, Vector3 direction)
        {
            //throw new System.NotImplementedException();
        }

        public override void OnChange()
        {
            RemoveUpdateWorker();
            shifter.Destroy();
        }

        public override void OnActivate()
        {
            AddUpdateWorker();
            
            shifter = new ShapeShifterItem();
            shifter.Drop(player.room, player.GetPosition());
        }
    }
}