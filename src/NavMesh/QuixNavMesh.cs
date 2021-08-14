using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using BepuPhysics.Collidables;
using BepuUtilities;
using ObjLoader.Loader.Loaders;
using QuixTest;
using SharpNav;
using SharpNav.IO.Json;
using SharpNav.Pathfinding;
using static ContentBuilder.MeshBuilder;

namespace QuixPhysics
{
    public class QuixNavMesh
    {
        private NavMesh navMesh;
        private NavMeshQuery navMeshQuery;
        private NavPoint startPt;
        private bool hasGenerated = true;
        private bool interceptExceptions;
        public static string FILES_DIR = "src/NavMesh/Files/";
        private Simulator simulator;

        public QuixNavMesh(Simulator simulator)
        {
            this.simulator = simulator;

        }
        // public bool I
        public NavMesh GenerateNavMesh(string name, NavMeshGenerationSettings settings)
        {
            QuixConsole.Log("Starting create new Mesh");
            Stopwatch stopWatch = new Stopwatch();

            var model = new ObjModel(FILES_DIR + name + ".obj");
            stopWatch.Start();
            //generate the mesh
            QuixConsole.Log(model.GetTriangles().Length);

            navMesh = NavMesh.Generate(model.GetTriangles(), settings);



            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("Navmesh generation " + ts);
            return navMesh;
        }
        public void SaveNavMeshToFile(string name)
        {
            if (!hasGenerated)
            {
                QuixConsole.WriteLine("No navmesh generated or loaded, cannot save.");
                return;
            }

            try
            {
                new NavMeshJsonSerializer().Serialize(FILES_DIR + name, navMesh);
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
        public TiledNavMesh GetTiledNavMesh(string name)
        {
            return new NavMeshJsonSerializer().Deserialize(FILES_DIR + name + ".snb");

        }
        public string CreateMesh(List<PhyObject> objects, string name, float resizer)
        {
            int repeat = 1;
            for (int i = 0; i < repeat; i++)
            {
                Scene scene = new Scene();
                foreach (var pobj in objects)
                {
                    if (pobj.state is BoxState)
                    {
                        BoxState state = (BoxState)pobj.state;

                        if (state.isMesh)
                        {
                            //Mesh shape = simulator.Simulation.Shapes.GetShape<Mesh>(pobj.shapeIndex.Index);
                            string nameObj = "Content/" + state.mesh + ".obj";
                            /* Scene loaderScene = new Scene();
                             QuixConsole.Log("Is mesh!");


                             loaderScene.Open(nameObj);


                             QuixConsole.Log("Entity", loaderScene.RootNode.ChildNodes.Count);
                              var node = scene.RootNode.CreateChildNode(loaderScene.RootNode.ChildNodes[0].Entity);*/

                            var PhyMesh = simulator.server.GetMesh(state.mesh);

                            using (FileStream fs = File.OpenRead(nameObj))
                            {
                               /* var result = new ObjLoaderFactory().Create(new MaterialStubLoader()).Load(fs);
                                Aspose.ThreeD.Entities.Mesh mesh = new Aspose.ThreeD.Entities.Mesh();
                                List<int> vertexIndex = new List<int>();
                                
                                foreach (var group in result.Groups)
                                {
                                    foreach (var face in group.Faces)
                                    {
                                        
                                        for (int a = 0; a < face.Count; a++)
                                        {
                                            var facee = face[a];
                                            
                                            for (int b = 0; b < face.Count; b++)
                                            {
                                                var bb = face[b];
                                                vertexIndex.Add(bb.VertexIndex);
                                            }
                                        }
                                    }
                                }
                            }*/
                        }



                        
                        // mesh.CreatePolygon()


                        //  SetNode(state.position, state.halfSize, state.quaternion, ref node, resizer);


                    }
                    else
                    {
                        var entity = new Aspose.ThreeD.Entities.Box();
                        var node = scene.RootNode.CreateChildNode(entity);
                        QuaternionEx.GetAxisAngleFromQuaternion(state.quaternion, out Vector3 axis, out float angle);


                        var noventa = Quaternion.CreateFromAxisAngle(new Vector3(axis.X, axis.Y, axis.Z), angle);

                        SetNode(state.position, state.halfSize, noventa, ref node, resizer);

                    }


                }
            }
            scene.RootNode.Transform.Scale *= new Aspose.ThreeD.Utilities.Vector3(1, 1, 1);

            using (FileStream fs = File.Create(FILES_DIR + name + ".obj"))
            {
                scene.Save(fs, FileFormat.WavefrontOBJ);
                fs.Dispose();
            }



        }
            return name;
        }
    private void SetNode(Vector3 position, Vector3 size, Quaternion quaternion, ref Node node, float resizer)
    {


        node.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3((position.X / resizer), position.Y / resizer, (position.Z / resizer));


        node.Transform.Rotation = new Aspose.ThreeD.Utilities.Quaternion(quaternion.W, quaternion.X, quaternion.Y, quaternion.Z);
        node.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3((size.X) / resizer, size.Y / resizer, size.Z / resizer);
    }

    public static bool PointInPoly(Vector3 pt, NavPolyId polyId, TiledNavMesh navMesh)
    {
        NavTile curTile;
        NavPoly curPoly;
        navMesh.TryGetTileAndPolyByRef(polyId, out curTile, out curPoly);

        int nverts = curPoly.VertCount;
        Vector3[] verts = curTile.Verts;

        bool c = false;

        for (int i = 0, j = nverts - 1; i < nverts; j = i++)
        {
            Vector3 vi = verts[i];
            Vector3 vj = verts[j];
            if (((vi.Z > pt.Z) != (vj.Z > pt.Z)) &&
                (pt.X < (vj.X - vi.X) * (pt.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
            {
                c = !c;
            }
        }

        return c;
    }


}
}
