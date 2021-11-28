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
        /// <summary>
        /// This method is called after ShouldActivate returned true.
        /// </summary>
        public abstract void Activate();
        /// <summary>
        /// This method should be called when the item has endend it's function. It changes the EntityLifeLoop to null.
        /// </summary>
        public virtual void Desactivate(){
            helper.ChangeLoop(null);
        }

        public abstract bool OnLastPolygon();

        public abstract void OnTrailInactive();

        public abstract void OnTrailActive();

        public abstract void OnStuck();

        public abstract void OnFall();
    }
}