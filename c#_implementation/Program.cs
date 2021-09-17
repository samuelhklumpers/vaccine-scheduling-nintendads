using System;
using static implementation.Parser;

namespace implementation
{
    class Program
    {
        static void Main(string[] args)
        {
            OfflineProblem offline_problem = Parse_problem_offline("../data/offline/from_assignment.txt");
            ExampleSolverOffline offline_solver = new ExampleSolverOffline();
            offline_solver.solve(offline_problem);



            OnlineProblem online_problem = Parse_problem_online("../data/online/from_assignment.txt");
            ExampleSolverOnline online_solver = new ExampleSolverOnline();
            online_solver.solve(online_problem);
        }
    }
}
