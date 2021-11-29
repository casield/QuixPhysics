using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Aspose.ThreeD;
using SharpNav;
using SharpNav.IO;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public struct ArenaProps
    {
        public float MIN_USERS;
        public float TURNS_TO_WIN;
    }
    /// <summary>
    /// Arena is a gamemode where the players fight each other to get more gems.
    /// </summary>
    public class Arena : Gamemode
    {
        public ArenaProps props = new ArenaProps() { MIN_USERS = 1, TURNS_TO_WIN = 3 };
        public List<User> users = new List<User>();


        private UserLoader userLoader;
        public AIManager aIManager;
        public HextilesAddon hextilesAddon;

        private List<User> turnWinners = new List<User>();

        public List<PhyObject> navObjects = new List<PhyObject>();
        private NavMeshBuilder navMesh;
        public TiledNavMesh tiledNavMesh;
        private string navMeshName;

        public event PhyAction OnNavMeshReadyListeners;
        public NavMeshQuery navMeshQuery;

        public Arena(Simulator simulator, Room room) : base(simulator, room)
        {
            this.room = room;
            this.room.gamemode = this;
            this.simulator = simulator;
        }
        public override void Init()
        {
            hextilesAddon = new HextilesAddon(simulator, this);
            userLoader = new UserLoader(simulator, this);
            aIManager = new AIManager(simulator, this);

            GenerateMap();
        }
        public override void OnJoin(User user)
        {
            users.Add(user);

            if (users.Count == props.MIN_USERS)
            {
                Start();
            }

        }

        public override Vector3 GetStartPoint(User user)
        {
            var point = new Vector3();
            //GetStartPointOnMap
            if (hextilesAddon != null)
            {
                point = hextilesAddon.GetRandomHextile().GetPosition();
                QuixConsole.Log("Point", point);
            }

            return point;

        }

        private Vector3 GetStartPointOnMap(User user)
        {
            if (room.map != null)
            {

                var index = users.IndexOf(user);

                QuixConsole.Log("Start position", room.map.startPositions.Count, index, user);

                if (index <= -1 || index > room.map.startPositions.Count)
                {
                    QuixConsole.Log("users was -1");
                    index = 0;

                }

                var x = float.Parse(room.map.startPositions[index].AsBsonDocument["x"].ToString());
                var y = float.Parse(room.map.startPositions[index].AsBsonDocument["y"].ToString());
                var z = float.Parse(room.map.startPositions[index].AsBsonDocument["z"].ToString());
                return new Vector3(x, y, z);

            }
            return Vector3.Zero;
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
            // GenerateMapCommand command = new GenerateMapCommand(simulator);

            // var objs = command.GenerateMap("hexagons", room);                    
            // navObjects.AddRange(objs);


            userLoader.OnMapsLoaded();
            aIManager.OnMapsLoaded();
            hextilesAddon.OnMapsLoaded();
            room.factory.createObjects();
        }
        public NavPoint GetRandomPoint(Vector3 position, Vector3 extend)
        {
            var query = navMeshQuery;
            if (navMeshQuery != null)
            {
                var nearest = query.FindNearestPoly(position, extend);
                query.FindRandomConnectedPoint(ref nearest, out NavPoint randomPoint);
                return randomPoint;
            }
            return new NavPoint();

        }

        public override void Start()
        {

            CreatePlayers();


            GenerateNavMesh();
            aIManager.OnStart();
            userLoader.OnStart();
            hextilesAddon.OnStart();


            QuixConsole.Log("NavMesh ready", navMeshName);
        }

        private void GenerateNavMesh()
        {
            QuixNavMesh qinavMesh = new QuixNavMesh(simulator);
            navMeshName = "Arena";
            var settings = NavMeshGenerationSettings.Default;

            // Resizes the mesh and the navmesh
            // Set resizer to 100 to export on normal size
            // Set to 1 to normal rendering
            float resizer = 1;
            float mulre = (100 / resizer);

            settings.CellSize = .3f * mulre;
            settings.CellHeight = 1.30f * mulre;
            settings.MaxClimb = 2f;

            settings.AgentHeight = 3f*mulre;
            settings.AgentRadius = .2f * mulre;


            //Creates the mesh .obj
            qinavMesh.CreateMesh(navObjects, navMeshName, resizer);
            //Then it generates de NavMesh
            navMesh = qinavMesh.GenerateNavMesh(navMeshName, settings);

            //Creates the tilednavmesh
            tiledNavMesh = new TiledNavMesh(navMesh);

            navMeshQuery = new NavMeshQuery(tiledNavMesh, 1048);

            //Save it to a file .snb
            qinavMesh.SaveNavMeshToFile(navMeshName, tiledNavMesh);

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