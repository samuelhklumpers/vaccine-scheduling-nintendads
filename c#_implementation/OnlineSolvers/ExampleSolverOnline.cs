using System;

namespace implementation
{
    class ExampleSolverOnline : IOnlineSolver
    {
        public Solution2D solve(OnlineProblem problem)
        {
            Console.WriteLine(problem.ToString());
            return null;
        }
    }
}