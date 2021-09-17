using System;

namespace implementation
{
    class ExampleSolverOnline : ISolverOnline
    {
        public void solve(OnlineProblem problem)
        {
            Console.WriteLine(problem.ToString());
        }
    }
}