namespace QuixPhysics{
    public class User{
        public string sessionId;
        public Player2 player;

        public User(string id,Player2 player){
            sessionId = id;
            this.player = player;
        }
    }
}