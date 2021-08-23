using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    public class AIManager : ArenaAddon
    {

        public List<Entity> entities = new List<Entity>();
        private Bot bot;

        public AIManager(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            
        }

        public override void OnStart()
        {
            this.bot.OnStart();
        }

        public override void OnMapsLoaded()
        {
            CreateBot();
            base.OnMapsLoaded();
            

        }
        

        private void CreateBot()
        {
           this.bot = new Bot(room);
        }
    }
}