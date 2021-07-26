using System;
using System.Numerics;
using FluentBehaviourTree;

namespace QuixPhysics
{
    struct CrocoProperties
    {
        public int watchLength;
    }
    public class CrocoBehaviour
    {

        public CrocoLoco crocoLoco;
        private IBehaviourTreeNode tree;
        private TimeData timeData;
        private CrocoProperties properties;

        public CrocoBehaviour(CrocoLoco _crocoLoco)
        {
            crocoLoco = _crocoLoco;
            var builder = new BehaviourTreeBuilder();
            tree = builder
                .Selector("Watch players")
                .Condition("Is any player visible",IsAnyPlayerVisible)

                .End()
                .Build();
                timeData = new TimeData();

                properties = new CrocoProperties();
                properties.watchLength = 300;
        }

        private bool IsAnyPlayerVisible(TimeData arg)
        {
           /* foreach (var user in crocoLoco.simulator.users)
            {
                var distance = Vector3.Distance(crocoLoco.reference.Pose.Position,user.Value.player.reference.Pose.Position);
                QuixConsole.Log(distance);
                if(distance < properties.watchLength){
                    return true;
                }
            }
            return false;*/
            return false;
        }

        public void Tick()
        {

        }
    }
}