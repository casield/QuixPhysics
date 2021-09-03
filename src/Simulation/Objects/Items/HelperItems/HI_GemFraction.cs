using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics
{
    /// <summary>
    /// This HelperItem makes the Helper find dropped gems
    /// </summary>
    public class HI_GemFraction : HelperItem
    {
       // List<Gem> gemsDropped = new List<Gem>();
        Helper helper;
        Random random = new Random();
        private int maxGemsToCarry = 3;
        public HI_GemFraction(Helper helper) : base(helper)
        {
            this.helper = helper;

        }
        public override void Constructor(ConnectionState connectionState, Simulator simulator, Room room)
        {
            base.Constructor(connectionState, simulator, room);
            room.gamemode.itemDroppedListeners += OnItemDropped;
        }

        public override void Activate()
        {
            //room.gamemode.itemDroppedListeners += OnItemDropped;
            LookGem();
        }
        /// <summary>
        /// Add the dropped gems
        /// </summary>
        /// <param name="item"></param>
        private void OnItemDropped(Item item)
        {
           /* if (item is Gem)
            {
                gemsDropped.Add((Gem)item);
                ((Gem)item).OnDelete+=OnGemDeleted;
            }*/
        }

        public override void Desactivate()
        {
            base.Desactivate();
            helper.bodyReference.Velocity.Linear *= .5f;
            // room.gamemode.itemDroppedListeners -= OnItemDropped;
        }

        public override void Instantiate(Room room, Vector3 position)
        {
            throw new System.NotImplementedException();
        }
        #region Value Of Gems
        /// <summary>
        /// Find the most valuable Gem.
        /// <br/>Looks closest Gem
        /// Looks if is from owner -> if ownerLove is lower than 0 it will try to get owner's gems otherwise it will find other dropped gems for it's owner.
        /// </summary>
        /// <returns>The most valuable Gem for the helper, if no gem was found it returns null</returns>
        private List<Gem> FindMostValuablesGems()
        {
            List<Gem> sortGems = new List<Gem>(((HelperKnowledge)helper.knowledge).gems);
            sortGems.Sort((gem1, gem2) =>
             {
                 if (gem1.bodyReference.Exists && gem2.bodyReference.Exists)
                 {
                     var distance1 = Vector3.Distance(gem1.GetPosition(), helper.GetPosition());
                     var distance2 = Vector3.Distance(gem2.GetPosition(), helper.GetPosition());
                     if (distance1 > helper.stats.vision)
                     {
                         return -2;
                     }

                     if (distance1 < distance2)
                     {
                         return 1;
                     }
                     if (distance1 > distance2)
                     {
                         return -1;
                     }
                 }

                 return 0;
             });


            ///Looks the gems by the amount of ownerLove
            int gemToLook = (int)(sortGems.Count * helper.stats.ownerLove);
            List<Gem> valuatedGems = new List<Gem>(sortGems);
            valuatedGems.Sort((gem1, gem2) =>
            {
                float valGem1 = ValueOfGem(gem1, valuatedGems.Count);
                float valGem2 = ValueOfGem(gem2, valuatedGems.Count);

                if (valGem1 >= valGem2)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }

            });


            return valuatedGems;
        }
        /// <summary>
        /// Gets the visible gems.
        /// </summary>
        /// <returns></returns>
        private List<Gem> GetVisibleGems()
        {
            List<Gem> closeGems = new List<Gem>();
            ((HelperKnowledge)helper.knowledge).gems.ForEach(gem =>
            {
                if (gem.Destroyed || !gem.bodyReference.Exists)
                {
                    return;
                }
                if (helper.Distance(gem.GetPosition()) <= helper.stats.vision)
                {
                    closeGems.Add(gem);
                }
            });
            return closeGems;
        }
        /// <summary>
        /// Gets the value of a gem
        /// </summary>
        /// <value>Value of Gem</value>
        private float ValueOfGem(Gem gem, int GemsSize)
        {

            if (helper.stats.ownerLove <= 0)
            {
                if (gem.state.owner == helper.state.owner)
                {
                    //If owner love is lower than 0 increase the value so it will search it first
                    return GemsSize / 6;
                }
                else
                {
                    return GemsSize / 2;
                }
            }
            else
            {
                if (gem.state.owner == helper.state.owner)
                {
                    //If owner love is bigger than 0 increase the value so it will search it first
                    return GemsSize / 2;
                }
                else
                {
                    return GemsSize / 6;
                }
            }
        }
        #endregion

        private void OnFullGems()
        {
            Gematorium gematorium = (Gematorium)helper.owner.objects.Find(e => e.state.type == "Gematorium");
            helper.FollowTarget(gematorium);
            
        }
        private void PickUpGem(Gem gem)
        {

            QuixConsole.Log("Gem picked up",helper.state.owner,helper.stats.gems);
            helper.stats.gems += 1;
            gem.Destroy();
            if (helper.stats.gems >= maxGemsToCarry)
            {
                OnFullGems();
            }
        }
        private bool IsLookingGem()
        {
            return helper.target is Gem;
        }
        private bool IsLookingGematorium()
        {
            return helper.target is Gematorium;
        }

        public override void OnTrailInactive()
        {

            if (!IsLookingGem())
            {
                if (!LookGem())
                {
                    Desactivate();
                }
            }
        }
        /// <summary>
        /// Look for the mos valuable gem. Return false if the body does not exists or FollowTarget returned null.
        /// </summary>
        /// <returns></returns>
        private bool LookGem()
        {
            var gems = FindMostValuablesGems();
            if (gems.Count > 0)
            {
                int witch = random.Next(0, gems.Count - 1);
                if (gems[witch].bodyReference.Exists)
                {
                    var follow = helper.FollowTarget(gems[witch]);

                    return follow;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        public override bool OnLastPolygon()
        {
            if (IsLookingGem())
            {
                helper.vehicle.Arrive(helper.target.GetPosition());
                if (helper.Distance(helper.target.GetPosition()) < 250)
                {
                    if (!helper.target.Destroyed)
                    {
                        PickUpGem((Gem)helper.target);

                    }
                    // Desactivate();
                    // return true;
                }
                if (helper.target.Destroyed)
                {
                    Desactivate();
                    return true;
                }

            }
            if (IsLookingGematorium())
            {
                helper.vehicle.Arrive(helper.target.GetPosition());
                if (helper.Distance(helper.target.GetPosition()) < 250)
                {
                    Gematorium gematorium = (Gematorium)helper.target;
                    gematorium.AddGems(helper.stats.gems);
                    helper.stats.gems =0;
                    Desactivate();
                    return true;
                }
            }
            return false;
        }

        public override void OnTrailActive()
        {
            //QuixConsole.Log("Trail active");
            helper.vehicle.Arrive(helper.trail.GetPoint());


        }



        public override bool ShouldActivate()
        {
            if(helper.stats.gems>=maxGemsToCarry){
                return true;
            }
            var gems = GetVisibleGems();
            if(gems !=null){
                 //QuixConsole.Log("Visible gems", gems.Count,helper.state.owner);
            return gems.Count > 0;
            }else{
                return false;
            }

           
        }

        public override void OnStuck()
        {
            QuixConsole.Log("Stucked on GemFraction",helper.state.owner);
            LookGem();
        }
    }
}