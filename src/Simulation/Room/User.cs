using System;
using System.Numerics;
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
        public Room room;

        public User(string id, Room room)
        {
            sessionId = id;
            oVarManager = room.simulator.oVarManager;
            simulator = room.simulator;

            gems = new OVar("gems" + id, 1, oVarManager);
            this.room = room;

        }

        public void CreatePlayer(Vector3 position)
        {
            var box = new SphereState();
            box.radius = 10;
            box.uID = PhyObject.createUID();
            box.instantiate = true;
            box.type = "Player2";
            box.mesh = "Players/Sol/sol_prefab";
            box.quaternion = Quaternion.Identity;
            box.mass = 30;
            box.owner = sessionId;
            box.position = position;


            var player = (Player2)room.Create(box);
            this.player = player;
            player.user = this;
            StartGemCount();

        }
        private void StartGemCount()
        {
            PhyInterval interval = new PhyInterval(10000, simulator);
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