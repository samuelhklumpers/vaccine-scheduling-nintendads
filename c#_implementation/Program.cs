using System;
using System.IO;
using static implementation.Parser;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {

            bool test = false;
            bool benchmark = true;


            if (test)
            {
                Console.WriteLine("Running offline solver");
                OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
                CallableSolverOffline offline_solver = new CallableSolverOffline("C:\\Program Files\\swipl\\bin\\swipl.exe", new String[] { "constraint_programming.pl" });

                // can't seem to convince C# to start an executable from path
                var solution = offline_solver.solve(offline_problem);
                OfflineValidator.validate(offline_problem, solution);
                Console.WriteLine("Problem:");
                Console.WriteLine(offline_problem.ToString());
                Console.WriteLine("\nSolution:");
                Console.WriteLine(solution.ToString());

                Console.WriteLine("Running online solver");
                OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
                ExampleSolverOnline online_solver = new ExampleSolverOnline();
                online_solver.solve(online_problem);
            }

            if (benchmark)
            {
                var solver = new CallableSolverOffline("C:\\Program Files\\swipl\\bin\\swipl.exe", new String[] { "constraint_programming.pl" });
                ISolverOffline[] solvers = new ISolverOffline[] { solver };

                var bench = Benchmarker.benchmark(solvers, 5.0);

                Console.WriteLine(bench.ToString());
            }
        }
    }
}
