namespace implementation
{
    class Patient
    {
        public int first_timeslot_first_dose;
        public int last_timeslot_first_dose;
        public int delay_between_doses;
        public int second_dose_interval;
        public int[] start_times_first_dose;
        public int[] start_times_second_dose;
        public int hospital_first_dose;
        public int hospital_second_dose; // to be filled in by planner
        public int temp_first_start_time; // subject to change if backtracking
        public int temp_second_start_time; // subject to change if backtracking
        public Patient(int first_timeslot_first_dose, int last_timeslot_first_dose, int delay_between_doses, int second_dose_interval, int processing_time_first_dose, int processing_time_second_dose, int gap)
        {
            this.first_timeslot_first_dose = first_timeslot_first_dose;
            this.last_timeslot_first_dose = last_timeslot_first_dose;
            this.delay_between_doses = delay_between_doses;
            this.second_dose_interval = second_dose_interval;

            // Haal de lengte van de gehele prikprodecude van (beide) intervals af per patient zodat je alleen maar naar de starttijden hoeft te kijken voor planning
            this.start_times_first_dose = Enumerable.Range(first_timeslot_first_dose, last_timeslot_first_dose - processing_time_first_dose).ToArray();
            // this.start_times_second_dose filled during planning
        }
        public override string ToString()
        {
            string part1 = "first_timeslot_first_dose: " + this.first_timeslot_first_dose + " ";
            string part2 = "last_timeslot_first_dose: " + this.last_timeslot_first_dose + " ";
            string part3 = "delay_between_doses: " + this.delay_between_doses + " ";
            string part4 = "second_dose_interval: " + this.second_dose_interval;
            return part1 + part2 + part3 + part4;
        }
    }

    class Registration
    {
        int timeslot_first_dose;
        int timeslot_second_dose;
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
        int timeslot_first_dose;
        int hospital_first_dose;
        int timeslot_second_dose;
        int hospital_second_dose;
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
    }
}
