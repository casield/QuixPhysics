using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Aspose.ThreeD;
using SharpNav;

namespace QuixPhysics
{
    public struct ArenaProps
    {
        public float MIN_USERS;
        public float TURNS_TO_WIN;
    }
    public class Arena : Gamemode
    {
        public ArenaProps props = new ArenaProps() { MIN_USERS = 1, TURNS_TO_WIN = 3 };
        public List<User> users = new List<User>();


        private UserLoader userLoader;

        private List<User> turnWinners = new List<User>();

        public List<PhyObject> navObjects = new List<PhyObject>();
        private NavMesh navMesh;
        public TiledNavMesh tiledNavMesh;
        private string navMeshName;
        private AIManager aIManager;
        public event PhyAction OnNavMeshReadyListeners;

        public Arena(Simulator simulator, Room room) : base(simulator, room)
        {
            this.room = room;
            this.simulator = simulator;
        }
        public override void Init()
        {
            userLoader = new UserLoader(simulator, this);
            aIManager = new AIManager(simulator, this);
            GenerateMap();
        }
        public override void OnJoin(User user)
        {
            users.Add(user);


            userLoader.LoadItems(user);
            if (users.Count == props.MIN_USERS)
            {
                Start();
            }

        }

        public override Vector3 GetStartPoint(User user)
        {
            var point = new Vector3();
            if (room.map != null)
            {

                var index = users.IndexOf(user);

                if (index <= -1 || room.map.startPositions.Count - 1 > index)
                {
                    index = 0;

                }

                var x = float.Parse(room.map.startPositions[index].AsBsonDocument["x"].ToString());
                var y = float.Parse(room.map.startPositions[index].AsBsonDocument["y"].ToString());
                var z = float.Parse(room.map.startPositions[index].AsBsonDocument["z"].ToString());
                point = new Vector3(x, y, z);

            }
            return point;

        }
        private void CreatePlayers()
        {
            foreach (var item in users)
            {

                var startpoint = room.gamemode.GetStartPoint(item);
                item.CreatePlayer(startpoint);
            }
        }

        private void GenerateMap()
        {
            GenerateMapCommand command = new GenerateMapCommand(simulator);

            var objs = command.GenerateMap("isla", room);
            navObjects.AddRange(objs);

            userLoader.OnMapsLoaded();
            aIManager.OnMapsLoaded();
            simulator.createObjects(room);
        }

        public override void Start()
        {

            CreatePlayers();


            GenerateNavMesh();
            aIManager.OnStart();


            QuixConsole.Log("NavMesh", navMeshName);
        }

        private void GenerateNavMesh()
        {
            QuixNavMesh qinavMesh = new QuixNavMesh(simulator);
            navMeshName = "Arena";
            var settings = NavMeshGenerationSettings.Default;

            //Resizes the mesh and the navmesh
            float resizer = 1;
            float mulre = (100 / resizer);

            settings.CellSize = 0.3f * mulre;
            settings.CellHeight = 0.2f * mulre;
            settings.MaxClimb = 0.9f * mulre;
            settings.AgentHeight = 2.0f * mulre;
            settings.AgentRadius = 0.6f * mulre;

            //Creates the mesh .obj
            qinavMesh.CreateMesh(navObjects, navMeshName, resizer);
            //Then it generates de NavMesh
            navMesh = qinavMesh.GenerateNavMesh(navMeshName, settings);


            //Save it to a file .snb
            qinavMesh.SaveNavMeshToFile(navMeshName);
            //Read it from .snb
            tiledNavMesh = qinavMesh.GetTiledNavMesh(navMeshName);

            OnNavMeshReadyListeners?.Invoke();

        }
        internal void OnHoleWin(User winner)
        {

            int gemsWon = 0;
            foreach (var item in users)
            {
                //TODO ADD GEMS FROM ALL SOURCES
                if (item != winner)
                {
                    gemsWon += (int)item.gems.value;
                }

            }
            gemsWon /= users.Count;
            turnWinners.Add(winner);
            winner.gems.Update((int)winner.gems.value + gemsWon);
            if (turnWinners.Count == props.TURNS_TO_WIN)
            {
                Finish();
            }

        }

        public override void Finish()
        {
            User winner = null;
            foreach (var user in users)
            {
                if (winner == null)
                {
                    winner = user;
                }
                else
                {
                    if ((int)winner.gems.value > (int)user.gems.value)
                    {
                        winner = user;
                    }
                }
            }

            QuixConsole.Log("Winner!", winner.sessionId);
        }

    }
}