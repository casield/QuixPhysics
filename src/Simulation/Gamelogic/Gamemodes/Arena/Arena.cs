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
        private GameFlowAddon gameFlowAddon;
        public HextilesAddon hextilesAddon;

        public List<ArenaAddon> addons = new List<ArenaAddon>();

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
            gameFlowAddon = new GameFlowAddon(simulator, this);

            addons.Add(hextilesAddon);
            addons.Add(userLoader);
            addons.Add(aIManager);
            addons.Add(gameFlowAddon);

            GenerateMap();
        }
        public override void OnJoin(User user)
        {
            users.Add(user);

            if (users.Count == 1)
            {
                Start();
            }
            AddPlayer(user);
        }

        public override Vector3 GetStartPoint(User user)
        {
            var point = new Vector3();
            //GetStartPointOnMap
            if (hextilesAddon != null)
            {
                point = new Vector3(0,600,0);
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
        private void AddPlayer(User user)
        {
            var startpoint = room.gamemode.GetStartPoint(user);
            user.CreatePlayer(startpoint);
            userLoader.LoadUser(user);
        }

        private void GenerateMap()
        {
            foreach (var item in addons)
            {
                item.OnMapsLoaded();
            }

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

            // CreatePlayers();
            var timeout = new PhyTimeOut(10000, simulator, true);
            timeout.Completed += () =>
            {

                foreach (var item in addons)
                {
                    item.OnStart();
                }
                GenerateNavMesh();
                QuixConsole.Log("NavMesh ready", navMeshName);
            };
            simulator.workers.Add(timeout);
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

            settings.AgentHeight = 3f * mulre;
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

    }
}