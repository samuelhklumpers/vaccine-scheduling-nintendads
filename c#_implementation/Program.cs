using System;
using System.Linq;
using System.IO;
using System.Collections;
using static implementation.Parser;
using System.Collections.Generic;
using System.Diagnostics;


namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                //RatioOnline(new VeryGreedyOnline(), new string[] { "10", "10", "1234" });
                //OfflineProblem prob = ParseOfflineProblem("./tests/offline/1.txt");
                //GreedyOffline greed = new GreedyOffline();
                //BFSBnBOffline bfs = new BFSBnBOffline();
                //Solution sol = bfs.solve(prob);
                //OfflineValidator val = new OfflineValidator(prob);
                //Console.WriteLine(sol);
                //BenchmarkOffline2(new BFSBnBOffline(), new string[] {"1.0","10","696969"} ); 
            }
            else if (args.Count() >= 3)
            {
                bool offline = true;
                string[] extra = args.Skip(3).ToArray();

                switch (args[1])

                {
                    case "offline": offline = true; break;
                    case "online": offline = false; break;
                }

                if (offline)
                {
                    IOfflineSolver solver = null;

                    switch (args[2])
                    {
                        case "sat": solver = new IntSatSolver(); break;
                        case "bf": solver = new RecursiveBruteforce(); break;
                        case "ilp": solver = new BranchAndBoundSolverOffline(); break;
                        case "bnb": solver = new BFSBnBOffline(); break;
                        default: throw new Exception($"invalid solver name: {args[2]}");
                    }

                    switch (args[0])
                    {
                        case "benchmark": BenchmarkOffline(solver, extra); break;
                        case "series": BenchmarkOffline2(solver, extra); break;
                        case "test": TestOffline(solver, extra); break;
                        case "case": RunCaseOffline(solver, extra); break;
                    }
                }
                else
                {
                    IOnlineSolver solver = null;

                    switch (args[2])
                    {
                        case "greedy": solver = new GreedyOnline(); break;
                        case "forward": solver = new Algorithm1(); break;
                        case "lexi": solver = new VeryGreedyOnline(); break;
                        default: throw new Exception($"invalid solver name: {args[2]}");
                    }

                    switch (args[0])
                    {
                        case "compete": CompeteOnline(solver, extra); break;
                        case "test": TestOnline(solver); break;
                        case "case": RunCaseOnline(solver, extra); break;
                        case "ratio": RatioOnline(solver, extra); break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Usage: xyz.exe <mode> <offline> <solver> [extra]");
            }
        }

        static void BenchmarkOffline(IOfflineSolver solver, string[] args)
        {
            double timeout = 60;

            if (args.Count() > 0)
            {
                timeout = double.Parse(args[0]);
            }

            var res = new Benchmarker(false, false).BenchmarkAll(new IOfflineSolver[] { solver }, timeout);

            Console.WriteLine(res.ToString());
        }


        static void BenchmarkOffline2(IOfflineSolver solver, string[] args)
        {
            double timeout = double.Parse(args[0]);
            int tries = int.Parse(args[1]);
            int seed = int.Parse(args[2]);

            var res = Benchmarker.BenchmarkSeries2(solver, timeout, tries, seed);
            Console.WriteLine(res);
        }

        static void TestOffline(IOfflineSolver solver, string[] args)
        {
            var testFile = args[0];
            var problem = ParseOfflineProblem(testFile);

            var alg = solver.solve(problem);
            Console.WriteLine(alg.To2D(problem));
        }

        static void RunCaseOffline(IOfflineSolver solver, string[] args)
        {
            var testFile = args[0];
            var problem = ParseOfflineProblem(testFile);
            var sol = solver.solve(problem);
            Console.WriteLine(sol.machines);
        }

        static void CompeteOnline(IOnlineSolver solver, string[] args)
        {
            var testFile = args[0];
            var problem = ParseOnlineProblem(testFile);

            var alg = solver.solve(problem);
            var opt = new IntSatSolver().solve(problem.CountN());

            new OfflineValidator(problem.CountN()).validate(opt);
            new OnlineValidator(problem).validate(alg);

            //Console.WriteLine($"alg: {alg.machines}");
            //Console.WriteLine($"opt: {opt.machines}");
            //Console.WriteLine($"ratio: {(double)alg.machines / opt.machines}");
            Console.WriteLine((double)alg.machines / opt.machines);
        }

        static void RatioOnline(IOnlineSolver solver, string[] args)
        {
            var runs = int.Parse(args[0]);
            var size = int.Parse(args[1]);
            var seed = int.Parse(args[2]);

            var (avg, worst) = Benchmarker.RandomRatioOnline(solver, runs, size, seed);
            Console.WriteLine(avg + " " + worst);
        }

        static void AdversaryOnline()
        {
            Console.WriteLine("adversary...");

            IOnlineSolver solver = new VeryGreedyOnline();

            var (problem, alg) = SimpleAdversary.Triple(solver);
            var opt = new IntSatSolver().solve(problem.CountN());

            //Console.WriteLine(problem);
            //Console.WriteLine(opt);
            //Console.WriteLine(alg);

            new OfflineValidator(problem.CountN()).validate(opt);
            new OnlineValidator(problem).validate(alg);

            Console.WriteLine($"alg: {alg.machines}");
            Console.WriteLine($"opt: {opt.machines}");
            Console.WriteLine($"ratio: {(double)alg.machines / opt.machines}");
        }

        static void TestOnline(IOnlineSolver solver)
        {

        }

        static void RunCaseOnline(IOnlineSolver solver, string[] args)
        {
            var testFile = args[0];
            var problem = ParseOnlineProblem(testFile);
            var sol = solver.solve(problem);



            Console.WriteLine(sol.machines);
        }


        // old

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

            var res = new Benchmarker(false, false).BenchmarkAll(solvers.ToArray(), 60.0);
            Console.WriteLine(res.ToString());
        }

        /*static void TestOffline()
        {
            var timer = new Stopwatch();

            timer.Start();

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
                "../data/offline/big_numbers.txt",
                "../data/offline/three_quarters.txt",
                "../data/offline/backtracker.txt",
                "./tests/offline/0.txt",
                "./tests/offline/1.txt",
                "./tests/offline/2-1.txt",
                "./tests/offline/2-2.txt",
                "./tests/offline/3-1.txt",
                "./tests/offline/3-2.txt",
                "./tests/offline/3-3.txt",
                "./tests/offline/4-1.txt",
                "./tests/offline/12.txt",
                "./tests/offline/15.txt",
                "./tests/offline/20.txt",
                "./tests/offline/45.txt",
                "./tests/offline/60.txt",
                "./tests/offline/100-1.txt",
                "./tests/offline/100-2.txt",
                "./tests/offline/340.txt"
            };
            List<string> online_problem_files = new List<string>
            {
                "../data/online/from_assignment.txt"
            };
            run_using_solvers_and_files(offline_solver_types, offline_problem_files, test_offline_solver);
            run_using_solvers_and_files(online_solver_types, online_problem_files, test_online_solver);

            timer.Stop();

            double dt = timer.Elapsed.TotalSeconds;

            Console.WriteLine($"passed {dt}s");
        }*/

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

        /*static private void test_online_solver(Type solver_type, string problem_file)
        {
            IOnlineSolver solver = Activator.CreateInstance(solver_type) as IOnlineSolver;
            OnlineProblem problem = ParseOnlineProblem(problem_file);
            Solution solution = solver.solve(problem);
            //OfflineValidator.validate(problem, solution); //TODO need to implement an online validator
            Console.WriteLine("Problem:");
            Console.WriteLine(problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());
        }*/

        /*static void TestOnline()
        {

            var solvers = new List<IOnlineSolver>(new IOnlineSolver[] {
                new GreedyOnline(),
                new VeryGreedyOnline(),
                new Algorithm1()
            });

            Benchmarker.RandomTestOnline(solvers.ToArray(), 10, 10, true);
        }*/
    }
}
