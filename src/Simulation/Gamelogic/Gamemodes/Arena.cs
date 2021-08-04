using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Aspose.ThreeD;

namespace QuixPhysics
{
    public class Arena : Gamemode
    {
        public List<User> users = new List<User>();
        private int MIN_USERS = 1;
        private Room room;
        private Simulator simulator;
        private QuixNavMesh navMesh;


        private static int TURNS_TO_WIN = 3;
        private List<User> turnWinners = new List<User>();
        

        public Arena(Simulator simulator, Room room) : base(simulator)
        {
            this.room = room;
            this.simulator = simulator;
            navMesh = new QuixNavMesh(simulator);
        }
        public override void OnJoin(User user)
        {
            users.Add(user);
            if (users.Count == MIN_USERS)
            {
                Start();
            }
        }

        public override Vector3 GetStartPoint(User user)
        {
            if (room.map != null)
            {

                var index = users.IndexOf(user);

                if (index > -1 && room.map.startPositions.Count - 1 > index)
                {
                    var point = new Vector3((float)room.map.startPositions[index].AsBsonDocument["x"].AsDouble,
                        (float)room.map.startPositions[index].AsBsonDocument["y"].AsDouble,
                             (float)room.map.startPositions[index].AsBsonDocument["z"].AsDouble);
                    return point;
                }
                else
                {
                    var point = new Vector3((float)room.map.startPositions[0].AsBsonDocument["x"].AsDouble,
                       (float)room.map.startPositions[0].AsBsonDocument["y"].AsDouble,
                            (float)room.map.startPositions[0].AsBsonDocument["z"].AsDouble);
                    return point;
                }

            }
            else
            {
                QuixConsole.Log("GetStartPoint");
                return base.GetStartPoint(user);
            }

        }
        private void SetPlayersToStart()
        {
            foreach (var item in users)
            {
                item.player.SetPositionToStartPoint();
            }
        }
        public override void Start()
        {
            GenerateMapCommand command = new GenerateMapCommand(simulator);

            var objs = command.GenerateMap("isla", room);
            QuixConsole.Log("Start");
            simulator.createObjects(room);
            SetPlayersToStart();

            /*PhyTimeOut p = new PhyTimeOut(500, simulator, true);
            p.Completed += CreateVillan;*/

            //CreateVillan();

            BoxState st = new BoxState() { mass = 0, type = "Villan", instantiate = false, halfSize = new Vector3(1, 100, 1), position = new Vector3(0, 0, 0), mesh = "15Cube", isMesh = true };
            var s = simulator.Create(st, room);

            QuixConsole.Log("Villian created");

            objs.Add(s);


           // navMesh.CreateMesh(objs, "test");

            //CreateMap();

        }

        private void CreateVillan()
        {
            BoxState st = new BoxState() { mass = 0, type = "Villan", instantiate = false, halfSize = new Vector3(1, 1, 1), position = new Vector3(0, 0, 0), mesh = "15Cube", isMesh = true };
            var s = simulator.Create(st, room);

            QuixConsole.Log("Villian created");

        }
        private void CreateMap()
        {

        }

        internal void OnHoleWin(User winner){

            int gemsWon = 0;
            foreach (var item in users)
            {
                //TODO ADD GEMS FROM ALL SOURCES
                if(item!=winner){
                    gemsWon+=(int)item.gems.value;
                }
               
            }
            gemsWon/=users.Count;
            turnWinners.Add(winner);
            winner.gems.Update((int)winner.gems.value+gemsWon);
            if(turnWinners.Count==TURNS_TO_WIN){
                Finish();
            }
            
        }

        public override void Finish()
        {
            User winner = null;
            foreach (var user in users)
            {
                if(winner == null){
                    winner = user;
                }else{
                    if((int)winner.gems.value > (int)user.gems.value){
                        winner = user;
                    }
                }
            }

            QuixConsole.Log("Winner!",winner.sessionId);
        }



    }
}