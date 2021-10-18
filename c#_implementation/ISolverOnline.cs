using System.Collections.Generic;

namespace implementation
{
    public interface IOnlineSolver
    {
        ///Print the solution
        Solution2D solve(OnlineProblem problem)
        {
            var sol = new Solution2D(0, new List<Doses2D>());
            foreach (var patient in problem.patients)
            {
                sol = this.Step(sol, patient, problem.parameters);
            }

            return sol;
        }

        Solution2D Step(Solution2D partial, Patient nextPatient, Parameters parameters);

        public void Reset() { }
    }
}