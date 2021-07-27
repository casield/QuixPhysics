using System;
using System.Diagnostics;
using System.Numerics;
using QuixTest;
using SharpNav;
using SharpNav.IO.Json;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public class QuixNavMesh
    {
        private NavMesh navMesh;
        private NavMeshQuery navMeshQuery;
        private NavPoint startPt;
        private bool hasGenerated = true;
        private bool interceptExceptions;

        public QuixNavMesh()
        {
            Stopwatch stopWatch = new Stopwatch();

            var model = new ObjModel("src/NavMesh/nav_test.obj");
            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = 1.7f;
            settings.AgentRadius = .5f;
            settings.CellSize = .6f;
            stopWatch.Start();
            //generate the mesh
            navMesh = NavMesh.Generate(model.GetTriangles(), settings);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("Navmesh generation " + ts);

            navMeshQuery = new NavMeshQuery(navMesh, 2048);

            Vector3 c = new Vector3(0, 0, 0);
            Vector3 e = new Vector3(5, 5, 5);
            navMeshQuery.FindNearestPoly(ref c, ref e, out startPt);

            SaveNavMeshToFile("src/NavMesh/");

            QuixConsole.Log("Navmesh", startPt);
        }
        private void SaveNavMeshToFile(string path)
        {
            if (!hasGenerated)
            {
                QuixConsole.WriteLine("No navmesh generated or loaded, cannot save.");
                return;
            }

            try
            {
                new NavMeshJsonSerializer().Serialize(path, navMesh);
            }
            catch (Exception e)
            {
                if (!interceptExceptions)
                    throw;
                else
                {
                    Console.WriteLine("Navmesh saving failed with exception:" + Environment.NewLine + e.ToString());
                    return;
                }
            }

            Console.WriteLine("Saved to file!");
        }
    }
}