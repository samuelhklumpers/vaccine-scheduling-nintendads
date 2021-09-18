using System;
using System.IO;
using static implementation.Parser;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running offline solver");
            OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
            CallableSolverOffline offline_solver = new CallableSolverOffline("C:\\Program Files\\swipl\\bin\\swipl.exe", new String[]{"Callables/constraint_programming.pl"});
            // can't seem to convince C# to start an executable from path
            var solution = offline_solver.solve(offline_problem);
            new OfflineValidator().validate(offline_problem, solution);
            Console.WriteLine("Problem:");
            Console.WriteLine(offline_problem.ToString());
            Console.WriteLine("\nSolution:");
            Console.WriteLine(solution.ToString());

            Console.WriteLine("Running online solver");
            OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
            ExampleSolverOnline online_solver = new ExampleSolverOnline();
            online_solver.solve(online_problem);
        }
    }
}
