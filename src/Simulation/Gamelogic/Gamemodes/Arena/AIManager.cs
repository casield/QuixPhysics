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
            
        }

        public override void OnMapsLoaded()
        {
            base.OnMapsLoaded();
            entities.ForEach(entities=>{
                entities.Init();
            });

        }

    }
}