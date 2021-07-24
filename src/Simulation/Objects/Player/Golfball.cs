using System;
using BepuPhysics;
using BepuPhysics.Constraints;

namespace QuixPhysics{
    public class GolfBall2:PhyObject{

        public BodyReference reference;

        public GolfBall2(){

        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
             simulator.narrowPhaseCallbacks.CollidableMaterials.Allocate(bodyHandle) = new SimpleMaterial
                {
                    FrictionCoefficient = 1f,
                    MaximumRecoveryVelocity = float.MaxValue,
                    SpringSettings = new SpringSettings(5, .00001f)
                };
                reference=GetReference();
              /*  material.FrictionCoefficient = 1;
                material.SpringSettings = new SpringSettings(1,0.00001f);*/
        }
    }
}