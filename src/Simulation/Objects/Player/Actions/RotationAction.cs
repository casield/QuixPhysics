using System;
using System.Numerics;
using BepuUtilities;

namespace QuixPhysics.Player
{
    public class RotationAction : PlayerAction
    {
        private XYMessage rotateMessage;
        private float rotationAcceleration;
        private static float _rotationSpeed = .5f;
        private float rotationSpeed = _rotationSpeed;
        private float maxAcc = .4f;

        public RotationAction(Player2 player) : base(player)
        {
        }

        public override void OnActivate()
        {

        }
        public void SetRotateMessage(XYMessage message)
        {
          
            rotateMessage = message;  
        }

        public override void OnUpdate()
        {
            if (!player.cameraLocked)
            {
                if (rotateMessage.clientId != null)
                {
                    var x= Math.Abs(rotateMessage.x);
                    var distance = 1-x;
                    player.bodyReference.Awake = true;
                    if (x > 0)
                    {
                        player.bodyReference.Awake = true;
                        rotationAcceleration +=(rotationSpeed) * rotateMessage.x;
                    
                        rotationAcceleration = Math.Clamp(rotationAcceleration, -maxAcc, maxAcc);
                        rotationAcceleration/=100;
                       // MathF.Pow(2f,(7/x));
                        
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