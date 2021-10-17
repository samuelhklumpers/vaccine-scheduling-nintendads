using System;
using System.IO;
using static implementation.Parser;
using System.Collections.Generic;


namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test();
            //Benchmark();
            TestOnline();
        }

        static void TestOnline()
        {
            ForwardMinimizeOnline f = new ForwardMinimizeOnline();
            // Console.WriteLine(f.calculateScore(new bool[3,3] {{true, true, true}, {true, true,false}, {true, false, false}})); // seems to be fine.

            // Okay, how do I test it? Where are our online test_cases?
            // I need an OnlineProblem. Make a simple one, I suppose.

            List<Patient> ps = new List<Patient>();
            int p1 = 2, p2 = 3, g = 0;
            ps.Add(new Patient(0, 2, 0, 3, p1, p2, g)); // why do I have to give p1, p2, g for patient?
            ps.Add(new Patient(5, 10, 0, 3, p1, p2, g));
            OnlineProblem o = new OnlineProblem(p1, p2, g, ps);

            // Doses2D a = new Doses2D(1,1,1,1);
            // Doses2D b = new Doses2D(1,1,1,1);
            // bool c = a == b;
            // Console.WriteLine(c);

            // List<Dose2D> ds = new List<Dose2D>();
            // ds.Add(new Dose2D(0, 0));
            // ds.Add(new Dose2D(2, 0));
            // ds.Add(new Dose2D(1, 1));
            // ds.Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t));
            // foreach(Dose2D d in ds)
            //     Console.Write(d.t + ", ");
            
            Solution2D sol = f.solve(o);
            // here goes nothing...
            Console.WriteLine("NO ERRORS!");
            foreach(Doses d in sol.doses)
                Console.Write(d.t1 + ", "+ d.t2 + "; ");


            // to add quickly (not needing sort), and remove quickly (not traversing everything).
            // SORTEDLIST REMOVE IS O(n) OPERATION.
            // SORTEDLIST ADD IS O(n) OPERATION. so only prevents the sort. (at least something)
            // I would like quick ADD, quick REMOVE, and no SORT. Write it yourself then...
            //   to add-sort a List of Ints.

            // YOOOO. On the most simple tasks, it runs!!
            // Now: 1. It is sub-optimal.
            //      2. Test-cases.
            //      3. Re-factor, etc.
        }

        static void Benchmark()
        {
            var solvers = new List<IOfflineSolver>(new IOfflineSolver[] {
                //new RecursiveBruteforce(),
                new IntSatSolver(),
            });

            /*String prologPath = "C:\\Program Files\\swipl\\bin\\swipl.exe";
            if (File.Exists(prologPath))
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
                var clpfd = new CallableSolver(prologPath, new String[] { "..\\..\\..\\Callables\\constraint_programming.pl" });
                solvers.Add(clpfd);
            }*/


            var res = new Benchmarker(false, false).BenchmarkAll(solvers.ToArray(), 10.0);
            Console.WriteLine(res.ToString());
        }

        static void Test()
        {
            var solvers = new List<IOfflineSolver>(new IOfflineSolver[] {
                new RecursiveBruteforce(), // TODO this one fails
                new IntSatSolver(),
            });

            Benchmarker.RandomTest(solvers.ToArray(), 10, 10);

            // validator test
            if (File.Exists("../data/offline/"))
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

        static private void ValidateTestcases(List<string> problem_filenames, List<string> solution_filenames) {
            for (int i = 0; i < problem_filenames.Count; i++){
                OfflineProblem prob = ParseOfflineProblem(problem_filenames[i]);
                Solution2D sol = ParseSolution2D(solution_filenames[i]);
                OfflineValidator val = new OfflineValidator(prob);
                Console.WriteLine($"Validating {problem_filenames[i]} and {solution_filenames[i]}...");
                val.validate(sol);
            }
        }
    }
}
