using System;
using System.Collections.Generic;

namespace implementation
{
    class ExampleSolverOnline : ISolverOnline
    {
        public Solution solve(OnlineProblem problem)
        {
            return new Solution(0, new List<Registration>());
        }
    }
}