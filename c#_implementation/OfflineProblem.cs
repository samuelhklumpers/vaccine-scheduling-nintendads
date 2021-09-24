using System.Collections.Generic;

namespace implementation
{
    class IProblem {
        public int processing_time_first_dose;
        public int processing_time_second_dose;
        public int gap;
    }

    class OfflineProblem : IProblem
    {
        public int number_of_patients;
        public List<Patient> patient_data;
        public OfflineProblem(int processing_time_first_dose, int processing_time_second_dose, int gap, int number_of_patients, List<Patient> patient_data)
        {
            this.processing_time_first_dose = processing_time_first_dose;
            this.processing_time_second_dose = processing_time_second_dose;
            this.gap = gap;
            this.number_of_patients = number_of_patients; // why is this not patient_data.Length?
            this.patient_data = patient_data;
        }
        
        public override string ToString()
        {
            string part1 = "processing_time_first_dose: " + this.processing_time_first_dose + "\n";
            string part2 = "processing_time_second_dose: " + this.processing_time_second_dose + "\n";
            string part3 = "gap: " + this.gap + "\n";
            string part4 = "number_of_patients: " + this.number_of_patients + "\n";
            string part5 = "";
            for (int i = 0; i < this.patient_data.Count; i++)
            {
                part5 += this.patient_data[i].ToString() + "\n";
            }
            return part1 + part2 + part3 + part4 + part5;
        }
    }
}
