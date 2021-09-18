using System.Collections.Generic;


namespace implementation
{
    class Solution
    {
        public int machines;
        public List<Registration> regs;

        public Solution(int machines, List<Registration> sol)
        {
            this.machines = machines;
            this.regs = sol;
        }

        public override string ToString()
        {
            string part1 = "machines: " + this.machines + "\n";
            string part2 = "";
            for (int i = 0; i < this.regs.Count; i++)
            {
                part2 += this.regs[i].ToString() + "\n";
            }
            return part1 + part2;
        }
    }
}
