using System.Collections.Generic;

namespace implementation
{
    class OnlineProblem : IProblem
    {
        public List<Patient> patient_data;
        public OnlineProblem(int processing_time_first_dose, int processing_time_second_dose, int gap, List<Patient> patient_data)
        {
            this.processing_time_first_dose = processing_time_first_dose;
            this.processing_time_second_dose = processing_time_second_dose;
            this.gap = gap;
            this.patient_data = patient_data;
        }
        public override string ToString()
        {
            string part1 = "processing_time_first_dose: " + this.processing_time_first_dose + "\n";
            string part2 = "processing_time_second_dose: " + this.processing_time_second_dose + "\n";
            string part3 = "gap: " + this.gap + "\n";
            string part4 = "";
            for (int i = 0; i < this.patient_data.Count; i++)
            {
                part4 += this.patient_data[i].ToString() + "\n";
            }
            return part1 + part2 + part3 + part4;
        }
    }
}
