using System;
using System.Numerics;
using BepuUtilities;

namespace QuixPhysics
{
    public class ObjectAnimation
    {
        public PhyObject phyObject;

        public Vector3[] Animation { get; private set; }
        private Vector3 localPosition = new Vector3();
        public int animationPosition = 0;
        public bool loop = false;

        private float animationVelocity = .1f;
        private float threshold = 10f;
        private float angle = 0;

        public ObjectAnimation(PhyObject obj)
        {
            phyObject = obj;

        }
        public void Start()
        {

        }
        public void SetAnimation(Vector3[] animation)
        {
            Animation = animation;
            localPosition = Animation[0];
        }
        /// <summary>
        /// Sets the velocity of the animation. With a higher value it goes slower.
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(float vel)
        {
            animationVelocity = vel;
        }

        private Vector3 GetLastAnimationPosition()
        {

            if (animationPosition == 0) return Animation[0];

            return Animation[animationPosition - 1];
        }
        private Vector3 GetAnimationPosition()
        {
            return Animation[animationPosition];
        }

        public void Update()
        {
            var distance = localPosition - GetAnimationPosition();
            if (localPosition == Animation[0] && GetAnimationPosition() == Animation[0])
            {
                localPosition = Animation[0];
                animationPosition++;
                return;
            }
            localPosition = Vector3.Lerp(localPosition, GetAnimationPosition(), animationVelocity / distance.Length());

            if (distance.Length() <= threshold && animationPosition<Animation.Length-1)
            {
                animationPosition++;
                if (animationPosition == Animation.Length )
                {
                    if(loop){
                        RestartAnimation();
                    }
                    
                }
            }

        }

        public void RestartAnimation()
        {

            animationPosition = 0;
            localPosition = Animation[0];

        }

        public Vector3 RotateToDirection(Quaternion quaternion)
        {
            var transform = Matrix3x3.CreateFromQuaternion(quaternion);

            Matrix3x3.Transform(localPosition, transform, out Vector3 outOrigin);
            outOrigin.X = -outOrigin.X;
            outOrigin.Z = -outOrigin.Z;

            return outOrigin;
        }
    }
}