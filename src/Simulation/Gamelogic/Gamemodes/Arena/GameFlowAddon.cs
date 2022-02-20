using System;
using OVars;

namespace QuixPhysics
{
    public class GameFlowAddon : ArenaAddon
    {
        public GameFlowAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public override void OnStart()
        {
           // Load players to check if who wins
           foreach (var item in arena.room.users)
           {
               item.Value.gems.UpdateEventListener+=OnUpdateGems;
           }
        }

        private void OnUpdateGems(object newValue,OVar ovar)
        {
           QuixConsole.Log("UpdateGems",newValue,ovar.name);
        }
    }
}