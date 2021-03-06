using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation 

{
    class BFSBnBOffline : IOfflineSolver 
    {
        
        public Solution solve(OfflineProblem problem)
        {
            if (problem.patients.Count < 1) { return new Solution(0, new List<Doses>()); }
            // Obtain lower and upper bounds with Pigeonhole and Greedy/ILP
            (int lower, int upper, Solution sol) = boundsOrSolve(problem, new Dictionary<string, double>());
            if (sol is not null && sol.machines <= lower) { return sol; }

            // Pre-emptively add lower amount of hospitals
            List<Hospital> hospitals = new List<Hospital>();
            List<Doses2D> regs = new List<Doses2D>();
            for (int i = 0; i < lower; i++)
            { 
                hospitals.Add(new Hospital(hospitals.Count));
            }

            // Sort patients by ascending availability
            List<(int,Patient)> idPatients = new List<(int, Patient)>();
            for (int i = 0; i < problem.patients.Count(); i++)
            {
                idPatients.Add((i,problem.patients[i]));
            }
            idPatients.Sort(ComparePatientAvaliability);
            Queue<Patient> patients = new Queue<Patient>();
            foreach ((int, Patient) ip in idPatients) 
            { 
                patients.Enqueue(ip.Item2); 
            }

            // Create the first PartialSolution and put it in the BFS queue
            // Keep planning partial solutions until a solution is found, branches now manage their own hospital count and bound accordingly
            bool solved = false;
            bool solution_found = false;
            Queue<PartialSolution> partials = new Queue<PartialSolution>();
            PartialSolution ps = new PartialSolution(hospitals, regs, patients);
            PartialSolution best_ps = ps;
            partials.Enqueue(ps);
            while (partials.Count > 0)
            {
                (solved, ps, sol) = BFSolve(problem, partials, lower, upper, solution_found);
                if (solved && sol is null && ps.hospitals.Count() <= upper)  /// <= because of the if (solution_found && ps.hospitals.Count >= upper) { return (false, null, null); } line
                {
                    solution_found = true;
                    best_ps = ps;
                    upper = best_ps.hospitals.Count();
                }
            }

            if (sol is not null) { return sol; }
            else 
            {
                // Results are still ordered by ascending availability, put them back in input order
                List<Doses2D> res = PutBack(idPatients, best_ps.regs);
                return new Solution2D(best_ps.hospitals.Count, res);
            }
        }

        private (bool, PartialSolution, Solution) BFSolve(OfflineProblem problem, Queue<PartialSolution> partials, int lower, int upper, bool solution_found)
        {           
            // Dequeue the latest partial and with it the current patient
            PartialSolution ps = partials.Dequeue();

            // If a solution has been found and this partial solution uses the same or greater amount of hospitals as the found solution, cull it too.
            if (solution_found && ps.hospitals.Count >= upper) { return (false, null, null); }

            // Every partial solution, check if the ILP can find an optimal solution. If so, return it.
            // Otherwise use it to check feasibility. If this partial solution isn't feasible, bound the branch.
            (bool feasible, bool optimalSolution, int? _, Solution sol) = LinearProgrammingILP.Solve(problem, ps.ToILP(), 100); // Tenth of a second
            if (sol is not null && (sol.machines <= lower || optimalSolution)) { return (true, null, sol); }
            else if (!feasible && !optimalSolution) { return (false, null, null); }

            Patient p = ps.patients.Dequeue();

            // Pretend all appointments are the one that takes the longest and keep hospitals empty accordingly
            int pmax = Math.Max(problem.p1, problem.p2);
            
            // If not a single appointment was possible, add a hospital. 
            // So keep track of the partials count at this moment
            int queueLen = partials.Count();
            while (queueLen == partials.Count()) 
            {
                foreach (int first_start_time in p.start_times_first_dose)
                {
                    // Naively set an appointment and fill a changelog of times marked as unavailable by planning this specific appointment.
                    // The changelog helps seperating what appointments marked which hours as unavaliable in case of overlap.
                    (List<(int,int)> first_changelog, int[] first_appointment) = tryStartTime(ps.hospitals, first_start_time, pmax);
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
                         
                        (List<(int, int)> second_changelog, int[] second_appointment) = tryStartTime(ps.hospitals, second_start_time, pmax);
                        if (second_appointment is null) { continue; }

                        // With a second appointment set, a registration can be generated and added to the list.
                        // Deepcopy the current partial solution and plan in this appointment in that copy.
                        PartialSolution copy = ps.deepcopy();
                        copy.regs.Add(new Doses2D(first_appointment[0], first_appointment[1], second_appointment[0], second_appointment[1]));

                        if (ps.patients.Count > 0)  
                        { 
                            // Algorithm isn't done, enqueue the copy.
                            // Undo everything of this layer to clean it for the next deepcopy
                            // But only if no solution already has been found.
                            if (!solution_found) 
                            { 
                                partials.Enqueue(copy); 
                                undoChangelog(ps.hospitals, second_changelog); 
                            }
                            else { return (false, null, null); }
                        }
                        // If all patients are planned in on this branch, return true and disregard the queue because who cares, we found a solution
                        else { return (true, copy, null); }
                    }
                    // Undo everything of this layer to clean it for the next deepcopy
                    undoChangelog(ps.hospitals, first_changelog);
                }
                // If this branch has not capped on hospitals, add a hospital.
                // If this branch fails to plan in the next patient and has capped on hopsitals, bound the branch by not enqueueing any children of it.
                // Additionally, if a solution has already been found, then branches with a hospital count equal to the upper are not interesting either anymore
                if ( queueLen == partials.Count() && ps.hospitals.Count < upper && (!solution_found || ps.hospitals.Count + 1 < upper)) 
                { 
                    ps.hospitals.Add(new Hospital(ps.hospitals.Count)); 
                }
                else { return (false, null, null); }
            }

            return (false, null, null);
        }

        private (int, int) calcBounds(OfflineProblem problem)
        {
            // Lower pigeonhole, for higher literally just run greedy on it and take its machine count
            int lower = Bounds.PigeonHole(problem);
            int upper = new GreedyOffline().solve(problem).machines;
            return (lower, upper);
        }

        private (int, int, Solution) boundsOrSolve(OfflineProblem problem, Dictionary<string, double> partialString)
        {
            // Run the ILP for half a second. (NB: initalisation takes the lion's share of the time)
            // Use the ILP to see if an optimal solution can be found within that time. If yes, steal all credit and return that instead
            // Additionally, run PigeonHole and Greedy for two other easily calculable bounds.
            // If the solution the ILP returns isn't optimal, use Greedy or the ILP's solution to return an upper bound.
            // So long the ILP deems the problem feasible, return the PigeonHole and Greedy bounds. Otherwise, return a failstate.
            (bool feasible, bool optimalSolution, int? upperboundHospitals, Solution sol) = LinearProgrammingILP.Solve(problem, partialString, 500);
            (int lower, int upper) = calcBounds(problem);
            if (sol is not null)
            {
                if (optimalSolution || sol.machines <= upper) { return (sol.machines, sol.machines, sol); }
                else 
                {
                    upper = Math.Min(sol.machines, upper);
                    return (lower, upper, null);
                }

            }
            else if (!feasible && !optimalSolution) { return (0, 0, null); }
            else { return (lower, upper, null); }
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
            foreach ((int t, int h) in changelog) { hospitals[h].busy_dict[t] = false; }
        }

        private static int ComparePatientAvaliability((int,Patient) tuple1, (int,Patient) tuple2)
        {
            // Sort patients from least available to most available
            Patient a = tuple1.Item2;
            Patient b = tuple2.Item2;
            int totalGapA = (a.d1 - a.r1) + a.L;
            int totalGapB = (b.d1 - b.r1) + b.L;
            return totalGapA.CompareTo(totalGapB);
        }

        private static List<Doses2D> PutBack(List<(int, Patient)> idPatients, List<Doses2D> regs)
        {
            // Assume algorithm solves in order of patient input.
            // But in case that patient list is sorted, put the registrations back in original input order
            Doses2D[] res = new Doses2D[idPatients.Count()];
            for (int i = 0; i < idPatients.Count(); i++) 
            {
                res[idPatients[i].Item1] = regs[i];
            }
            return res.ToList();
        }
    }

    class PartialSolution 
    {
        // Store problem state for breadth first search rather than depth first
        public List<Hospital> hospitals;
        public List<Doses2D> regs;
        public Queue<Patient> patients;

        public PartialSolution(List<Hospital> hospitals, List<Doses2D> regs, Queue<Patient> patients)
        {
            this.hospitals = hospitals;
            this.regs = regs;
            this.patients = patients;
        }

        public PartialSolution deepcopy() 
        {
            // Need to return deep copies for the sake of BFS
            return new PartialSolution(this.copyHospitals(this.hospitals), new List<Doses2D>(this.regs), new Queue<Patient>(this.patients));
        }

        private List<Hospital> copyHospitals(List<Hospital> h) 
        {
            // Each dictionary also needs to be a deep copy, otherwise multiple hospitals will point to the same dictionary
            List<Hospital> copy = new List<Hospital>();
            foreach (Hospital hos in h) 
            {
                copy.Add(new Hospital(hos.id) { busy_dict = new Dictionary<int,bool>(hos.busy_dict) });
            }
            return copy;
        }

        public Dictionary<string,double> ToILP()
        {
            Dictionary<string,double> res = new Dictionary<string, double>();
            int t = 0;
            foreach (Doses2D dose in this.regs)
            {
                res["t" + t.ToString()] = (double) dose.t1;
                t++;
                res["t" + t.ToString()] = (double) dose.t2;
                t++;
            }            
            return res;
        }
    }
}    