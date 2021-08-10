using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD;
using BepuPhysics.Collidables;
using BepuUtilities;
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

                        var entity = new Aspose.ThreeD.Entities.Box();
                        var node = scene.RootNode.CreateChildNode(entity);

                        if (state.isMesh)
                        {
                            /* Mesh shape = simulator.Simulation.Shapes.GetShape<Mesh>(pobj.shapeIndex.Index);
                             Vector3 max;
                             Vector3 min;

                             shape.ComputeBounds(state.quaternion, out min, out max);

                             Vector3 size = Vector3.Subtract(max, min);

                             QuixConsole.Log("Mesh " + state.type, min, max);
                             QuixConsole.Log("Mesh size", size);
                             node.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3(size.X / resizer, size.Y / resizer, size.Z / resizer);
                             node.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3(-(state.position.X / resizer),( state.position.Y / resizer), -(state.position.Z / resizer));
                             //node.Transform.*/
                        }
                        else
                        {

                            var size = state.halfSize;


                            Vector3 position = state.position;



                            node.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3((position.X / resizer), position.Y / resizer, (position.Z / resizer));
                            node.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3((size.X) / resizer, size.Y / resizer, size.Z / resizer);


                            QuaternionEx.GetAxisAngleFromQuaternion(state.quaternion, out Vector3 axis, out float angle);

                             var noventa = Quaternion.CreateFromAxisAngle(new Vector3(-axis.X, -axis.Y, -axis.Z), -angle);

                            node.Transform.Rotation = new Aspose.ThreeD.Utilities.Quaternion(noventa.W,noventa.X,noventa.Y,noventa.Z);

                        }


                    }
                }

                using (FileStream fs = File.Create(FILES_DIR + name +".obj"))
                {
                    scene.Save(fs, FileFormat.WavefrontOBJ);
                    fs.Dispose();
                }



            }
            return name;
        }

        public static bool PointInPoly(Vector3 pt,NavPolyId polyId,TiledNavMesh navMesh)
		{
            NavTile curTile;
            NavPoly curPoly;
            navMesh.TryGetTileAndPolyByRef(polyId,out curTile,out curPoly);

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
