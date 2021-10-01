using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public OfflineProblem problem;
        public Solution solution;

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

        public void assertSameShape(Solution sol) {
            // assert that the solution forgets no patients
            Debug.Assert(this.problem.number_of_patients == sol.regs.Count);
        }

        public void assertMachines(Solution sol)
        {
            // assert that the solution uses as machines as it claims to
            var r = sol.regs;

            var starts = new int[2 * r.Count];
            var ends = new int[2 * r.Count];

            for (int i = 0; i < r.Count; ++i)
            {
                starts[2 * i] = r[i].timeslot_first_dose;
                starts[2 * i + 1] = r[i].timeslot_second_dose;
                ends[2 * i] = r[i].timeslot_first_dose + this.problem.processing_time_first_dose;
                ends[2 * i + 1] = r[i].timeslot_second_dose + this.problem.processing_time_second_dose;
            }

            var s2 = new List<int>(starts);
            var e2 = new List<int>(ends);
            s2.Sort();
            e2.Sort();

            var s = new LinkedList<int>(s2);
            var e = new LinkedList<int>(e2);

            int m = 0;
            int c = 0;
            while (s.Count > 0)
            {
                var t = s.First.Value;

                while (s.Count > 0 && s.First.Value <= t)
                {
                    s.RemoveFirst();
                    ++c;
                }

                while (e.Count > 0 && e.First.Value <= t)
                {
                    e.RemoveFirst();
                    --c;
                }

                if (c > m) {
                    m = c;
                }
            }

            Debug.Assert(m == sol.machines);
        }

        public void assertFeasible(Solution sol) {
            // assert that the solution is feasible
            var p1 = problem.processing_time_first_dose;
            var p2 = problem.processing_time_second_dose;
            var g = problem.gap;

            foreach (var (p, r) in Enumerable.Zip(problem.patient_data, sol.regs))
            {
                var r1 = p.first_timeslot_first_dose;
                var d1 = p.last_timeslot_first_dose;
                var x = p.delay_between_doses;
                var L = p.second_dose_interval;

                var t1 = r.timeslot_first_dose;
                var t2 = r.timeslot_second_dose;

                Debug.Assert(r1 <= t1);
                Debug.Assert(t1 <= d1 - p1 + 1);
                
                var r2 = t1 + p1 + x + g;
                Debug.Assert(r2 <= t2);
                Debug.Assert(t2 <= r2 + L - p2 + 1);
            }
        }

        private void appointmentIntervals(HospitalSolution sol)
        {
            List<RegistrationWithHospital> regs = sol.hospitals;
            Dictionary<int, List<(int, bool)>> sameHospitals = new Dictionary<int, List<(int, bool)>>();

            // Accumulate all appointments a certain hospital has into dict buckets
            // Iterate over dict buckets. If any appointments overlap, exception                
            foreach (RegistrationWithHospital reg in regs)
            {
                sameHospitals.TryGetValue(reg.hospital_first_dose, out List<(int, bool)> exists);
                if (exists is null) { sameHospitals[reg.hospital_first_dose] = new List<(int, bool)>(); }

                sameHospitals.TryGetValue(reg.hospital_second_dose, out List<(int, bool)> existsToo);
                if (existsToo is null) { sameHospitals[reg.hospital_second_dose] = new List<(int, bool)>(); }

                sameHospitals[reg.hospital_first_dose].Add((reg.timeslot_first_dose, true));
                sameHospitals[reg.hospital_second_dose].Add((reg.timeslot_second_dose, false));
            }

            for (int h = 0; h < this.solution.machines; h++){
                sameHospitals[h].Sort();
            }

            for (int h = 0; h < this.solution.machines; h++)
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
                            if (first_dose && gap < this.problem.processing_time_first_dose)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time first dose={this.problem.processing_time_first_dose}");
                            }
                            else if (!first_dose && gap < this.problem.processing_time_second_dose)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time second dose={this.problem.processing_time_second_dose}");
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
