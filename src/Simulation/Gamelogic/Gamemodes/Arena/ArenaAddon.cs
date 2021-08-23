using System.Diagnostics;

namespace QuixPhysics
{
    public interface IArenaAddon
    {
        /// <summary>
        /// This method should be called when all users are ready to play.
        /// </summary>
        void OnStart();
        void OnMapsLoaded();
    }
    public abstract class ArenaAddon : IArenaAddon
    {
        public Floor floor;
        internal Simulator simulator;
        internal Room room;
        internal Arena arena;

        public ArenaAddon(Simulator simulator, Arena arena)
        {
            this.simulator = simulator;
            this.room = arena.room;
            this.arena = arena;
        }

        public virtual void OnMapsLoaded()
        {
           
        }

        public abstract void OnStart();
    }
}