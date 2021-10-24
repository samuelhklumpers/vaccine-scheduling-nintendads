using System;
using System.Collections.Generic;
using System.Linq;

namespace implementation
{
    public class Parameters
    {
        public int p1;
        public int p2;
        public int g;
    }

    public class OnlineProblem : IProblem
    {

        public List<Patient> patients;
        public Parameters parameters;

        public OnlineProblem(int p1, int p2, int g, List<Patient> patients)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.g = g;
            this.patients = patients;

            this.parameters = new Parameters() { p1 = p1, p2 = p2, g = g };
        }

        public override string ToString()
        {
            var ret = String.Join(' ', new int[] {p1, p2, g}.Select<int, string>(x => x.ToString())) + "\n";

            ret += String.Join("\n", patients.Select<Patient, String>(x => x.ToString())) + "\n";

            return ret;
        }

        public OfflineProblem CountN()
        {
            return new OfflineProblem(this.p1, this.p2, this.g, this.patients.Count, this.patients);
        }
    }
}
