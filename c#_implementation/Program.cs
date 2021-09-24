using System;
using static implementation.Parser;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
            /*ExampleSolverOffline offline_solver = new ExampleSolverOffline();
            var solution = offline_solver.solve(offline_problem);
            new OfflineValidator().validate(offline_problem, solution); */

            BruteforceSolverOffline brute = new BruteforceSolverOffline();
            var solution = brute.solve(offline_problem);
            new OfflineValidator().validate(offline_problem, solution);

            OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
            ExampleSolverOnline online_solver = new ExampleSolverOnline();
            online_solver.solve(online_problem);
        }
    }
}
