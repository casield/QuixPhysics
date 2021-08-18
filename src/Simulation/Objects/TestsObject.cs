using System;
using System.Numerics;
using BepuPhysics.Constraints;

namespace QuixPhysics
{
    public class TestsObject : Item
    {
        public Player2 player;

        public override void Instantiate(Room room, Vector3 position)
        {
            room.factory.Create(new SphereState
            {
                mass = 1,
                position = position,
                instantiate = true,
                radius = 60,
                quaternion = Quaternion.Identity,
                type = "TestObject"
            }, room, this);
        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
           // AddConstrain();
        }

        private void AddConstrain()
        {
            var a = handle.bodyHandle;
            var b = player.handle.bodyHandle;
           room.simulator.Simulation.Solver.Add(b, a, new BallSocket { LocalOffsetA = new Vector3(0, 1, 0), LocalOffsetB = new Vector3(0, 100, 0), SpringSettings = new SpringSettings(30, 1) });
            room.simulator.Simulation.Solver.Add(b, a, new AngularHinge { LocalHingeAxisA = new Vector3(0, 1, 0), LocalHingeAxisB = new Vector3(0, -100, 0), SpringSettings = new SpringSettings(30, 1) });
        }
    }
}