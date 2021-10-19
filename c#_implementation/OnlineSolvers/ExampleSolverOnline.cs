using System;
using System.Collections.Generic;

namespace implementation
{
    class ExampleSolverOnline : IOnlineSolver
    {
        public Solution solve(OnlineProblem problem)
        {
            return new Solution(0, new List<Doses>());
        }
    }
}