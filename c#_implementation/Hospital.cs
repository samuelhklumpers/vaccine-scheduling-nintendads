using System.Collections.Generic;    
    class Hospital
    {
        public int id;
        public Dictionary<int, bool> isBusyAt;

        public Hospital(int id)
        {
            this.id = id;
            this.isBusyAt = new Dictionary<int,bool>();
        }
    }