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
        public bool Started = false;

        private Vector3 arrivePosition;
        private Player2 target;

        private Vector3 firstPosition;
        private PhyWaiter pointWaiter;
        private Vector3 lastPosition;

        public Villan()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            //state.mesh = "Board/Crocodile/Crocodile";
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].FrictionCoefficient = .1f;


            arena = (Arena)room.gamemode;
            vehicle = new Vehicle(this, new VehicleProps() { maxSpeed = new Vector3(10f, 10f, 10f) });
            vehicle.isActive = true;

            navMeshQuery = new NavMeshQuery(arena.tiledNavMesh, 2048);
            pointWaiter = new PhyWaiter(3000);



            trail = new Trail(simulator, this, navMeshQuery);
            trail.OnLastPoint += OnLastPoint;
            firstPosition = state.position;

            trail.Start();

            AddWorker(new PhyInterval(1, simulator)).Completed += Update;

        }



        private bool IsFalling()
        {
            return GetPosition().Y < -50;
        }

        private void OnLastPoint()
        {
            
        }
        private void OnStuck()
        {
            /*  trail.Start();
              LookRandomPoint();*/
            QuixConsole.Log("Stucked", trail.IsActive());
           // OnLastPoint();

            LookTarget(target);

        }
        public void LookRandomPoint()
        {
            if(!trail.IsActive()){
                trail.Start();
            }
            var closepo = navMeshQuery.FindNearestPoly(GetPosition(), trail.GetExtend());
            navMeshQuery.FindRandomConnectedPoint(ref closepo, out NavPoint random);
            QuixConsole.Log("On Last point", random);

            arrivePosition = random.Position;
            trail.SetTarget(random.Position);
        }

        private void Update()
        {
            if (trail.IsActive())
            {
                if(trail.hasFinished){
                    if(Distance(target.GetPosition())<100){
                        Wait10SecondsToReactivate();
                    }
                }
                vehicle.SeekFlee(trail.GetPoint(), true);
            }
            else
            {
                /* if (Distance(target.GetPosition()) < 6000)
                 {
                     trail.Start();
                     LookTarget(target);
                 }*/
            }

            CheckPositionForStuck();
            if (IsFalling())
            {
                OnFall();
            }



            vehicle.Update();
            bodyReference.Velocity.Linear *= .9f;
        }
        private void Wait10SecondsToReactivate()
        {
            trail.Stop();
            AddWorker(new PhyTimeOut(10000,simulator,true)).Completed+=()=>{
                trail.Restart();
                trail.Start();
                LookTarget(target);
            };
        }

        private void OnFall()
        {
            SetPosition(firstPosition);
            OnLastPoint();
        }

        private void CheckPositionForStuck()
        {
            var distance = Distance(lastPosition);
            if (distance < 5)
            {
                if (pointWaiter.Tick())
                {
                    //Too many time in one point
                    OnStuck();
                    pointWaiter.Reset();

                }



            }
            else
            {
                lastPosition = GetPosition();
            }

            //  QuixConsole.Log("Dis", Distance(lastPosition));



        }
        public float Distance(Vector3 target)
        {
            return Vector3.Distance(GetPosition(), target);
        }

        public bool IsTargetVisible()
        {
            //TODO> Best player detection
            return Distance(target.GetPosition()) < 400;
        }

        public void LookTarget(Player2 target)
        {
            if (trail.SetTarget(target.GetPosition()))
            {
                this.target = target;
                QuixConsole.Log("Look player");

                arrivePosition = target.GetPosition();

            }
        }
        public void ChangeTarget()
        {

        }
    }
}