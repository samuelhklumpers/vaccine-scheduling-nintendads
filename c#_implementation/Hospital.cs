using System.Collections.Generic;    
    class Hospital
    {
        public int id;
        public Dictionary<int, bool> busy_dict;

        public Hospital(int id)
        {
            this.id = id;
            this.busy_dict = new Dictionary<int,bool>();
        }
    }