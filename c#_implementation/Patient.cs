using System;
using System.Linq;

namespace implementation
{
    public class Patient
    {
        public int first_timeslot_first_dose;
        public int last_timeslot_first_dose;
        public int delay_between_doses;
        public int second_dose_interval;
        public int[] start_times_first_dose;
        public int[] start_times_second_dose;
        public int hospital_first_dose;
        public int hospital_second_dose;
        public int temp_first_start_time;
        public int temp_second_start_time;
        public Patient(int first_timeslot_first_dose, int last_timeslot_first_dose, int delay_between_doses, int second_dose_interval, int processing_time_first_dose, int processing_time_second_dose, int gap)
        {
            this.first_timeslot_first_dose = first_timeslot_first_dose;
            this.last_timeslot_first_dose = last_timeslot_first_dose;
            this.delay_between_doses = delay_between_doses;
            this.second_dose_interval = second_dose_interval;

            // Remove processing time from the range making it a list of viable start times for that patient, rather than the full viable interval
            // The interval range including the starting hour itself (Enumerable.Range(start,count) will return an empty range if count is 0)
            // The required processing time for the second dose -1 as the starting hour itself is also used
            int interval_range = last_timeslot_first_dose - first_timeslot_first_dose + 1;
            int processing = processing_time_first_dose - 1;
            this.start_times_first_dose = Enumerable.Range(first_timeslot_first_dose, interval_range - processing).ToArray();
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
}
