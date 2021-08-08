using System.Diagnostics;

namespace QuixPhysics
{
    public interface IArenaHelper
    {
        void OnStart();
        void OnMapsLoaded();
    }
    public class ArenaHelper : IArenaHelper
    {
        public Floor floor;
        internal Simulator simulator;
        internal Room room;
        internal Arena arena;

        public ArenaHelper(Simulator simulator, Arena arena)
        {
            this.simulator = simulator;
            this.room = arena.room;
            this.arena = arena;
        }

        public virtual void OnMapsLoaded()
        {
            floor = (Floor)arena.navObjects.Find(e => { return e is Floor; });
            Debug.Assert(floor == null);
        }

        public virtual void OnStart()
        {
            throw new System.NotImplementedException();
        }
    }
}