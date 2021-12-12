using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using System;
using System.Collections.Generic;
using System.Numerics;
using ContentLoader;
using Newtonsoft.Json;
using System.Diagnostics;

namespace QuixPhysics
{
    public class PhyObjectFactory
    {

        Simulator simulator;

        internal DummyPart Create(SphereState sphereState)
        {
            throw new NotImplementedException();
        }

        Room room;
        public Dictionary<string, PhyObject> objects = new Dictionary<string, PhyObject>();
        Simulation Simulation;
        public Dictionary<BodyHandle, PhyObject> objectsHandlers = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<StaticHandle, PhyObject> staticObjectsHandlers = new Dictionary<StaticHandle, PhyObject>();

        public Dictionary<Guid, PhyObject> allObjects = new Dictionary<Guid, PhyObject>();

        public Dictionary<Guid, PhyObject> OnContactListeners = new Dictionary<Guid, PhyObject>();

        private Stopwatch stopwatch = new Stopwatch();
        public PhyObjectFactory(Room room)
        {
            this.room = room;
            this.simulator = room.simulator;
            this.Simulation = simulator.Simulation;
        }
        /// <summary>
        /// Creates a PhyObject. If the instantiated value is not null, it will take that instance and create the Physic object and add it to it.
        /// Otherwise it will create a new class instance of the state.type string. (e.g. state.type == 'Player2')
        /// </summary>
        /// <param name="state"></param>
        /// <param name="room"></param>
        /// <param name="instantiated"></param>
        /// <returns></returns>
        public PhyObject Create(ObjectState state, Room room, PhyObject instantiated = null,bool bodyRotates=true)
        {

            PhyObject phy;
            if (instantiated == null)
            {
                phy = GetPhyClass(state.type);
                //Create a bot... should not be this way...
                if (phy is Player2 && state.owner == "Bot")
                {
                    phy = new PlayerBot();
                }
            }
            else
            {
                phy = instantiated;
            }
            instantiated = phy;
            if (state.uID == null || objects.ContainsKey(state.uID))
            {
                state.uID = PhyObject.createUID();
            }

            phy.BeforeLoad(state);

            if (state is BoxState)
            {
                if (phy is MeshBox)
                {
                    phy = CreateMesh((BoxState)state, room, instantiated,bodyRotates);
                }
                else
                {
                    phy = CreateBox((BoxState)state, room, instantiated,bodyRotates);
                }

            }
            if (state is SphereState)
            {
                phy = CreateSphere((SphereState)state, room, instantiated,bodyRotates);
            }


            if (!objects.ContainsKey(state.uID))
            {
                objects.Add(state.uID, phy);
            }
            else
            {
                QuixConsole.Log("Objects already had that key", state.uID);
            }

            if (state.owner != null)
            {
                if (room.users.ContainsKey(state.owner))
                {
                    room.users[state.owner].objects.Add(phy);
                }
            }
            return phy;

        }
        private PhyObject CreateVanilla(ObjectState state, CollidableDescription collidableDescription, BodyInertia bodyInertia, Room room, PhyObject instantiated)
        {
            PhyObject phy = instantiated;
            Guid guid = Guid.NewGuid();
            var material = new SimpleMaterial
            {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(30, 1f),
                collidable = true,
                guid = guid
            };
            if (state.quaternion == new Quaternion(0, 0, 0, 0))
            {
                state.quaternion = Quaternion.Identity;
            }

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

                SimpleMaterial allocatedMat = simulator.collidableMaterials.Allocate(bodyHandle) = material;

                var handle = new Handle { bodyHandle = bodyHandle };

                phy.Load(handle, room.connectionState, simulator, state, guid, room);


                objectsHandlers.Add(bodyHandle, phy);
                allObjects.Add(guid, phy);

            }
            else
            {

                StaticDescription description = new StaticDescription(state.position, state.quaternion, collidableDescription);
                StaticHandle statichandle = Simulation.Statics.Add(description);

                simulator.collidableMaterials.Allocate(statichandle) = material;
                var handle = new Handle { staticHandle = statichandle };

                phy.Load(handle, room.connectionState, simulator, state, guid, room);

                staticObjectsHandlers.Add(statichandle, phy);
                allObjects.Add(guid, phy);

            }


