using System.Collections.Generic;

namespace implementation
{
    class OnlineProblem : IProblem
    {
        public List<Patient> patients;
        public OnlineProblem(int p1, int p2, int g, List<Patient> patients)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.g = g;
            this.patients = patients;
        }
        public override string ToString()
        {
            string part1 = "processing_time_first_dose: " + this.p1 + "\n";
            string part2 = "processing_time_second_dose: " + this.p2 + "\n";
            string part3 = "gap: " + this.g + "\n";
            string part4 = "";
            for (int i = 0; i < this.patients.Count; i++)
            {
                part4 += this.patients[i].ToString() + "\n";
            }
            return part1 + part2 + part3 + part4;
        }
    }
}
