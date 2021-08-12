using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    public class AIManager : ArenaHelper
    {

        public List<Entity> entities = new List<Entity>();
        public AIManager(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public override void OnStart()
        {
            CreateVillan();
            entities.ForEach(entities=>{
                entities.Init();
            });
        }

        public override void OnMapsLoaded()
        {
            base.OnMapsLoaded();
            var enti = arena.navObjects.FindAll(entities=>{return entities is Entity;});
            foreach (var item in enti)
            {
                entities.Add((Entity)item);
            }

        }


        private void CreateVillan()
        {
            /* float size = 60;
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
            */
        }
    }
}