using System;
using System.Linq;
using System.IO;
using System.Collections;
using static implementation.Parser;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            HospitalSolution testSolution = Parse_solution("../data/big_numbers.txt");
            Console.WriteLine(testSolution.ToString());

            RecursiveBruteforce brute = new RecursiveBruteforce();
            Solution solution = brute.solve(offline_problem);
            OfflineValidator validator = new OfflineValidator(offline_problem, solution);
            validator.validate();

            Console.WriteLine(solution);

            OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
            ExampleSolverOnline online_solver = new ExampleSolverOnline();
            online_solver.solve(online_problem);*/

            bool test = true;
            bool benchmark = false;
            bool validate = true;


            OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
            if (test)
            {
                Type[] offline_solver_types = { typeof(BranchAndBoundSolverOffline), typeof(CallableSolverOffline), typeof(RecursiveBruteforce) };
                Type[] online_solver_types = { typeof(ExampleSolverOnline) };
                string[] offline_problem_files = { "../data/offline/backtracker.txt" };
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

            if (validate)
            {
                List<string> problems = new List<string>
                {
                    "../data/offline/backtracker.txt",
                    "../data/offline/big_numbers.txt",
                    "../data/offline/three_quarters.txt",
                    "../data/online/Problem1.txt",
                    "../data/online/Problem2.txt"
                };

                List<string> solutions = new List<string>
                {
                    "../data/solutions/offline/backtracker.txt",
                    "../data/solutions/offline/big_numbers.txt",
                    "../data/solutions/offline/three_quarters.txt",
                    "../data/solutions/online/Solution1.txt",
                    "../data/solutions/online/Solution2.txt"
                };
                testABunch(problems, solutions);
            }
        }

        static private void testABunch(List<string> problem_filenames, List<string> solution_filenames)
        {
            for (int i = 0; i < problem_filenames.Count; i++)
            {
                OfflineProblem prob = Parse_problem_offline(problem_filenames[i]);
                HospitalSolution sol = Parse_solution(solution_filenames[i]);
                OfflineValidator val = new OfflineValidator(prob, sol);
                Console.WriteLine($"Validating {problem_filenames[i]} and {solution_filenames[i]}...");
                val.validate();
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
            OfflineValidator validator = new OfflineValidator(problem, solution);
            validator.validate();
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
