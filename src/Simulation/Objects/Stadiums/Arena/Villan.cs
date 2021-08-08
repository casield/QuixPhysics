using System;
using System.Numerics;
using BepuPhysics;
using SharpNav;
using SharpNav.Pathfinding;

namespace QuixPhysics
{
    public class Villan : PhyObject
    {
        QuixNavMesh navMesh;
        Arena arena;

        Boolean Looking = true;
        private NavMeshQuery navMeshQuery;

        private Vehicle vehicle;
        private bool canAddPath = true;

        private Trail trail;

        private Vector3 seekPosition;

        public Villan()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            //state.mesh = "Board/Crocodile/Crocodile";
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);


            arena = (Arena)room.gamemode;
            vehicle = new Vehicle(this, new VehicleProps() { maxSpeed = new Vector3(0.1f, 0.1f, 0.1f) });
            vehicle.isActive = true;

            navMeshQuery = new NavMeshQuery(arena.tiledNavMesh, 2048);

            trail = new Trail(simulator, this, navMeshQuery);


            AddWorker(new PhyInterval(1, simulator)).Completed += Update;

        }

        private void Update()
        {
            if (trail.hasFinished)
            {
                vehicle.Arrive(seekPosition);
            }
            else
            {
                vehicle.SeekFlee(trail.GetNextPoint(), true);
            }

            vehicle.Update();
        }

        public void LookPlayer(Player2 player)
        {
            trail.SetTarget(player.GetPosition());
            trail.Start();
            seekPosition = player.GetPosition();
        }
    }
}