using System;

namespace implementation
{
    class ExampleSolverOffline : ISolverOffline
    {
        public void solve(OfflineProblem problem)
        {
            Console.WriteLine(problem.ToString());
        }
    }
}