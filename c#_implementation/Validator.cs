using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public OfflineProblem problem;
        public Solution solution;

        public OfflineValidator(OfflineProblem problem, Solution solution)
        {
            this.problem = problem;
            this.solution = solution;
            // assert each registration is valid
            // assert the number of machines is correct
        }

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

        private void appointmentIntervals()
        {
            if (this.solution.withHospital)
            {
                List<RegistrationWithHospital> regs = this.solution.registrationWithHospitals;
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

                for (int h = 0; h < this.solution.machines; h++)
                {
                    if (sameHospitals[h].Count > 1)
                    {
                        for (int i = 0; i < sameHospitals[h].Count - 1; i++)
                        {
                            (int first_start_time, bool first_dose) = sameHospitals[h][i];
                            (int second_start_time, _) = sameHospitals[h][i + 1];

                            int max = Math.Max(first_start_time, second_start_time);
                            int min = Math.Min(first_start_time, second_start_time);

                            int gap = max - min;
                            if (first_dose && gap < this.problem.processing_time_first_dose)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: patient 1's first start time = {min}, patient 2's first start time = {max}, gap = {gap}, processing time first dose={this.problem.processing_time_first_dose}");
                            }
                            else if (!first_dose && gap < this.problem.processing_time_second_dose)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: patient 1's first start time = {min}, patient 2's first start time = {max}, gap = {gap}, processing time second dose={this.problem.processing_time_first_dose}");
                            }
                        }
                    }
                }
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

                    if (first + this.problem.processing_time_first_dose - 1 > this.problem.patient_data[i].last_timeslot_first_dose) {
                        throw new Exception($"Patient's first appointment is beyond their feasible first dose interval. start time = {first}, end time = {first + this.problem.processing_time_first_dose - 1} last timeslot first dose = {this.problem.patient_data[i].last_timeslot_first_dose}");
                    }

                    int begin_second = first + this.problem.gap + this.problem.patient_data[i].delay_between_doses + this.problem.processing_time_first_dose;
                    int end_second = begin_second + this.problem.patient_data[i].second_dose_interval;

                    if (second + this.problem.processing_time_second_dose - 1 > end_second) {
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

                    if (first + this.problem.processing_time_first_dose - 1 > this.problem.patient_data[i].last_timeslot_first_dose) {
                        throw new Exception($"Patient's first appointment is beyond their feasible first dose interval. start time = {first}, end time = {first + this.problem.processing_time_first_dose - 1} last timeslot first dose = {this.problem.patient_data[i].last_timeslot_first_dose}");
                    }

                    int begin_second = first + this.problem.gap + this.problem.patient_data[i].delay_between_doses + this.problem.processing_time_first_dose;
                    int end_second = begin_second + this.problem.patient_data[i].second_dose_interval;

                    if (second + this.problem.processing_time_second_dose - 1 > end_second) {
                        throw new Exception($"Patient's second appointment is beyond their feasible second dose interval. start time = {second}, end time = {second + this.problem.processing_time_second_dose - 1}, last timeslot second dose = {end_second}");
                    }
                }
            }
        }
    }
}
