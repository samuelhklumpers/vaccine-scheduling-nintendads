using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public OfflineProblem problem;

        public OfflineValidator(OfflineProblem problem)
        {
            this.problem = problem;
        }

        public void validate(Solution sol)
        {
            // note that the hospital layout, even if incorrect, isn't really relevant
            assertSameShape(sol);
            assertMachines(sol);
            assertFeasible(sol);
        }

        public void assertSameShape(Solution sol)
        {
            // assert that the solution forgets no patients
            Debug.Assert(this.problem.nPatients == sol.doses.Count);
        }

        public void assertMachines(Solution sol)
        {
            // assert that the solution uses as machines as it claims to
            var r = sol.doses;

            var starts = new int[2 * r.Count];
            var ends = new int[2 * r.Count];

            for (int i = 0; i < r.Count; ++i)
            {
                starts[2 * i] = r[i].t1;
                starts[2 * i + 1] = r[i].t2;
                ends[2 * i] = r[i].t1 + this.problem.p1;
                ends[2 * i + 1] = r[i].t2 + this.problem.p2;
            }

            var starts2 = new List<int>(starts);
            var ends2 = new List<int>(ends);
            starts2.Sort();
            ends2.Sort();

            var s = new LinkedList<int>(starts2);
            var e = new LinkedList<int>(ends2);

            int max = 0;
            int curr = 0;
            while (s.Count > 0)
            {
                var t = s.First.Value;

                while (s.Count > 0 && s.First.Value <= t)
                {
                    s.RemoveFirst();
                    ++curr;
                }

                while (e.Count > 0 && e.First.Value <= t)
                {
                    e.RemoveFirst();
                    --curr;
                }

                if (curr > max)
                {
                    max = curr;
                }
            }

            Debug.Assert(max == sol.machines);
        }

        public void assertFeasible(Solution sol)
        {
            // assert that the solution is feasible
            var p1 = problem.p1;
            var p2 = problem.p2;
            var g = problem.g;

            foreach (var (p, r) in Enumerable.Zip(problem.patients, sol.doses))
            {
                var (r1, d1, x, L, t1, t2) = (p.r1, p.d1, p.x, p.L, r.t1, r.t2);

                Debug.Assert(r1 <= t1);
                Debug.Assert(t1 <= d1 - p1 + 1);

                var r2 = t1 + p1 + x + g;
                Debug.Assert(r2 <= t2);
                Debug.Assert(t2 <= r2 + L - p2 + 1);
            }
        }

        private void appointmentIntervals(Solution2D sol)
        {
            List<Doses2D> regs = sol.hospitals;
            Dictionary<int, List<(int, bool)>> sameHospitals = new Dictionary<int, List<(int, bool)>>();

            // Accumulate all appointments a certain hospital has into dict buckets
            // Iterate over dict buckets. If any appointments overlap, exception                
            foreach (Doses2D reg in regs)
            {
                sameHospitals.TryGetValue(reg.h1, out List<(int, bool)> exists);
                if (exists is null) { sameHospitals[reg.h1] = new List<(int, bool)>(); }

                sameHospitals.TryGetValue(reg.h2, out List<(int, bool)> existsToo);
                if (existsToo is null) { sameHospitals[reg.h2] = new List<(int, bool)>(); }

                sameHospitals[reg.h1].Add((reg.t1, true));
                sameHospitals[reg.h2].Add((reg.t2, false));
            }

            for (int h = 0; h < sol.machines; h++)
            {
                sameHospitals[h].Sort();
            }

            for (int h = 0; h < sol.machines; h++)
            {
                if (sameHospitals[h].Count > 1)
                {
                    for (int i = 0; i < sameHospitals[h].Count - 1; i++)
                    {
                        (int first_start_time, bool first_dose) = sameHospitals[h][i];

                        for (int j = i + 1; j < sameHospitals[h].Count - 1; j++)
                        {
                            (int second_start_time, _) = sameHospitals[h][j];
                            int max = Math.Max(first_start_time, second_start_time);
                            int min = Math.Min(first_start_time, second_start_time);

                            int gap = max - min;
                            if (first_dose && gap < this.problem.p1)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time first dose={this.problem.p1}");
                            }
                            else if (!first_dose && gap < this.problem.p2)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time second dose={this.problem.p2}");
                            }
                        }
                    }
                }
            }
        }

        /*
        public void validate()
        {
            patientNumbers();
            hospitalNumbers();
            jabGaps();
            appointmentIntervals();
        }


        
        private void patientNumbers()
        {
            if (this.solution.withHospital)
            {
                if (this.problem.number_of_patients != this.solution.registrationWithHospitals.Count)
                {
                    throw new Exception($"Solution and problem have differing numbers of patients: #regs={this.solution.registrationWithHospitals.Count}, #patients={this.problem.number_of_patients}.");
                }
            }
            else
            {
                if (this.problem.number_of_patients != this.solution.regs.Count)
                {
                    throw new Exception($"Solution and problem have differing numbers of patients: #regs={this.solution.regs.Count}, #patients={this.problem.number_of_patients}.");
                }
            }
        }

        private void hospitalNumbers()
        {
            if (this.solution.machines > this.problem.number_of_patients)
            {
                throw new Exception($"More hospitals than patients generated: #hopsitals={this.solution.machines}, #patients={this.problem.number_of_patients}.");
            }
        }

        private void jabGaps()
        {
            // Assume soltion and patient data are in the same order. Please note this is not always the case and this probably needs improvement
            // Still, I think this validator is valuable
            if (this.solution.withHospital)
            {
                List<RegistrationWithHospital> regs = this.solution.registrationWithHospitals;
                for (int i = 0; i < regs.Count - 1; i++)
                {
                    int first = regs[i].timeslot_first_dose;
                    int second = regs[i].timeslot_second_dose;
                    int gap = second - first;
                    int min_gap = this.problem.processing_time_first_dose + this.problem.gap + this.problem.patient_data[i].delay_between_doses;
                    if (gap < min_gap)
                    {
                        throw new Exception($"Patient appointment start times planned too close together: first start time = {first}, second start time = {second}, gap = {gap}, min. gap={min_gap}");
                    }

                    if (first + this.problem.processing_time_first_dose - 1 > this.problem.patient_data[i].last_timeslot_first_dose)
                    {
                        throw new Exception($"Patient's first appointment is beyond their feasible first dose interval. start time = {first}, end time = {first + this.problem.processing_time_first_dose - 1} last timeslot first dose = {this.problem.patient_data[i].last_timeslot_first_dose}");
                    }

                    int begin_second = first + this.problem.gap + this.problem.patient_data[i].delay_between_doses + this.problem.processing_time_first_dose;
                    int end_second = begin_second + this.problem.patient_data[i].second_dose_interval;

                    if (second + this.problem.processing_time_second_dose - 1 > end_second)
                    {
                        throw new Exception($"Patient's second appointment is beyond their feasible second dose interval. start time = {second}, end time = {second + this.problem.processing_time_second_dose - 1}, last timeslot second dose = {end_second}");
                    }

                }
            }
            else
            {
                List<Registration> regs = this.solution.regs;
                for (int i = 0; i < regs.Count - 1; i++)
                {
                    int first = regs[i].timeslot_first_dose;
                    int second = regs[i].timeslot_second_dose;
                    int gap = second - first;
                    int min_gap = this.problem.processing_time_first_dose + this.problem.gap + this.problem.patient_data[i].delay_between_doses;
                    if (gap < min_gap)
                    {
                        throw new Exception($"Patient appointment start times planned too close together: first start time = {first}, second start time = {second}, gap = {gap}, min. gap={min_gap}");
                    }

                    if (first + this.problem.processing_time_first_dose - 1 > this.problem.patient_data[i].last_timeslot_first_dose)
                    {
                        throw new Exception($"Patient's first appointment is beyond their feasible first dose interval. start time = {first}, end time = {first + this.problem.processing_time_first_dose - 1} last timeslot first dose = {this.problem.patient_data[i].last_timeslot_first_dose}");
                    }

                    int begin_second = first + this.problem.gap + this.problem.patient_data[i].delay_between_doses + this.problem.processing_time_first_dose;
                    int end_second = begin_second + this.problem.patient_data[i].second_dose_interval;

                    if (second + this.problem.processing_time_second_dose - 1 > end_second)
                    {
                        throw new Exception($"Patient's second appointment is beyond their feasible second dose interval. start time = {second}, end time = {second + this.problem.processing_time_second_dose - 1}, last timeslot second dose = {end_second}");
                    }
                }
            }
        }*/
    }
}
