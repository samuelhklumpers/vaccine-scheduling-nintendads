using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class ExampleSolverOffline : IOfflineSolver
    {
        public Solution solve(OfflineProblem problem)
        {
            Console.WriteLine(problem.ToString());
            return new Solution(0, new List<Doses>());
        }
    }
}