using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class Hospital
    {
        public int id;
        public List<int> times_busy;

        public Hospital(int id)
        {
            this.id = id;
            this.times_busy = new List<int>();
        }
    }

    class BruteforceSolverOffline : ISolverOffline
    {
        public Solution solve(OfflineProblem problem)
        {
            //First, dumb bruteforce attempt: Without any heuristics, initiate a hospital and try to fit in patients. If infeasable, retry with another hospital. Repeat until A(!) solution is found
            bool solved = false;
            List<Hospital> hospitals = new List<Hospital>();

            hospitals.Add(new Hospital(hospitals.Count + 1));

            foreach (Patient p in problem.patient_data)
            {
                // Kies een hospital
                // Neem een starttijd die in start_times_first_dose zit en niet in de times_busy zit van die hospital, herhaal voor elke starttjid
                // als elke starttijd niet mogelijk is kies andere hospital en herhaal hierboven
                // als geen enkel hospital mogelijk creëer nieuw hospital en herhaal alles
                // als geen enkel hospital mogelijk creëer nieuw hospital en herhaal alles

                // Zowel wel een tijd gevonden is zet die vast en sla je op welke hospital die patient aangewezen heeft en zet je die tijdspan (begin + processing) als ingenomen in die hospitals times_busy
                // bereken start_times_second_dose van die patient en herhaal het proces hierboven

                // Dit klinkt op zich als online(?)
                bool planned = false;
                while (!planned)
                {
                    planned = tryStartTimes(hospitals, p, p.start_times_first_dose, false, true);

                    if (planned)
                    {
                        p.start_times_second_dose = Enumerable.Range(p.temp_first_start_time + problem.gap + p.delay_between_doses + problem.processing_time_first_dose,
                                                                     p.temp_first_start_time + problem.gap + p.delay_between_doses + problem.processing_time_first_dose + p.second_dose_interval - problem.processing_time_second_dose).ToArray();
                        planned = tryStartTimes(hospitals, p, p.start_times_second_dose, false, false);
                        // patient is planned in, continue with next patient
                    }

                    if (!planned) { hospitals.Add(new Hospital(hospitals.Count + 1)); }
                }

            }

            // pretend it's solved
            List<Registration> regs = new List<Registration>(); //todo with hospital
            foreach (Patient p in problem.patient_data)
            {
                regs.Add(new Registration(p.temp_first_start_time, p.temp_second_start_time));
            }
            return new Solution(hospitals.Count, regs);

        }

        private bool tryStartTimes(List<Hospital> hospitals, Patient p, int[] start_times, bool planned, bool firstDose)
        {
            foreach (Hospital h in hospitals)
            {
                foreach (int start_time in start_times)
                {
                    if (!h.times_busy.Contains(start_time))
                    {
                        if (firstDose)
                        {
                            p.hospital_first_dose = h.id;
                            p.temp_first_start_time = start_time;
                            h.times_busy.AddRange(Enumerable.Range(start_time, start_time + problem.processing_time_first_dose));
                        }
                        else
                        {
                            p.hospital_second_dose = h.id;
                            p.temp_second_start_time = start_time;
                            h.times_busy(AddRange(Enumerable.Range(start_time, start_time + problem.processing_time_second_dose)));
                        }

                        planned = true;
                        break;
                    }
                }
                if (planned) break; // if planned, no need to look further
            }
            return planned;
        }
    }
}