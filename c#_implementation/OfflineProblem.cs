using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Linq;

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
            var ret = String.Join(' ', new int[] { p1, p2, g }.Select<int, string>(x => x.ToString())) + "\n";

            ret += String.Join("\n", patients.Select<Patient, String>(x => x.ToString())) + "\n";

            ret += this.nPatients.ToString() + "\n";

            return ret;
        }

        public OnlineProblem ForgetN()
        {
            return new OnlineProblem(this.p1, this.p2, this.g, this.patients);
        }
    }
}
