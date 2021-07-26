using System;
using OVars;

namespace QuixPhysics
{
    public class User
    {
        public string sessionId;
        public Player2 player;

        private OVarManager oVarManager;
        private Simulator simulator;

        public OVar gems;

        public User(string id, Player2 player)
        {
            sessionId = id;
            this.player = player;
            oVarManager = player.simulator.oVarManager;
            simulator = player.simulator;

            gems = new OVar("gems" + id, 1, oVarManager);

            PhyInterval interval = new PhyInterval(1000, simulator);
            interval.Completed += TickGem;


        }

        private void TickGem()
        {
            if (gems != null)
            {
                //QuixConsole.Log("Gems",gems.value);
                int newVal = (int)gems.value + 1;
                gems.Update(newVal);
            }

        }
    }
}