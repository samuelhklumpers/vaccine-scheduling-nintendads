using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class Hospital
    {
        public int id;
        public Dictionary<int, bool> busy_dict;

        public Hospital(int id)
        {
            this.id = id;
            this.busy_dict = new Dictionary<int,bool>();
        }
    }

    class RecursiveBruteforce : IOfflineSolver
    {
        public Solution solve(OfflineProblem problem)
        {
            Stack<Doses2D> regs = new Stack<Doses2D>();
            List<Hospital> hospitals = new List<Hospital>();
            hospitals.Add(new Hospital(hospitals.Count));

            Stack<Patient> patients = ReverseStack(new Stack<Patient>(problem.patients));

            bool solved = solveR(problem, hospitals, regs, patients, patients.Pop());
            while (!solved)
            {
                hospitals.Add(new Hospital(hospitals.Count));
                if (hospitals.Count > problem.nPatients)
                {
                    throw new Exception($"More hospitals ({hospitals.Count}) than patients ({problem.nPatients}) generated.");
                }
                solved = solveR(problem, hospitals, regs, patients, patients.Pop());
            }

            return new Solution2D(hospitals.Count, ReverseStack(regs).ToList());
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
        private Stack<T> ReverseStack<T>(Stack<T> input)
        {
            Stack<T> tmp = new Stack<T>();
            while (input.Count > 0)
            {
                tmp.Push(input.Pop());
            }
            return tmp;
        }
    }
}