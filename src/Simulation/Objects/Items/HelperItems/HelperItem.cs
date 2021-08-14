namespace QuixPhysics{

    public abstract class HelperItem : Item,EntityLifeLoop
    {
        Helper helper;
        public bool finished = false;

        public HelperItem(Helper helper){
            this.helper = helper;
        }
        public abstract void Activate();
        public abstract void Desactivate();
        public abstract void Update();

        public abstract bool OnLastPolygon();

        public abstract void OnTrailInactive();

        public abstract void OnTrailActive();
    }
}