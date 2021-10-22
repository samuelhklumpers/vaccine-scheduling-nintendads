using System;
using System.Linq;

namespace implementation
{
    public class Patient
    {
        public int r1;
        public int d1;
        public int x;
        public int L;
        public int[] start_times_first_dose;
        public int[] start_times_second_dose;
        public int h1;
        public int h2;
        public int temp_first_start_time;
        public int temp_second_start_time;
        public Patient(int r1, int d1, int x, int L, int p1, int p2, int g)
        {
            this.r1 = r1;
            this.d1 = d1;
            this.x = x;
            this.L = L;

            // Remove processing time from the range making it a list of viable start times for that patient, rather than the full viable interval
            // The interval range including the starting hour itself (Enumerable.Range(start,count) will return an empty range if count is 0)
            // The required processing time for the second dose -1 as the starting hour itself is also used
            int interval_range = d1 - r1 + 1;
            int processing = p1 - 1;
            this.start_times_first_dose = Enumerable.Range(r1, interval_range - processing).ToArray();
        }

        public override string ToString()
        {
            return String.Join(' ', new int[] { r1, d1, x, L });
        }
    }
}
