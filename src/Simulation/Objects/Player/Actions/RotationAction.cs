using System;
using System.Numerics;
using BepuUtilities;

namespace QuixPhysics.Player
{
    public class RotationAction : PlayerAction
    {
        private XYMessage rotateMessage;
        private float rotationAcceleration;
        private float rotationSpeed = 2.5f;
        private float maxAcc = 0.5f;

        public RotationAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {
           
        }
        public void SetRotateMessage(XYMessage message){
            rotateMessage = message;
        }

        public override void OnUpdate()
        {
            if (!player.cameraLocked)
            {
                if (rotateMessage.clientId != null)
                {
                    player.bodyReference.Awake = true;

                    if (Math.Abs(rotateMessage.x) > 0)
                    {
                        player.bodyReference.Awake = true;
                        rotationAcceleration += rotationSpeed * rotateMessage.x;
                        rotationAcceleration = Math.Clamp(rotationAcceleration, -maxAcc, maxAcc);
                        rotationAcceleration /= 100;
                    }

                    player.lookObject.AddY(rotateMessage.y);

                    if (rotateMessage.y == 0)
                    {
                        player.lookObject.Release();
                    }

                    if (rotateMessage.x == 0)
                    {
                        rotationAcceleration *= .9f;
                    }
                    if (player.lookObject.watching is Player2)
                    {
                        player.rotationController += rotationAcceleration;
                    }
                    else
                    {
                        var prod = AngleBetweenVector2(player.lookObject.GetPosition(), player.GetPosition());
                        player.rotationController = prod;
                    }

                    player.state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-(player.rotationController), 0, 0);
                }
            }
        }

        float AngleBetweenVector2(Vector3 vec1, Vector3 vec2)
        {
            var deltx = vec1.X - vec2.X;
            var delty = vec1.Z - vec2.Z;


            var rot = MathF.Atan2(-delty, -deltx);
            return rot;
        }
    }
}