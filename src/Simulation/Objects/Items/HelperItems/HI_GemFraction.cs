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
        List<Gem> gemsDropped = new List<Gem>();
        Helper helper;
        Random random = new Random();
        public HI_GemFraction(Helper helper) : base(helper)
        {
            this.helper = helper;
        }

        public override void Activate()
        {

            room.gamemode.itemDroppedListeners += OnItemDropped;
        }
        /// <summary>
        /// Add the dropped gems
        /// </summary>
        /// <param name="item"></param>
        private void OnItemDropped(Item item)
        {
            if (item is Gem)
            {
                gemsDropped.Add((Gem)item);
            }
        }

        public override void Desactivate()
        {
            room.gamemode.itemDroppedListeners -= OnItemDropped;
        }

        public override void Instantiate(Room room,Vector3 position)
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
            List<Gem> sortGems = new List<Gem>(gemsDropped);
            sortGems.Sort((gem1, gem2) =>
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
                 return 0;
             });


            ///Looks the gems by the amount of ownerLove
            int gemToLook = (int)(sortGems.Count * helper.stats.ownerLove);
            List<Gem> valuatedGems = new List<Gem>(sortGems);
            valuatedGems.Sort((gem1, gem2) =>
            {
                float valGem1 = ValueOfGem(gem1, valuatedGems.Count);
                float valGem2 = ValueOfGem(gem2, valuatedGems.Count);

                if (valGem1 > valGem2)
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

        private bool IsLookingGem()
        {
            return helper.target is Gem;
        }

        public override void OnTrailInactive()
        {

            if (!IsLookingGem())
            {
                var gems = FindMostValuablesGems();
                if (gems.Count > 0)
                {
                    int witch = random.Next(0,gems.Count-1);
                    helper.FollowTarget(gems[witch]);
                }
            }
        }

        public override bool OnLastPolygon()
        {
            helper.vehicle.Arrive(helper.target.GetPosition()+helper.extend);
            if (helper.Distance(helper.target.GetPosition()) < 20)
            {
                return true;
            }
            return false;
        }

        public override void OnTrailActive()
        {
            helper.vehicle.Arrive(helper.trail.GetPoint());
        }

        public override void Update()
        {
            //Look for gems around if cant see any wonder around looking for more
            //After 3 tries end use
        }
    }
}