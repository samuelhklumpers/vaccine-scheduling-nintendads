using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class BranchAndBoundSolverOffline : IOfflineSolver
    {

        public Solution solve(OfflineProblem problem)
        {
            //start timelimit high and decrease when branching more --> start with 4 seconds, then 2 en then go to 100 milliseconds or something
            // done to first see if an optimal solution can be found in reasonable time before starting to branch a lot. but with a lot of branching it needs to be faster.
            Dictionary<string, double> partial_solution = new Dictionary<string, double>();
            //partial_solution["t0"] = 4;
            //(bool feasibleNoSolution, bool someSolution, int? upperboundHospitals, Solution? sol) = IntegerLinearProgramming.Solve(problem, partial_solution, 10000);

            Solution sol = LinearProgrammingILP.Solve(problem, partial_solution, 10000);
            return sol;

            /*

            if (sol != null)
            {
                return sol;
            }

            else if (feasibleNoSolution == false && someSolution == false)
            {
                //stop branching as it is infeasible
                Console.WriteLine("infeasible");
                return sol;
            }

            else if (feasibleNoSolution && someSolution == false)
            {
                //do greedy for upperbound cuz no solution was found but it is feasible
                Console.WriteLine("no solution");
                return sol;
            }

            else 
            {
                //branch with upperbound given by upperboundHospitals or with min of upperbound en greedy --> check of greedy beter of niet, solution was found but not an optimal one
                Console.WriteLine("non optimal " );
                Console.WriteLine("upperbound " + upperboundHospitals);

                GreedyOffline greedy = new GreedyOffline();
                Solution greedy_sol = greedy.solve(problem);

                Console.WriteLine("upperbound greedy " + greedy_sol.machines );
                
                return sol;
            }*/
        }

        public Solution solve2(OfflineProblem problem) 
        {
            (int lower, int upper) = calcBounds(problem);
            Stack<Doses2D> regs = new Stack<Doses2D>();
            List<Hospital> hospitals = new List<Hospital>();
            for (int i = 0; i < lower; i++){
                hospitals.Add(new Hospital(hospitals.Count));
            }

            Stack<Patient> patients = deleteMeReverseStack(new Stack<Patient>(problem.patients));

            bool solved = solveR(problem, hospitals, regs, patients, patients.Pop());
            while (!solved)
            {
                hospitals.Add(new Hospital(hospitals.Count));
                if (hospitals.Count > upper)
                {
                    throw new Exception($"More hospitals ({hospitals.Count}) than upper bound ({upper}) generated.");
                }
                solved = solveR(problem, hospitals, regs, patients, patients.Pop());
            }

            return new Solution2D(hospitals.Count, deleteMeReverseStack(regs).ToList());
        }


        private bool solveR(OfflineProblem problem, List<Hospital> hospitals, Stack<Doses2D> regs, Stack<Patient> patients, Patient p)
        {
            // After each successful try of first- and second appointment, recurse to the next patient.
            // If that doesn't work out, recurse back and try another second appointment combination and recurse.
            // If that doesn't work out either, also try another first appointment, try all second appointments and recurse.
            bool solved = false;
            int pmax = Math.Max(problem.p1, problem.p2);
            foreach (int first_start_time in p.start_times_first_dose)
            {
                // Naively set an appointment and fill a changelog of times marked as unavailable by planning this specific appointment.
                // The changelog helps seperating what appointments marked which hours as unavaliable in case of overlap.
                (List<(int, int)> first_changelog, int[] first_appointment) = tryStartTime(hospitals, first_start_time, pmax);
                if (first_appointment is null) { continue; }

                // Calculate the start and end of the second appointment interval by adding the various delays
                int begin_second = first_appointment[0] + problem.g + p.x + problem.p1;
                int end_second = begin_second + p.L;

                // The interval range including the starting hour itself (Enumerable.Range(start,count) will return an empty range if count is 0)
                // The required processing time for the second dose -1 as the starting hour itself is also used
                int interval_range = end_second - begin_second + 1; 
                int processing = problem.p2 - 1; 
                int[] start_times_second_dose = Enumerable.Range(begin_second, interval_range - processing).ToArray();

                foreach (int second_start_time in start_times_second_dose)
                {
                    (List<(int, int)> second_changelog, int[] second_appointment) = tryStartTime(hospitals, second_start_time, pmax);
                    if (second_appointment is null) { continue; }

                    // With a second appointment set, a registration can be generated and added to the list.
                    // Stack used for more efficient adding and removing.
                    regs.Push(new Doses2D(first_appointment[0], first_appointment[1], second_appointment[0], second_appointment[1]));

                    // The success of this appointment depends on the success of subsequent appointments, so recurse if there are any left to plan
                    if (patients.Count > 0) { solved = solveR(problem, hospitals, regs, patients, patients.Pop()); }
                    else { solved = true; }
                    if (!solved)
                    {
                        // Some later appointment interfered with the success of this one
                        // Undo all accounting related to the second appointment, delete the registration, try a different second starting time
                        undoChangelog(hospitals, second_changelog);
                        regs.Pop();
                        continue;
                    }
                    // If successful: recurse all the way to the top to finish up
                    // If failed: backtrack.
                    return solved;
                }
                // If arrived at this point, all second appointment times failed
                // Undo all accounting related to the first appointment and try a different first starting time
                if (!solved) { undoChangelog(hospitals, first_changelog); }
            }
            // This patient failed to be able to be planned in with this schedule with this amount of hospitals.
            // Re-add patient for future evaluation and backtrack. Add hospital if neccesary.
            if (!solved) { patients.Push(p); }
            return solved;
        }
        private (List<(int, int)>, int[]) tryStartTime(List<Hospital> hospitals, int start_time, int processing_time)
        {
            foreach (Hospital h in hospitals)
            {
                // Try this start time in all hospitals.
                // If available, naively plan in and track a changelog of which starting times are designated as busy as a result of this specific appointment.
                h.busy_dict.TryGetValue(start_time, out bool already_busy);
                if (!already_busy)
                {
                    List<(int, int)> changelog = new List<(int, int)>();
                    for (int i = start_time - processing_time + 1; i < start_time + processing_time; i++) 
                    {
                        h.busy_dict.TryGetValue(i, out bool already_true);
                        if (!already_true) 
                        { 
                            h.busy_dict[i] = true;  
                            changelog.Add((i, h.id)); 
                        }
                    }
                    return (changelog, new int[2] { start_time, h.id });
                }
            }
            // All start times failed to be planned in, return a fail state
            return (null, null);
        }

        private void undoChangelog(List<Hospital> hospitals, List<(int, int)> changelog) {
            foreach ((int t, int h) in changelog) {
                hospitals[h].busy_dict[t] = false;
            }
        }

        // Temporary for testing purposes
        private Stack<T> deleteMeReverseStack<T>(Stack<T> input)
        {
            Stack<T> tmp = new Stack<T>();
            while (input.Count > 0)
            {
                tmp.Push(input.Pop());
            }
            return tmp;
        }
        
        private (int, int) calcBounds(OfflineProblem problem)
        {
            // Calculate lower and upper bound of machines with pigeonhole and greedy planning
            int lower = Bounds.PigeonHole(problem);
            GreedyOffline greedy = new GreedyOffline();
            Solution sol = greedy.solve(problem);
            OfflineValidator val = new OfflineValidator(problem);
            val.validate(sol);
            return (lower, sol.machines);
        }
    }
}