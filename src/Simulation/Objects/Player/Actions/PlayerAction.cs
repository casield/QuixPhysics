
using QuixPhysics;

public interface IPlayerAction
    {
        /// <summary>
        /// This method is called when the action is activated
        /// </summary>
        void OnActivate();
        /// <summary>
        /// Update method when this PlayerAction is active.
        /// </summary>
        void OnUpdate();
        Player2 player {get;set;}
    }
    public abstract class PlayerAction : IPlayerAction
    {
        public Player2 player { get; set; }

        public PlayerAction(Player2 player){
            SetPlayer(player);
        }

        public abstract void OnActivate();

        public abstract void OnUpdate();

        public void SetPlayer(Player2 player){
            this.player = player;
        }

        public void Remove(){
            player.actionsManager.RemoveAction(this);
        }

    }