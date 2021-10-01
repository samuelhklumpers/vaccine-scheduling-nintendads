using System;

namespace implementation
{
    class ExampleSolverOnline : IOnlineSolver
    {
        public void solve(OnlineProblem problem)
        {
            Console.WriteLine(problem.ToString());
        }
    }
}