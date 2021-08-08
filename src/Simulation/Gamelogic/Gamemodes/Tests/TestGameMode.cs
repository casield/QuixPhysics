using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics
{
    public class TestGameMode : Gamemode
    {
        private List<PhyObject> navObjects = new List<PhyObject>();
        private QuixNavMesh navMesh;

        public TestGameMode(Simulator simulator, Room room) : base(simulator, room)
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
            GenerateMapCommand command = new GenerateMapCommand(simulator);

            var objs = command.GenerateMap("testnav", room);
            navObjects.AddRange(objs);

            simulator.createObjects(room);

            CreateNavMesh();
        }

        
        private void CreateNavMesh()
        {
          /*  navMesh = new QuixNavMesh(simulator);
            navMesh.CreateMesh(navObjects, "testNuevo");
            var realNav =navMesh.GenerateNavMesh("testNuevo");
            navMesh.SaveNavMeshToFile("testNuevo");*/
            
        }

        public override void OnJoin(User user)
        {
            user.CreatePlayer(GetStartPoint(user));
        }
    }
}