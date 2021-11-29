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
            CreateCrocoLoca();
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

        private CrocoLoca CreateCrocoLoca(){
            var randhextile = room.GetGameMode<Arena>().hextilesAddon.GetRandomHextile();
            CrocoLoca croc = (CrocoLoca)room.factory.Create(CrocoLoca.Build(randhextile.GetPosition()),room);

            return croc;
        }        

        private void CreateBot()
        {
           this.bot = new Bot(room);
        }
    }
}