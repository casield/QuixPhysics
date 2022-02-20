using System;
using System.Collections.Generic;

namespace QuixPhysics.Player
{
    /// <summary>
    /// This class manages the actions of the player. For example: Running, Jumping, etc...
    /// </summary>
    public class ActionsManager
    {
        private Player2 player;
        private List<PlayerAction> activeActions = new List<PlayerAction>();


        public MoveAction moveAction;
        public JumpAction jumpAction;
        public RotationAction rotationAction;
        public ShootAndGrabAction shootAction;
        public RaycastAction raycastAction;
        public FallAction fallAction;
        public  GrabBallAction grabBallAction;
        public DamageAction damageAction;

        public ActionsManager(Player2 player)
        {
            this.player = player;

            SetActions();
        }
        /// <summary>
        /// Set all the available actions
        /// </summary>
        private void SetActions()
        {
            moveAction = new MoveAction(player);

            jumpAction = new JumpAction(player);
            rotationAction = new RotationAction(player);
            shootAction = new ShootAndGrabAction(player);
            raycastAction = new RaycastAction(player);
            fallAction = new FallAction(player);
            grabBallAction = new GrabBallAction(player);
            damageAction = new DamageAction(player);

        }
        /// <summary>
        /// Add the action to the activated actions list.
        /// </summary>
        /// <param name="action"></param>
        private void ActivateAction(PlayerAction action)
        {
            if (!activeActions.Contains(action))
            {
                activeActions.Add(action);
            }
            action.OnActivate();

        }
        /// <summary>
        /// Removes the action frome the active actions list. Every action is responsable for calling this method.
        /// </summary>
        /// <param name="action"></param>
        internal void RemoveAction(PlayerAction action)
        {
            activeActions.Remove(action);
        }
        /// <summary>
        /// Call OnUpdate method for all the active actions 
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < activeActions.Count; i++)
            {
                activeActions[i].OnUpdate();
            }
        }

        #region Action's public methods

        public void Move(XYMessage message)
        {
            ActivateAction(moveAction);
            moveAction.SetMoveMessage(message);
        }
        public void Jump(XYMessage message)
        {
            ActivateAction(jumpAction);

        }
        public void TakeDamage(int damage){
            damageAction.SetDamage(damage);
            ActivateAction(damageAction);
        }

        internal void Rotate(XYMessage message)
        {
            ActivateAction(rotationAction);
            rotationAction.SetRotateMessage(message);
        }
        public void Shoot(ShootMessage message){
            ActivateAction(shootAction);
            shootAction.Shoot(message);
        }

        internal void Fall()
        {
           ActivateAction(fallAction);
        }
        internal void GrabBall(){
            ActivateAction(grabBallAction);
        }

        internal void RayCast()
        {
            ActivateAction(raycastAction);
        }
        #endregion
    }
}