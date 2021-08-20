namespace QuixPhysics{

    public abstract class HelperItem : Item,EntityLifeLoop
    {
        Helper helper;
        public bool finished = false;

        public HelperItem(Helper helper){
            this.helper = helper;
        }
        /// <summary>
        /// This method returns true if the item should be activated.
        /// </summary>
        /// <returns></returns>
        public abstract bool ShouldActivate();
        public abstract void Activate();
        public abstract void Desactivate();

        public abstract bool OnLastPolygon();

        public abstract void OnTrailInactive();

        public abstract void OnTrailActive();
    }
}