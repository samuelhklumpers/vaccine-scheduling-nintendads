using System;
using System.IO;
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

            bool test = false;
            bool benchmark = false;
            bool validate = true;


            OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
            if (test)
            {
                Console.WriteLine("Running offline solver");
                //OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
                CallableSolverOffline offline_solver = new CallableSolverOffline("C:\\Program Files\\swipl\\bin\\swipl.exe", new String[] { "constraint_programming.pl" });

                // can't seem to convince C# to start an executable from path
                var solution = offline_solver.solve(offline_problem);
                new OfflineValidator(offline_problem, solution).validate();
                Console.WriteLine("Problem:");
                Console.WriteLine(offline_problem.ToString());
                Console.WriteLine("\nSolution:");
                Console.WriteLine(solution.ToString());

                //Console.WriteLine("Running online solver");
                //OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
                //ExampleSolverOnline online_solver = new ExampleSolverOnline();
                //online_solver.solve(online_problem);
            }

            if (benchmark)
            {
                var solver = new CallableSolverOffline("C:\\Program Files\\swipl\\bin\\swipl.exe", new String[] { "constraint_programming.pl" });
                ISolverOffline[] solvers = new ISolverOffline[] { solver };

                var bench = Benchmarker.benchmark(solvers, 5.0);

                Console.WriteLine(bench.ToString());
            }

            if (validate)
            {
                List<string> problems = new List<string>
                {
                    "../data/offline/backtracker.txt",
                    "../data/offline/big_numbers.txt",
                    "../data/online/Problem1.txt",
                    "../data/online/Problem2.txt"
                };

                List<string> solutions = new List<string> 
                {
                    "../data/solutions/offline/backtracker.txt",
                    "../data/solutions/offline/big_numbers.txt",
                    "../data/solutions/online/Solution1.txt",
                    "../data/solutions/online/Solution2.txt"
                };
                testABunch(problems, solutions);
            }
        }

        static private void testABunch(List<string> problem_filenames, List<string> solution_filenames) {
            for (int i = 0; i < problem_filenames.Count; i++){
                OfflineProblem prob = Parse_problem_offline(problem_filenames[i]);
                HospitalSolution sol = Parse_solution(solution_filenames[i]);
                OfflineValidator val = new OfflineValidator(prob,sol);
                Console.WriteLine($"Validating {problem_filenames[i]} and {solution_filenames[i]}...");
                val.validate();
            }
        }
    }
}
