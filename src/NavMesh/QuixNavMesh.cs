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
using SharpNav.Geometry;
using SharpNav.IO.Json;
using SharpNav.Pathfinding;
using static ContentBuilder.MeshBuilder;

namespace QuixPhysics
{
    public class QuixNavMesh
    {
        private NavMeshBuilder buildData;
        private NavMeshQuery navMeshQuery;
        private NavPoint startPt;
        private bool hasGenerated = true;
        private bool interceptExceptions;
        public static string FILES_DIR = "src/NavMesh/Files/";
        private Simulator simulator;
        private Dictionary<string, Scene> scenesLoaded = new Dictionary<string, Scene>();
        private Heightfield heightfield;
        private CompactHeightfield compactHeightfield;
        private ContourSet contourSet;
        private PolyMesh polyMesh;
        private PolyMeshDetail polyMeshDetail;

        public QuixNavMesh(Simulator simulator)
        {
            this.simulator = simulator;

        }
        // public bool I
        public NavMeshBuilder GenerateNavMesh(string name, NavMeshGenerationSettings settings)
        {
            QuixConsole.Log("Starting create new Mesh");
            Stopwatch stopWatch = new Stopwatch();

            var model = new ObjModel(FILES_DIR + name + ".obj");
            stopWatch.Start();
            //generate the mesh
            QuixConsole.Log(model.GetTriangles().Length);


            //navMesh = NavMesh.Generate(model.GetTriangles(), settings);
            GenerateNavMeshFilters(model,settings);

            buildData = new NavMeshBuilder(polyMesh, polyMeshDetail, new SharpNav.Pathfinding.OffMeshConnection[0], settings);



            stopWatch.Stop();
         
            Console.WriteLine("Navmesh generation " + stopWatch.ElapsedMilliseconds);
            
            return hasGenerated?buildData:null;
        }

        private void GenerateNavMeshFilters(ObjModel level,NavMeshGenerationSettings settings){
            Console.WriteLine("Generating NavMesh");

			Stopwatch sw = new Stopwatch();
			sw.Start();
			long prevMs = 0;
			try
			{
				//level.SetBoundingBoxOffset(new SVector3(settings.CellSize * 0.5f, settings.CellHeight * 0.5f, settings.CellSize * 0.5f));
				var levelTris = level.GetTriangles();
				var triEnumerable = TriangleEnumerable.FromTriangle(levelTris, 0, levelTris.Length);
				BBox3 bounds = triEnumerable.GetBoundingBox();

				heightfield = new Heightfield(bounds, settings);

				Console.WriteLine("Heightfield");
				Console.WriteLine(" + Ctor\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				/*Area[] areas = AreaGenerator.From(triEnumerable, Area.Default)
					.MarkAboveHeight(areaSettings.MaxLevelHeight, Area.Null)
					.MarkBelowHeight(areaSettings.MinLevelHeight, Area.Null)
					.MarkBelowSlope(areaSettings.MaxTriSlope, Area.Null)
					.ToArray();
				heightfield.RasterizeTrianglesWithAreas(levelTris, areas);*/
				heightfield.RasterizeTriangles(levelTris, Area.Default);

				Console.WriteLine(" + Rasterization\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				Console.WriteLine(" + Filtering");
				prevMs = sw.ElapsedMilliseconds;

				heightfield.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);

				Console.WriteLine("   + Ledge Spans\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				heightfield.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);

				Console.WriteLine("   + Low Hanging Obstacles\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				heightfield.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

				Console.WriteLine("   + Low Height Spans\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				compactHeightfield = new CompactHeightfield(heightfield, settings);

				Console.WriteLine("CompactHeightfield");
				Console.WriteLine(" + Ctor\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;
				 
				compactHeightfield.Erode(settings.VoxelAgentRadius);

				Console.WriteLine(" + Erosion\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				compactHeightfield.BuildDistanceField();

				Console.WriteLine(" + Distance Field\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				compactHeightfield.BuildRegions(0, settings.MinRegionSize, settings.MergedRegionSize);

				Console.WriteLine(" + Regions\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				/*Random r = new Random();
				regionColors = new Color4[compactHeightfield.MaxRegions];
				regionColors[0] = Color4.Black;
				for (int i = 1; i < regionColors.Length; i++)
					regionColors[i] = new Color4((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255), 255);

				Console.WriteLine(" + Colors\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;*/

				contourSet = compactHeightfield.BuildContourSet(settings);

				Console.WriteLine("ContourSet");
				Console.WriteLine(" + Ctor\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				polyMesh = new PolyMesh(contourSet, settings);

				Console.WriteLine("PolyMesh");
				Console.WriteLine(" + Ctor\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				polyMeshDetail = new PolyMeshDetail(polyMesh, compactHeightfield, settings);

				Console.WriteLine("PolyMeshDetail");
				Console.WriteLine(" + Ctor\t\t\t\t" + (sw.ElapsedMilliseconds - prevMs).ToString("D3") + " ms");
				prevMs = sw.ElapsedMilliseconds;

				hasGenerated = true;


			}
			catch (Exception e)
			{
				if (!interceptExceptions)
					throw;
				else
					Console.WriteLine("Navmesh generation failed with exception:" + Environment.NewLine + e.ToString());
			}
			finally
			{
				sw.Stop();
			}
        }
        public void SaveNavMeshToFile(string name, TiledNavMesh tiled)
        {
            if (!hasGenerated)
            {
                QuixConsole.WriteLine("No navmesh generated or loaded, cannot save.");
                return;
            }

            try
            {
                new NavMeshJsonSerializer().Serialize(FILES_DIR + name,tiled);
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

                        if (typeof(MeshBox).IsInstanceOfType(pobj))
                        {
                            //Mesh shape = simulator.Simulation.Shapes.GetShape<Mesh>(pobj.shapeIndex.Index);
                            string nameObj = "Content/" + state.mesh + ".obj";
                            Scene loaderScene;
                            if (!scenesLoaded.ContainsKey(nameObj))
                            {
                                loaderScene = new Scene();
                            
                            
                                loaderScene.Open(nameObj);
                                scenesLoaded.Add(nameObj,loaderScene);

                            }else{
                                loaderScene = scenesLoaded[nameObj];
                            }

                            var node = scene.RootNode.CreateChildNode((loaderScene.RootNode.ChildNodes[0].Entity));
                            var newH = state.halfSize;


                            SetNode(state.position, newH, pobj.GetQuaternion(), ref node, resizer);


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
                PolygonModifier.Triangulate(scene);

                using (FileStream fs = File.Create("C:/Users/Casiel/Desktop/ciberchico420/C#/SharpNav/Source/SharpNav.Examples/nav_test.obj"))
                {
                    scene.Save(fs, FileFormat.WavefrontOBJ);
                    fs.Dispose();
                }

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
