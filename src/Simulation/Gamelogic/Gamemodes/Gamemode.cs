namespace QuixPhysics{
    public interface IGamemode{
        string name {get;set;}
        User winner{get;set;}
        int gameTime{get;set;}

        bool started {get;set;}

        void OnJoin(User user);
        void Start();
        void Finish();
        void Pause();
        void Update();
        
    }
    public abstract class Gamemode : IGamemode
    {
        private Simulator simulator;

        public string name { get; set; }
        public User winner { get; set; }
        public bool started { get; set; }
        public int gameTime { get; set; }

        public Gamemode(Simulator simulator){
            this.simulator = simulator;
        }

        public virtual void Finish()
        {
           
        }

        public virtual void OnJoin(User user)
        {
            
        }

        public virtual void Pause()
        {
           
        }

        public virtual void Start()
        {
            
        }

        public virtual void Update()
        {
        
        }
    }
}