using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using System;
using System.Collections.Generic;
using System.Numerics;
using ContentLoader;
namespace QuixPhysics
{
    public class PhyObjectFactory
    {

        Simulator simulator;
        Room room;
        public Dictionary<string, PhyObject> objects = new Dictionary<string, PhyObject>();
        Simulation Simulation;
        public CollidableProperty<SimpleMaterial> collidableMaterials;
        public Dictionary<BodyHandle, PhyObject> objectsHandlers = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<StaticHandle, PhyObject> staticObjectsHandlers = new Dictionary<StaticHandle, PhyObject>();
        
        public Dictionary<Guid, PhyObject> allObjects = new Dictionary<Guid, PhyObject>();
        public PhyObjectFactory(Room room)
        {
            this.room = room;
            this.simulator = room.simulator;
            this.Simulation = simulator.Simulation;
        }

        public PhyObject Create(ObjectState state, Room room)
        {
            PhyObject phy = null;
            if (state.uID == null || objects.ContainsKey(state.uID))
            {
                state.uID = PhyObject.createUID();
            }
            if (state is BoxState)
            {
                if (state.isMesh)
                {
                    phy = CreateMesh((BoxState)state, room);
                }
                else
                {
                    phy = CreateBox((BoxState)state, room);
                }

            }
            if (state is SphereState)
            {
                phy = CreateSphere((SphereState)state, room);
            }


            if (!objects.ContainsKey(state.uID))
            {
                objects.Add(state.uID, phy);
            }
            else
            {
                QuixConsole.WriteLine("Objects already had that key");
            }
            return phy;

        }
        private PhyObject CreateVanilla(ObjectState state, CollidableDescription collidableDescription, BodyInertia bodyInertia, Room room)
        {
            PhyObject phy;
            Guid guid = Guid.NewGuid();
            var material = new SimpleMaterial
            {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(30, 1f),
                collidable = true,
                guid = guid
            };
            if (state.quaternion == new Quaternion())
            {
                state.quaternion = Quaternion.Identity;
            }
            if (state.mass != 0)
            {
                BodyDescription boxDescription = BodyDescription.CreateDynamic(state.position, bodyInertia,
                     collidableDescription,
                     new BodyActivityDescription(0.01f));

                boxDescription.Pose = new RigidPose(state.position, state.quaternion);
                var bodyHandle = Simulation.Bodies.Add(boxDescription);

                SimpleMaterial allocatedMat = collidableMaterials.Allocate(bodyHandle) = material;

                phy = SetUpPhyObject(new Handle { bodyHandle = bodyHandle }, state, guid, room);

                objectsHandlers.Add(bodyHandle, phy);
                allObjects.Add(guid, phy);

            }
            else
            {

                StaticDescription description = new StaticDescription(state.position, state.quaternion, collidableDescription);
                StaticHandle handle = Simulation.Statics.Add(description);

                collidableMaterials.Allocate(handle) = material;
                phy = SetUpPhyObject(new Handle { staticHandle = handle }, state, guid, room);

                staticObjectsHandlers.Add(handle, phy);
                allObjects.Add(guid, phy);

            }


            return phy;
        }
        private PhyObject CreateBox(BoxState state, Room room)
        {
            var box = new Box(state.halfSize.X, state.halfSize.Y, state.halfSize.Z);

            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(box), 0.1f);
            BodyInertia bodyInertia;

            box.ComputeInertia(state.mass, out bodyInertia);


            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room);
            return phy;

        }

        private PhyObject CreateSphere(SphereState state, Room room)
        {

            var sphere = new Sphere(state.radius);
            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(sphere), 0.1f);
            BodyInertia bodyInertia;

            sphere.ComputeInertia(state.mass, out bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room);
            return phy;
        }

        private PhyObject CreateMesh(BoxState state, Room room)
        {
            LoadModel(simulator.server.GetMesh(state.mesh), out var mesh, state.halfSize);

            //fs.Close();

            TypedIndex shapeIndex = Simulation.Shapes.Add(mesh);

            CollidableDescription collidableDescription = new CollidableDescription(shapeIndex, 0.1f);


            mesh.ComputeClosedInertia(state.mass, out var bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room);
            phy.shapeIndex = shapeIndex;
            objects.Add(state.uID, phy);

            return phy;
        }

        public void LoadModel(MeshContent meshContent, out Mesh mesh, Vector3 scale)
        {
            simulator.bufferPool.Take<Triangle>(meshContent.Triangles.Length, out var triangles);

            for (int i = 0; i < meshContent.Triangles.Length; ++i)
            {
                triangles[i] = new Triangle(meshContent.Triangles[i].A, meshContent.Triangles[i].B, meshContent.Triangles[i].C);
            }

            // scale.Y *=-1;
            mesh = new Mesh(triangles, scale, simulator.bufferPool);


        }

        private PhyObject GetPhyClass(string name)
        {
            System.Type t = System.Type.GetType("QuixPhysics." + name + ", QuixPhysics");

            PhyObject phy = null;
            if (t != null)
            {

                phy = (PhyObject)Activator.CreateInstance(t);
            }
            else
            {
                phy = new PhyObject();
            }
            return phy;
        }

        private PhyObject SetUpPhyObject(Handle bodyHandle, ObjectState state, Guid guid, Room room)
        {

            PhyObject phy = GetPhyClass(state.type);

            phy.Load(bodyHandle, simulator.connectionState, simulator, state, guid, room);

            return phy;
        }
    }
}