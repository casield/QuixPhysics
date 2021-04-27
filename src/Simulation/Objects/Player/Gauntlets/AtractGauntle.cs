using System;
using BepuPhysics;

namespace QuixPhysics{
    public class AtractGauntlet : IGauntlet
    {
        Player2 Player;
        BodyReference golfBallRef;
        Vehicle vehicle;
        public void Activate()
        {
            if(!isActivated()){
                var timer = new PhyInterval(1,Player.simulator);
                timer.Completed+=OnTick;
                golfBallRef = Player.golfball.GetReference();
                vehicle = new Vehicle(Player.golfball);
                QuixConsole.Log("Activaded","xd","asdasd");
            }
            if(isActivated()){
                vehicle.isActive=true;
            }
        }

        private bool isActivated(){
            return Player!=null&&vehicle!=null;
        }

        private void OnTick()
        {
            if(Player!=null){
                vehicle.Seek(Player.reference.Pose.Position);
                vehicle.Update();
                if(Player.IsSnapped()){
                    vehicle.isActive = false;
                }
            }
        }

        public void AddPlayer(Player2 player)
        {
            this.Player =player;
        }
    }
}