using System;
using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    public class AIManager : ArenaHelper
    {
        public AIManager(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public override void OnStart()
        {
           CreateVillan();
        }

        public override void OnMapsLoaded()
        {
            base.OnMapsLoaded();
             
        }


        private void CreateVillan()
        {
            float size = 60;
            var v = new SphereState()
            {
                position = new Vector3(-1900,floor.GetTop(size*2),-1000),
                quaternion = Quaternion.Identity,
                //halfSize = new Vector3(size,size, size),
               radius=size,
                mass = 1,
                instantiate = true,
                type = "Villan"
            };
            Villan villan = (Villan)simulator.Create(v, room);
            User user = arena.users.Find(e => { return true; });
            villan.LookTarget(user.player);

        }
    }
}