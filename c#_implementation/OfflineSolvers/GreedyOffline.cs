using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class GreedyOffline : IOfflineSolver
    {
        public Solution solve(OfflineProblem problem)
        {
            List<Doses2D> regs = new List<Doses2D>();
            List<Hospital> hospitals = new List<Hospital>();
            hospitals.Add(new Hospital(hospitals.Count));
            
            foreach (Patient p in problem.patients)
            {
                bool planned = false;
                while (!planned) 
                {
                    planned = solveR(problem, hospitals, regs, p);
                    if (!planned) { hospitals.Add(new Hospital(hospitals.Count)); }
                }
            }

            return new Solution2D(hospitals.Count, regs);
        }
        private bool solveR(OfflineProblem problem, List<Hospital> hospitals, List<Doses2D> regs, Patient p)
        {
            // After each successful try of first- and second appointment, recurse to the next patient.
            // If that doesn't work out, recurse back and try another second appointment combination and recurse.
            // If that doesn't work out either, also try another first appointment, try all second appointments and recurse.
            int pmax = Math.Max(problem.p1, problem.p2);
            foreach (int first_start_time in p.start_times_first_dose)
            {
                // Naively set an appointment and fill a changelog of times marked as unavailable by planning this specific appointment.
                // The changelog helps seperating what appointments marked which hours as unavaliable in case of overlap.
                int[] first_appointment = tryStartTime(hospitals, first_start_time, pmax);
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
                    int[] second_appointment = tryStartTime(hospitals, second_start_time, pmax);
                    if (second_appointment is null) { continue; }
                    else 
                    {
                        regs.Add(new Doses2D(first_appointment[0], first_appointment[1], second_appointment[0], second_appointment[1]));
                        return true;
                    }
                }
                // If arrived at this point, all second appointment times failed
                return false;
            }
            // This patient failed to be able to be planned in with this schedule with this amount of hospitals. Add hospital.
            return false;
        }
        private int[] tryStartTime(List<Hospital> hospitals, int start_time, int processing_time)
        {
            foreach (Hospital h in hospitals)
            {
                // Try this start time in all hospitals.
                h.busy_dict.TryGetValue(start_time, out bool already_busy);
                if (!already_busy)
                {
                    for (int i = start_time - processing_time + 1; i < start_time + processing_time; i++) 
                    {
                        h.busy_dict.TryGetValue(i, out bool already_true);
                        if (!already_true) { h.busy_dict[i] = true; }
                    }
                    return new int[2] { start_time, h.id };
                }
            }
            // All start times failed to be planned in, return a fail state
            return null;
        }
    }
}