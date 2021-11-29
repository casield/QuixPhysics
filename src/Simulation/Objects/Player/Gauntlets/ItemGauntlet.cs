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
            return player.lookObject.GetPosition();
        }
        public override void Activate(bool active)
        {
            
            if(active){
                
                Gematorium gematorium = new Gematorium(player.user);
                var totalGems = (int)player.user.gems.value - gematorium.GetPrice();
                if(totalGems>=0){
                    QuixConsole.Log("Total gems",totalGems);
                    player.user.gems.Update(totalGems);
                     gematorium.Drop(player.room,shifter.GetPosition()+new Vector3(0,50,0));
                }
               
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