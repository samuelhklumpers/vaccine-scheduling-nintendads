using System;
using System.Linq;
using System.IO;
using System.Collections;
using static implementation.Parser;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {

            bool test = true;
            bool benchmark = false;


            if (test)
            {
                Type[] offline_solver_types = { typeof(BranchAndBoundSolverOffline), typeof(CallableSolverOffline) };
                Type[] online_solver_types = { typeof(ExampleSolverOnline) };
                string[] offline_problem_files = { "../data/offline/from_assignment.txt" };
                string[] online_problem_files = { "../data/online/from_assignment.txt" };

                run_using_solvers_and_files(offline_solver_types, offline_problem_files, test_offline_solver);
                run_using_solvers_and_files(online_solver_types, online_problem_files, test_online_solver);
            }

            if (benchmark)
            {
                ISolverOffline[] solvers = { new CallableSolverOffline(), new BranchAndBoundSolverOffline() };

                var bench = Benchmarker.benchmark(solvers, 5.0);

                Console.WriteLine(bench.ToString());
            }
        }
        static private void run_using_solvers_and_files(Type[] solver_types, string[] problem_files, Action<Type, string> test_method)
        {
            foreach (var solver_type in solver_types)
            {
                foreach (string problem_file in problem_files)
                {
                    Console.WriteLine("---------------" + solver_type + "---------------");
                    Console.WriteLine("input: " + problem_file);
                    try
                    {
                        test_method(solver_type, problem_file);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                    Console.WriteLine("---------------------------------------------");
                }
            }
        }
        static private void test_offline_solver(Type solver_type, string problem_file)
        {
            ISolverOffline solver = Activator.CreateInstance(solver_type) as ISolverOffline;
            OfflineProblem problem = Parse_problem_offline(problem_file);
            Solution solution = solver.solve(problem);
            OfflineValidator.validate(problem, solution);
            Console.WriteLine("Problem:");
            Console.WriteLine(problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());
        }
        static private void test_online_solver(Type solver_type, string problem_file)
        {
            ISolverOnline solver = Activator.CreateInstance(solver_type) as ISolverOnline;
            OnlineProblem problem = Parse_problem_online(problem_file);
            Solution solution = solver.solve(problem);
            OnlineValidator.validate(problem, solution);
            Console.WriteLine("Problem:");
            Console.WriteLine(problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());
        }
    }
}
