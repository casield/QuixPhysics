using System;
using System.Numerics;

namespace QuixPhysics
{
    public interface IGauntlet
    {

        void AddPlayer(Player2 player);
        /// <summary>
        /// Activate is called when the player press or release the gauntlet button.
        /// </summary>
        /// <param name="active"></param>
        void Activate(bool active);
        /// <summary>
        /// Swipe is called when the player makes a swipe action.
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="direction"></param>
        void Swipe(double degree, Vector3 direction);
        /// <summary>
        /// This method is called after the creation of the Gauntlet.
        /// </summary>
        void Init();

        /// <summary>
        /// This method is called when the player changes this gauntlet. <br/>All workers should be removed.
        /// </summary>
        void OnChange();
        /// <summary>
        /// This method is called to the new gauntlet. Its called after OnChange().
        /// </summary>
        void OnActivate();
        string name { get; set; }
    }
    public abstract class Gauntlet : IGauntlet
    {
        internal Player2 player;
        internal bool isActive = false;
        internal PhyInterval updateWorker;

        public string name { get; set; }

        public virtual void Activate(bool active)
        {
            isActive = active;
        }

        public void AddPlayer(Player2 player)
        {
            this.player = player;
        }
        /// <summary>
        /// This method is used as shorcut to add a new worker.
        /// </summary>
        public void AddUpdateWorker()
        {
            if (updateWorker == null)
            {
                updateWorker = new PhyInterval(1, player.simulator);
                updateWorker.Completed += Update;
            }
        }
        public void RemoveUpdateWorker(){
            if (updateWorker != null)
            {
                
                updateWorker.Completed -= Update;
                updateWorker = null;
            }
        }
        public virtual void Update(){

        }

        public abstract void Init();

        public abstract void OnActivate();

        public abstract void OnChange();

        public abstract void Swipe(double degree, Vector3 direction);
    }
}