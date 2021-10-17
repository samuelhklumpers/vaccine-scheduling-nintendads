using System.Diagnostics;
using System.Collections.Generic;

namespace implementation
{
    public class IProblem {
        public int p1;
        public int p2;
        public int g;
    }

    public class OfflineProblem : IProblem
    {
        public int nPatients;
        public List<Patient> patients;
        public OfflineProblem(int p1, int p2, int g, int n, List<Patient> patients)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.g = g;
            this.nPatients = n; Debug.Assert(patients.Count == n); // why is this not just patients.Count?
            this.patients = patients;
        }
        
        public override string ToString()
        {
            string part1 = "processing_time_first_dose: " + this.p1 + "\n";
            string part2 = "processing_time_second_dose: " + this.p2 + "\n";
            string part3 = "gap: " + this.g + "\n";
            string part4 = "number_of_patients: " + this.nPatients + "\n";
            string part5 = "";
            for (int i = 0; i < this.patients.Count; i++)
            {
                part5 += this.patients[i].ToString() + "\n";
            }
            return part1 + part2 + part3 + part4 + part5;
        }

        public OnlineProblem ForgetN()
        {
            return new OnlineProblem(this.p1, this.p2, this.g, this.patients);
        }
    }
}
