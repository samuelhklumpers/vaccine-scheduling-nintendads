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
        public int hospital;

        public Appointment(int timeslot, int hospital)
        {
            this.index = 0;
            this.parent = null;
            this.timeslot = timeslot;
            this.hospital = hospital;
        }

        public static (Appointment, Appointment) Split(Registration r)
        {
            return (new Appointment(r.timeslot_first_dose, 0), new Appointment(r.timeslot_second_dose, 0));
        }

        public static RegistrationWithHospital WithHospital((Appointment, Appointment) apps)
        {
            var a = apps.Item1;
            var b = apps.Item2;

            return new RegistrationWithHospital(a.timeslot, a.hospital, b.timeslot, b.hospital);
        }
    }

    class Solution
    {
        public int machines;
        public List<Registration> regs;
        public List<RegistrationWithHospital> registrationWithHospitals;
        public bool withHospital;

        public Solution(int machines, List<Registration> sol)
        {
            this.machines = machines;
            this.regs = sol;
            this.withHospital = false;
        }

        public Solution(int machines, List<RegistrationWithHospital> sol) // Hacky override because I don't understand how this new code fits together
        {
            this.machines = machines;
            this.registrationWithHospitals = sol;
            this.withHospital = true;
        }

        public override string ToString()
        {
            string part1 = "machines: " + this.machines + "\n";
            string part2 = "";

            if (withHospital)
            {
                for (int i = 0; i < this.registrationWithHospitals.Count; i++)
                {
                    part2 += this.registrationWithHospitals[i].ToString() + "\n";
                }
            }
            else
            {
                for (int i = 0; i < this.regs.Count; i++)
                {
                    part2 += this.regs[i].ToString() + "\n";
                }
            }
            return part1 + part2;
        }

        public HospitalSolution AddHospitals(IProblem problem)
        {
            int[] hospitals = new int[this.machines];

            var regs2 = this.regs.Select<Registration, (Appointment, Appointment)>(Appointment.Split);

            var appointments = regs2.SelectMany<(Appointment, Appointment), Appointment>(x => new Appointment[] { x.Item1, x.Item2 });
            appointments = appointments.OrderBy(x => x.timeslot);

            foreach (var app in appointments)
            {
                for (int j = 0; j < hospitals.Count(); ++j) // TODO right now N * H, could probably be N * log(H) if we sort hospitals
                {
                    if (hospitals[j] < app.timeslot)
                    {
                        app.hospital = j;
                        hospitals[j] = app.timeslot + problem.processing_time_first_dose;
                    }
                }
            }

            var regs3 = regs2.Select<(Appointment, Appointment), RegistrationWithHospital>(Appointment.WithHospital);

            return new HospitalSolution(this.machines, regs3.ToList());
        }
    }

    class Registration
    {
        public int timeslot_first_dose;
        public int timeslot_second_dose;
        public Registration(int timeslot_first_dose, int timeslot_second_dose)
        {
            this.timeslot_first_dose = timeslot_first_dose;
            this.timeslot_second_dose = timeslot_second_dose;
        }
        public override string ToString()
        {
            string part1 = "timeslot_first_dose: " + this.timeslot_first_dose + " ";
            string part2 = "timeslot_second_dose: " + this.timeslot_second_dose;
            return part1 + part2;
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
            var tuple = new int[] { this.timeslot_first_dose, this.hospital_first_dose, this.timeslot_second_dose, this.hospital_second_dose };

            return String.Join(", ", tuple.Select<int, string>(x => x.ToString()));
        }
    }

    class HospitalSolution : Solution
    {
        public List<RegistrationWithHospital> hospitals;

        public HospitalSolution(int machines, List<RegistrationWithHospital> hospitals) : base(machines, hospitals)
        {
            this.hospitals = hospitals;
            // could fix this.sol
        }

        public override string ToString()
        {
            var ret = String.Join('\n', this.hospitals.Select<RegistrationWithHospital, string>(x => x.ConformToString()));
            ret += "\n" + this.machines.ToString();
            return ret;
        }
    }
}