            return phy;
        }

        private PhyObject CreateBox(BoxState state, Room room, PhyObject instantiated,bool bodyRotates)
        {

            var box = new Box(state.halfSize.X, state.halfSize.Y, state.halfSize.Z);

            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(box), 0.1f);
            BodyInertia bodyInertia;
            if(bodyRotates){
                 box.ComputeInertia(state.mass, out bodyInertia);
            }else{
                bodyInertia = new BodyInertia{InverseMass=1/state.mass};
            }
           


            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room, instantiated);
            return phy;

        }

        private PhyObject CreateSphere(SphereState state, Room room, PhyObject instantiated,bool bodyRotates)
        {

            var sphere = new Sphere(state.radius);
            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(sphere), 0.1f);
            BodyInertia bodyInertia;
            if(bodyRotates){
                sphere.ComputeInertia(state.mass, out bodyInertia);
            }else{
                bodyInertia = new BodyInertia{InverseMass=1/state.mass};
            }
            

            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room, instantiated);
            return phy;
        }

        private PhyObject CreateMesh(BoxState state, Room room, PhyObject instantiated,bool bodyRotates)
        {
            LoadModel(simulator.server.GetMesh(state.mesh), out var mesh, state.halfSize);

            //fs.Close();

            TypedIndex shapeIndex = Simulation.Shapes.Add(mesh);

            CollidableDescription collidableDescription = new CollidableDescription(shapeIndex, 0.1f);
            BodyInertia bodyInertia;
            if(bodyRotates){
               mesh.ComputeClosedInertia(state.mass, out bodyInertia); 
            }else{
                bodyInertia = new BodyInertia{InverseMass=1/state.mass};
            }
            

            var phy = CreateVanilla(state, collidableDescription, bodyInertia, room, instantiated);
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

        public void SendUpdate()
        {
            var set = Simulation.Bodies.Sets[0];
            string[] bodies2 = new string[allObjects.Count];
            int bodiesAdded = 0;

            int bodyIndex = 0;
            foreach (var body in objectsHandlers)
            {
                if (body.Value.bodyReference.Awake)
                {
                    bodies2[bodyIndex] = body.Value.getJSON();
                    bodyIndex++;

                    bodiesAdded++;
                }
            }



            foreach (var item in staticObjectsHandlers)
            {
                if (item.Value.needUpdate)
                {
                    //bodies.Add(item.Value.getJSON());
                    bodies2[bodiesAdded] = item.Value.getJSON();
                    //QuixConsole.Log("Updating",item.Value.state.type,item.Value.state.position);
                    item.Value.needUpdate = false;
                    bodiesAdded += 1;
                }
            }

            if (bodiesAdded > 0)
            {
                var slice = bodies2[0..bodiesAdded];
                simulator.SendMessage("update", JsonConvert.SerializeObject(slice), room.connectionState.workSocket);
            }
        }
        internal void createObjects()
        {
            int width = 10;
            int max = 3000;
            var random = new Random();
            for (int a = 0; a < 0; a++)
            {
                var box = new SphereState();
                box.uID = PhyObject.createUID();
                box.uID += "" + a;
                box.mass = 10;
                box.type = "Bomb";
                // box.instantiate = false;

                int x = a % width;    // % is the "modulo operator", the remainder of i / width;
                int y = a / width;    // where "/" is an integer division
                //box.position = new Vector3(x * sizeObj, 1050 + (timesPressedCreateBoxes * sizeObj), y * sizeObj);
                box.position = new Vector3(random.Next(-max, max), 2500, random.Next(-max, max));
                box.radius = 10;
                box.mesh = "Board/Bomb";
                box.instantiate = false;
                box.quaternion = Quaternion.Identity;
                var b = Create(box, room);
            }
        }
        internal void Dispose()
        {
            foreach (var item in objects)
            {
                item.Value.Destroy();
            }
            objects.Clear();
            objectsHandlers.Clear();
            staticObjectsHandlers.Clear();
        }

    }
}