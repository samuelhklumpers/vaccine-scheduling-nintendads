using System;

namespace implementation
{
    public class ExampleSolverOnline : IOnlineSolver
    {
        public Solution2D solve(OnlineProblem problem)
        {
            Console.WriteLine(problem.ToString());
            return null;
        }

        public Solution2D Step(Solution2D partial, Patient nextPatient, Parameters parameters)
        {
            return null;
        }
    }
}