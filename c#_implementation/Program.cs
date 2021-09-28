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
            HospitalSolution testSolution = Parse_solution("../data/big_numbers.txt");
            Console.WriteLine(testSolution.ToString());

            RecursiveBruteforce brute = new RecursiveBruteforce();
            Solution solution = brute.solve(offline_problem);
            OfflineValidator validator = new OfflineValidator(offline_problem, solution);
            validator.validate();

            Console.WriteLine(solution);

            OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
            ExampleSolverOnline online_solver = new ExampleSolverOnline();
            online_solver.solve(online_problem);
        }
    }
}
