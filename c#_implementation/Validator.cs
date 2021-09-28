using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public OfflineProblem problem;
        public Solution solution;

        public OfflineValidator(OfflineProblem problem, Solution solution) {
            this.problem = problem;
            this.solution = solution;
            // assert each registration is valid
            // assert the number of machines is correct
        }

        public void validate(){
            patientNumbers();
            hospitalNumbers();
            jabGaps();
        }


        private void patientNumbers() {
            if (this.solution.withHospital)
            {
                if (this.problem.number_of_patients != this.solution.registrationWithHospitals.Count) {
                    throw new Exception($"Solution and problem have differing numbers of patients: #regs={this.solution.registrationWithHospitals.Count}, #patients={this.problem.number_of_patients}.");
                }
            }
            else {
                if (this.problem.number_of_patients != this.solution.regs.Count) {
                    throw new Exception($"Solution and problem have differing numbers of patients: #regs={this.solution.regs.Count}, #patients={this.problem.number_of_patients}.");
                }
            }
        }

        private void hospitalNumbers() {
            if (this.solution.machines > this.problem.number_of_patients) {
                throw new Exception($"More hospitals than patients generated: #hopsitals={this.solution.machines}, #patients={this.problem.number_of_patients}.");
            }
        }

        private void jabGaps(){
            // Assume soltion and patient data are in the same order. Please note this is not always the case and this probably needs improvement
            // Still, I think this validator is valuable
            if (this.solution.withHospital){
                List<RegistrationWithHospital> regs = this.solution.registrationWithHospitals;
                for (int i = 0; i < regs.Count - 1; i++) {
                    int first = regs[i].timeslot_first_dose;
                    int second = regs[i].timeslot_second_dose;
                    int gap = second - first;
                    int min_gap = this.problem.processing_time_first_dose + this.problem.gap + this.problem.patient_data[i].delay_between_doses;
                    if (gap < min_gap) {
                        throw new Exception($"Patient appointment start times planned too close together: first start time = {first}, second start time = {second}, gap = {gap}, min. gap={min_gap}");
                    }
                }
            }
            else {
                List<Registration> regs = this.solution.regs;
                for (int i = 0; i < regs.Count - 1; i++) {
                    int first = regs[i].timeslot_first_dose;
                    int second = regs[i].timeslot_second_dose;
                    int gap = second - first;
                    int min_gap = this.problem.processing_time_first_dose + this.problem.gap + this.problem.patient_data[i].delay_between_doses;
                    if (gap < min_gap) {
                        throw new Exception($"Patient appointment start times planned too close together: first start time = {first}, second start time = {second}, gap = {gap}, min. gap={min_gap}");
                    }
                }
            }
        }

    }
}
