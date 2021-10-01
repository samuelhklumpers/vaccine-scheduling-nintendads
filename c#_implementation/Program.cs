using System;
using System.IO;
using static implementation.Parser;
using System.Collections.Generic;


namespace implementation
{
    class Program
    {
        static void Test(string[] args)
        {
            // solver tests
            var solvers = new List<ISolverOffline>();
            solvers.Add(new RecursiveBruteforce());

            String prologPath = "C:\\Program Files\\swipl\\bin\\swipl.exe";
            if (File.Exists(prologPath))
            {
                var clpfd = new CallableSolverOffline(prologPath, new String[] { ".\\Callables\\constraint_programming.pl" });
                solvers.Add(clpfd);
            }

            Benchmarker.test(solvers.ToArray(), 5, 5);
            //var res = Benchmarker.benchmark(solvers.ToArray(), 2.0);
            //Console.WriteLine(res.ToString());


            // validator test
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

        static void Main(string[] args)
        {
            Test(args);
        }

        static private void testABunch(List<string> problem_filenames, List<string> solution_filenames) {
            for (int i = 0; i < problem_filenames.Count; i++){
                OfflineProblem prob = Parse_problem_offline(problem_filenames[i]);
                HospitalSolution sol = Parse_solution(solution_filenames[i]);
                OfflineValidator val = new OfflineValidator(prob);
                Console.WriteLine($"Validating {problem_filenames[i]} and {solution_filenames[i]}...");
                val.validate(sol);
            }
        }
    }
}
