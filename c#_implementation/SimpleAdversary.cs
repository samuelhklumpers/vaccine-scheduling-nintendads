using System;
using System.Collections.Generic;

namespace implementation
{
	public class SimpleAdversary
	{

		public static (OnlineProblem, Solution2D) Triple(IOnlineSolver solver)
		{
			var problem = new OnlineProblem(2, 1, 0, new List<Patient>());
			var parameters = problem.parameters;

			var dt = 10;

			problem.patients.Add(new Patient(dt, dt + 3, 2 * dt, 2 * dt, 2, 1, 0));

			var sol = new Solution2D(0, new List<Doses2D>());
			sol = solver.Step(sol, problem.patients[0], parameters);

			var t1 = sol.hospitals[0].t1;

			problem.patients.Add(new Patient(0, 1, t1 - 2, 2, 2, 1, 0));
			sol = solver.Step(sol, problem.patients[1], parameters);

			var t2 = sol.hospitals[1].t2;

			problem.patients.Add(new Patient(2, 3, t2 - 4, 1, 2, 1, 0));
			sol = solver.Step(sol, problem.patients[2], parameters);

			new OnlineValidator(problem).validate(sol);

			return (problem, sol);
		}
	}
}