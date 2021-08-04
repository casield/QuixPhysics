using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using Aspose.ThreeD;
using BepuPhysics.Collidables;
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
        public NavMesh GenerateNavMesh(string name)
        {
            Stopwatch stopWatch = new Stopwatch();

            var model = new ObjModel(FILES_DIR + name + ".obj");
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
            return navMesh;
        }
        private void SaveNavMeshToFile(string name)
        {
            if (!hasGenerated)
            {
                QuixConsole.WriteLine("No navmesh generated or loaded, cannot save.");
                return;
            }

            try
            {
                new NavMeshJsonSerializer().Serialize(name, navMesh);
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

        public string CreateMesh(List<PhyObject> objects, string name)
        {
            Scene scene = new Scene();
            foreach (var pobj in objects)
            {
                if (pobj.state is BoxState)
                {
                    BoxState state = (BoxState)pobj.state;


                    var node = scene.RootNode.CreateChildNode(new Aspose.ThreeD.Entities.Box());
                    if (state.isMesh)
                    {
                        Mesh shape = simulator.Simulation.Shapes.GetShape<Mesh>(pobj.shapeIndex.Index);
                        Vector3 max;
                        Vector3 min;

                        shape.ComputeBounds(state.quaternion, out min, out max);

                        Vector3 size = Vector3.Subtract(max,min);

                        QuixConsole.Log("Mesh "+state.type, min, max);
                        QuixConsole.Log("Mesh size",size);
                        node.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3(size.X, size.Y,size.Z);
                        node.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3(-state.position.X, state.position.Y, -state.position.Z);
                        //node.Transform.
                    }
                    else
                    {
                        node.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3(state.halfSize.X*2, state.halfSize.Y*2 , state.halfSize.Z*2 );
                          node.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3(-state.position.X, state.position.Y, -state.position.Z);
                    }

                  
                }
            }

            // Create a Cylinder model
            //scene.RootNode.CreateChildNode("cylinder", new Aspose.ThreeD.Entities.Pyramid());
            using (FileStream fs = File.Create(FILES_DIR + name + ".obj"))
            {
                scene.Save(fs, FileFormat.WavefrontOBJ);
                fs.Dispose();
            }

            return name;

        }
    }

    class SceneStream : Stream
    {
        private Stream inner;
        public override bool CanRead { get => inner.CanRead; }

        public override bool CanSeek { get => inner.CanSeek; }

        public override bool CanWrite { get => inner.CanWrite; }

        public override long Length { get => inner.Length; }

        private StringBuilder builder;

        public override long Position { get; set; }

        public SceneStream()
        {
            builder = new StringBuilder();
        }
        public override void Flush()
        {
            inner.Flush();

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            QuixConsole.Log("Read", buffer.Length);

            return inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            QuixConsole.Log("Seek");
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string str = Encoding.Default.GetString(buffer);
            builder.Append(str);
            inner.Write(buffer, offset, count);
        }
    }
}