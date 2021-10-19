using System;
using System.Linq;
using System.IO;
using System.Collections;
using static implementation.Parser;
using System.Collections.Generic;


namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                bool test = false;
                bool benchmark = true;
                bool validate = false;

                //Test();
                if (test)
                {
                    Test();
                }
                if (benchmark)
                {
                    Benchmark();
                }
                if (validate)
                {
                    Validate();
                }
            }
            else
            {
                switch (args[0])
                {
                    case "benchmark": Benchmark(); break;
                    case "test": Test(); break;
                    case "case": RunCase(args); break;
                }
            }
        }

        static void RunCase(String[] args)
        {
            if (args.Length < 3)
                return;

            var (solverName, testFile) = (args[1], args[2]);
            var solver = makeSolver(solverName);

            OfflineProblem problem = ParseOfflineProblem(testFile);

            var sol = solver.solve(problem);
            Console.WriteLine(sol.machines);
            //new OfflineValidator(problem).validate(sol);
            //i'll trust you I guess
        }

        static IOfflineSolver makeSolver(String name)
        {
            switch (name)
            {
                case "sat": return new IntSatSolver();
                case "bf": return new RecursiveBruteforce();
                case "ilp": return new BranchAndBoundSolverOffline();
                default: throw new Exception($"invalid solver name: {name}");
            }
        }

        static void Benchmark()
        {
            var solvers = new List<IOfflineSolver>(new IOfflineSolver[] {
                //new RecursiveBruteforce(),
                new IntSatSolver(),
                new BranchAndBoundSolverOffline()
            });

            /*String prologPath = "C:\\Program Files\\swipl\\bin\\swipl.exe";
            if (File.Exists(prologPath))
            {
                Type[] offline_solver_types = { typeof(BranchAndBoundSolverOffline) };
                Type[] online_solver_types = { typeof(ExampleSolverOnline) };
                string[] offline_problem_files = { "../data/offline/from_assignment.txt", "../data/offline/backtracker.txt", "../data/offline/big_numbers.txt", "../data/offline/three_quarters.txt" };
                string[] online_problem_files = { "../data/online/from_assignment.txt" };

                run_using_solvers_and_files(offline_solver_types, offline_problem_files, test_offline_solver);
                run_using_solvers_and_files(online_solver_types, online_problem_files, test_online_solver);
            }

            if (benchmark)
            {
                ISolverOffline[] solvers = { new CallableSolverOffline(), new BranchAndBoundSolverOffline() };
                Console.WriteLine(Directory.GetCurrentDirectory());
                var clpfd = new CallableSolver(prologPath, new String[] { "..\\..\\..\\Callables\\constraint_programming.pl" });
                solvers.Add(clpfd);
            }*/


            var res = new Benchmarker(false, false).BenchmarkAll(solvers.ToArray(), 60.0);
            Console.WriteLine(res.ToString());
        }

        static void Test()
        {
            Type[] offline_solver_types =
            {
                typeof(BranchAndBoundSolverOffline),
                //typeof(RecursiveBruteforce),
                //typeof(IntSatSolver)
            };
            Type[] online_solver_types =
            {

            };
            List<string> offline_problem_files = new List<string>
            {
                "../data/offline/from_assignment.txt",
                "../data/offline/backtracker.txt",
                "../data/offline/big_numbers.txt",
                "../data/offline/three_quarters.txt"
            };
            List<string> online_problem_files = new List<string>
            {
                "../data/online/from_assignment.txt"
            };
            run_using_solvers_and_files(offline_solver_types, offline_problem_files, test_offline_solver);
            run_using_solvers_and_files(online_solver_types, online_problem_files, test_online_solver);
        }
        static void Validate()
        {
            // validator test
            if (Directory.Exists("../data/offline/"))
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

                ValidateTestcases(problems, solutions);
            }
            else
            {
                Console.WriteLine("warning: can't find unit test path");
            }
        }

        static private void ValidateTestcases(List<string> problem_filenames, List<string> solution_filenames)
        {
            for (int i = 0; i < problem_filenames.Count; i++)
            {
                OfflineProblem prob = ParseOfflineProblem(problem_filenames[i]);
                Solution2D sol = ParseSolution2D(solution_filenames[i]);
                OfflineValidator val = new OfflineValidator(prob);
                Console.WriteLine($"Validating {problem_filenames[i]} and {solution_filenames[i]}...");
                val.validate(sol);
            }
        }
        static private void run_using_solvers_and_files(Type[] solver_types, List<string> problem_files, Action<Type, string> test_method)
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
            IOfflineSolver solver = Activator.CreateInstance(solver_type) as IOfflineSolver;
            OfflineProblem problem = ParseOfflineProblem(problem_file);
            Solution solution = solver.solve(problem);
            OfflineValidator validator = new OfflineValidator(problem);
            validator.validate(solution);
            Console.WriteLine("Problem:");
            Console.WriteLine(problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());
        }
        static private void test_online_solver(Type solver_type, string problem_file)
        {
            IOnlineSolver solver = Activator.CreateInstance(solver_type) as IOnlineSolver;
            OnlineProblem problem = ParseOnlineProblem(problem_file);
            Solution solution = solver.solve(problem);
            //OfflineValidator.validate(problem, solution); //TODO need to implement an online validator
            Console.WriteLine("Problem:");
            Console.WriteLine(problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());
        }
    }
}
