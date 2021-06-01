using BepuUtilities;
using BepuUtilities.Memory;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Diagnostics;
using BepuUtilities.Collections;
using System.Collections.Generic;

namespace QuixPhysics
{
    public struct QuixPoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        /// <summary>
        /// Gravity to apply to dynamic bodies in the simulation.
        /// </summary>
        public Vector3 Gravity;
        /// <summary>
        /// Fraction of dynamic body linear velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.
        /// </summary>
        public float LinearDamping;
        /// <summary>
        /// Fraction of dynamic body angular velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.
        /// </summary>
        public float AngularDamping;

        Vector3 gravityDt;
        float linearDampingDt;
        float angularDampingDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public void Initialize(Simulation simulation)
        {
            //In this demo, we don't need to initialize anything.
            //If you had a simulation with per body gravity stored in a CollidableProperty<T> or something similar, having the simulation provided in a callback can be helpful.
        }

        /// <summary>
        /// Creates a new set of simple callbacks for the demos.
        /// </summary>
        /// <param name="gravity">Gravity to apply to dynamic bodies in the simulation.</param>
        /// <param name="linearDamping">Fraction of dynamic body linear velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.</param>
        /// <param name="angularDamping">Fraction of dynamic body angular velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.</param>
        public QuixPoseIntegratorCallbacks(Vector3 gravity, float linearDamping = .1f, float angularDamping = .03f) : this()
        {
            Gravity = gravity;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
        }

        public void PrepareForIntegration(float dt)
        {
            //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            gravityDt = Gravity * dt;
            //Since these callbacks don't use per-body damping values, we can precalculate everything.
            linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt);
            angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            //Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
            if (localInertia.InverseMass > 0)
            {
                velocity.Linear = (velocity.Linear + gravityDt) * linearDampingDt;
                velocity.Angular = velocity.Angular * angularDampingDt;
            }
            //Implementation sidenote: Why aren't kinematics all bundled together separately from dynamics to avoid this per-body condition?
            //Because kinematics can have a velocity- that is what distinguishes them from a static object. The solver must read velocities of all bodies involved in a constraint.
            //Under ideal conditions, those bodies will be near in memory to increase the chances of a cache hit. If kinematics are separately bundled, the the number of cache
            //misses necessarily increases. Slowing down the solver in order to speed up the pose integrator is a really, really bad trade, especially when the benefit is a few ALU ops.

            //Note that you CAN technically modify the pose in IntegrateVelocity by directly accessing it through the Simulation.Bodies.ActiveSet.Poses, it just requires a little care and isn't directly exposed.
            //If the PositionFirstTimestepper is being used, then the pose integrator has already integrated the pose.
            //If the PositionLastTimestepper or SubsteppingTimestepper are in use, the pose has not yet been integrated.
            //If your pose modification depends on the order of integration, you'll want to take this into account.

            //This is also a handy spot to implement things like position dependent gravity or per-body damping.
        }

    }
    public delegate void ContactAction(PhyObject A, PhyObject B);
    public unsafe struct QuixNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public SpringSettings ContactSpringiness;
        public CollidableProperty<SimpleMaterial> CollidableMaterials;

        private Simulation simulation;
        public Simulator simulator;

        // public QuickList<BodyHandle> onContactListeners;
        //QuickDictionary<CollidableReference, QuickList<PreviousCollisionData>, CollidableReferenceComparer> listeners;

        public void Initialize(Simulation simulation)
        {
            this.simulation = simulation;
            CollidableMaterials.Initialize(simulation);
            // onContactListeners= new QuickList<BodyHandle>(4096,simulation.BufferPool);
            //Use a default if the springiness value wasn't initialized.
            //if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
            // ContactSpringiness = new SpringSettings(1000f, 0.001f);

            ContactSpringiness = new SpringSettings(10f, 0.1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            bool AcanCollide = true;
            bool BcanCollide = true;
           if (a.Mobility != CollidableMobility.Static || b.Mobility != CollidableMobility.Static)
            {
                if (a.Mobility == CollidableMobility.Dynamic)
                {

                    AcanCollide = simulator.objectsHandlers[a.BodyHandle].collidable;
                }
                if (a.Mobility == CollidableMobility.Static)
                {

                    AcanCollide = simulator.staticObjectsHandlers[a.StaticHandle].collidable;
                }
                if (b.Mobility == CollidableMobility.Dynamic)
                {

                    BcanCollide = simulator.objectsHandlers[b.BodyHandle].collidable;
                }
                if (b.Mobility == CollidableMobility.Static)
                {

                    BcanCollide = simulator.staticObjectsHandlers[b.StaticHandle].collidable;
                }
            }

            //While the engine won't even try creating pairs between statics at all, it will ask about kinematic-kinematic pairs.
            //Those pairs cannot emit constraints since both involved bodies have infinite inertia. Since most of the demos don't need
            //to collect information about kinematic-kinematic pairs, we'll require that at least one of the bodies needs to be dynamic.
            return BcanCollide && AcanCollide;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            var a = CollidableMaterials[pair.A];
            var b = CollidableMaterials[pair.B];

            pairMaterial.FrictionCoefficient = 0.5f;
            pairMaterial.MaximumRecoveryVelocity = 2000f;
            pairMaterial.SpringSettings = ContactSpringiness;


          if (pair.A.Mobility == CollidableMobility.Static)
            {
                if (simulator.OnStaticContactListeners.ContainsKey(pair.A.StaticHandle))
                {
                    var con = simulator.collidableToPhyObject(pair.B);
                    simulator.OnStaticContactListeners[pair.A.StaticHandle].OnContact(con);
                }
            }
            else
            {
                if (simulator.OnContactListeners.ContainsKey(pair.A.BodyHandle))
                {
                    var con = simulator.collidableToPhyObject(pair.B);
                    simulator.OnContactListeners[pair.A.BodyHandle].OnContact(con);
                }
            }

            if (pair.B.Mobility == CollidableMobility.Static)
            {
                if (simulator.OnStaticContactListeners.ContainsKey(pair.B.StaticHandle))
                {
                    var con = simulator.collidableToPhyObject(pair.A);
                    simulator.OnStaticContactListeners[pair.B.StaticHandle].OnContact(con);
                }
            }
            else
            {
                if (simulator.OnContactListeners.ContainsKey(pair.B.BodyHandle))
                {
                    var con = simulator.collidableToPhyObject(pair.A);
                    simulator.OnContactListeners[pair.B.BodyHandle].OnContact(con);
                }
            }



            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
    public struct SimpleMaterial
    {
        public SpringSettings SpringSettings;
        public float FrictionCoefficient;
        public float MaximumRecoveryVelocity;

        public static bool operator ==(SimpleMaterial op1, SimpleMaterial op2)
        {
            return op1.Equals(op2);
        }

        public static bool operator !=(SimpleMaterial op1, SimpleMaterial op2)
        {
            return !op1.Equals(op2);
        }
    }

}