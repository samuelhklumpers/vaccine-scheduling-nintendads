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

    class BruteforceSolverOffline : IOfflineSolver
    {
        public Solution solve(OfflineProblem problem)
        {
            //First, dumb bruteforce attempt: Without any heuristics, initiate a hospital and try to fit in patients. If infeasable, retry with another hospital. Repeat until A(!) solution is found
            List<Hospital> hospitals = new List<Hospital>();

            hospitals.Add(new Hospital(hospitals.Count));

            foreach (Patient p in problem.patients)
            {
                // Kies een hospital
                // Neem een starttijd die in start_times_first_dose zit en niet in de times_busy zit van die hospital, herhaal voor elke starttjid
                // als elke starttijd niet mogelijk is kies andere hospital en herhaal hierboven
                // als geen enkel hospital mogelijk creeer nieuw hospital en herhaal alles

                // Zowel wel een tijd gevonden is zet die vast en sla je op welke hospital die patient aangewezen heeft en zet je die tijdspan (begin + processing) als ingenomen in die hospitals times_busy
                // bereken start_times_second_dose van die patient en herhaal het proces hierboven

                // Dit klinkt op zich als online(?)
                bool planned = false;
                while (!planned)
                {
                    planned = tryStartTimes(problem, hospitals, p, p.start_times_first_dose, true);

                    if (planned)
                    {
                        int begin_second = p.temp_first_start_time + problem.g + p.x + problem.p1;
                        int end_second   = p.temp_first_start_time + problem.g + p.x + problem.p1 + p.L - problem.p2;
                        //int second_range = end_second - begin_second + 1; // determine range by end time - begin time + 1 (if same number, 0 + 1 = range of 1 number [10]. if two numbers: 11-10 +1 = range of 2 numbers [10, 11]])
                        //Console.WriteLine($"Range of {begin_second} and {end_second}:");
                        
                        p.start_times_second_dose = Enumerable.Range(begin_second, end_second - begin_second + 1).ToArray();
                        /*foreach (int startTime in p.start_times_second_dose){
                            Console.WriteLine(startTime);
                        }*/
                        
                        planned = tryStartTimes(problem, hospitals, p, p.start_times_second_dose, false);
                        // patient is planned in, continue with next patient
                    }

                    if (!planned) 
                    { 
                        // the first time was planned in but the second time failed. Therefore, backtrack and remove first time from the busytimes
                        //hospitals[p.hospital_first_dose].times_busy.RemoveRange(p.temp_first_start_time - 1, problem.processing_time_first_dose + 1);

                        //rather than mess with unsorted list, simply loop over range and set value to false


                        // Problem with backtracking: not taking note of whether it's planning the second time that's failing or planning a new patient that's failing


                        if (p.temp_first_start_time != 0 && p.temp_second_start_time == 0){
                            for (int i = p.temp_first_start_time - problem.p1; i < p.temp_first_start_time + problem.p1; i++){
                                hospitals[p.h1].busy_dict[i] = false;
                            }
                        }

                        hospitals.Add(new Hospital(hospitals.Count));
                    }
                }

            }

            // pretend it's solved
            List<Doses> regs = new List<Doses>(); //todo with hospital
            foreach (Patient p in problem.patients)
            {
                regs.Add(new Doses(p.temp_first_start_time, p.temp_second_start_time));
            }
            return new Solution(hospitals.Count, regs);

        }

        private bool tryStartTimes(OfflineProblem problem, List<Hospital> hospitals, Patient p, int[] start_times, bool firstDose)
        {
            bool plannedin = false;
            foreach (Hospital h in hospitals)
            {
                foreach (int start_time in start_times)
                {
                    h.busy_dict.TryGetValue(start_time, out bool already_busy);
                    
                    if (!already_busy)
                    {
                        if (firstDose)
                        {
                            p.h1 = h.id;
                            p.temp_first_start_time = start_time;
                            for (int i = start_time - problem.p1; i < start_time + problem.p1; i++){
                                h.busy_dict[i] = true;
                            }
                            //h.times_busy.AddRange(Enumerable.Range(start_time - problem.processing_time_first_dose, start_time + problem.processing_time_first_dose));
                        }
                        else
                        {
                            p.h2 = h.id;
                            p.temp_second_start_time = start_time;
                            for (int i = start_time - problem.p2; i < start_time + problem.p2; i++){
                                h.busy_dict[i] = true;
                            }
                            //h.times_busy.AddRange(Enumerable.Range(start_time - problem.processing_time_first_dose, start_time + problem.processing_time_second_dose));
                        }

                        plannedin = true;
                        break;
                    }
                }
                if (plannedin) break; // if planned, no need to look further
            }
            return plannedin;
        }
    }
}