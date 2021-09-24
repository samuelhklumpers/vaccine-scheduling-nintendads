using System;
using System.Collections.Generic;
using System.Linq;


namespace implementation
{
    class Appointment
    {
        public int timeslot;
        public int index;
        public Registration parent;

        public Appointment(int timeslot, int index, Registration parent)
        {
            this.timeslot = timeslot;
            this.index = index;
            this.parent = parent;
        }

        public static (Appointment, Appointment) Split(Registration r) {
            return (new Appointment(r.timeslot_first_dose, 0, r), new Appointment(r.timeslot_second_dose, 1, r));
        }
    }

    class Solution
    {
        public int machines;
        public List<Registration> regs;

        public Solution(int machines, List<Registration> sol)
        {
            this.machines = machines;
            this.regs = sol;
        }

        public override string ToString()
        {
            string part1 = "machines: " + this.machines + "\n";
            string part2 = "";
            for (int i = 0; i < this.regs.Count; i++)
            {
                part2 += this.regs[i].ToString() + "\n";
            }
            return part1 + part2;
        }

        public HospitalSolution addHospitals(IProblem problem) {
            int[] hospitals = new int[this.machines];

            var regs2 = this.regs.Select<Registration, (Appointment, Appointment)>(Appointment.Split);

            var appointments = regs2.SelectMany<(Appointment, Appointment), Appointment>(x => new Appointment[]{x.Item1, x.Item2});
            appointments = appointments.OrderBy(x => x.timeslot);

            foreach (var app in appointments)
            {

                for (int j = 0; j < hospitals.Count(); ++j) // TODO right now N * H, could probably be N * log(H) if we sort hospitals
                {
                    var t = reg.timeslot_first_dose;

                    if (hospitals[j] < t)
                    {
                        reg2.timeslot_first_dose = t;
                        reg2.hospital_first_dose = j;
                        hospitals[j] = t + problem.processing_time_first_dose;
                    }
                }
            }

            return null;
        }
    }

    class RegistrationWithHospital
    {
        public int timeslot_first_dose;
        public int hospital_first_dose;
        public int timeslot_second_dose;
        public int hospital_second_dose;
        public RegistrationWithHospital(int timeslot_first_dose, int hospital_first_dose, int timeslot_second_dose, int hospital_second_dose)
        {
            this.timeslot_first_dose = timeslot_first_dose;
            this.hospital_first_dose = hospital_first_dose;
            this.timeslot_second_dose = timeslot_second_dose;
            this.hospital_second_dose = hospital_second_dose;
        }
        
        public override string ToString()
        {
            string part1 = "timeslot_first_dose: " + this.timeslot_first_dose + " ";
            string part2 = "hospital_first_dose: " + this.hospital_first_dose + " ";
            string part3 = "timeslot_second_dose: " + this.timeslot_second_dose + " ";
            string part4 = "hospital_second_dose: " + this.hospital_second_dose;
            return part1 + part2 + part3 + part4;
        }
        
        public string ConformToString()
        {
            var tuple = new int[]{this.timeslot_first_dose, this.hospital_first_dose, this.timeslot_second_dose, this.hospital_second_dose};

            return String.Join(", ", tuple.Select<int, string>(x => x.ToString()));
        }
    }

    class HospitalSolution
    {
        public int machines;
        public List<RegistrationWithHospital> sol;

        public HospitalSolution(int machines, List<RegistrationWithHospital> sol)
        {
            this.machines = machines;
            this.sol = sol;
        }

        public override string ToString()
        {
            var ret = String.Join('\n', this.sol.Select<RegistrationWithHospital, string>(x => x.ConformToString()));
            ret += "\n" + this.machines.ToString();
            return ret;
        }
    }
}
