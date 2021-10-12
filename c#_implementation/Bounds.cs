using System.Collections.Generic;
using System.Linq;
using System;

namespace implementation
{
    class Bounds
    {
        // calculate pigeonhole on weak second timeslot bounds
        static public int PigeonHole(OfflineProblem p) {
            List<(int, int, int)> intervals = new List<(int, int, int)>();

            var (p1, p2, g) = (p.p1, p.p2, p.g);
            var ps = p.patients;
            var n = ps.Count;
            
            for (int i = 0; i < n; ++i)
            {
                var pat = ps[i];

                var (left1, right1) = (pat.r1, pat.d1 - p1 + 1);
                var (left2, right2) = (left1 + p1 + g + pat.x, right1 + p1 + g + pat.x + pat.L - p2 + 1);

                intervals.Add((left1, p1, right1));
                intervals.Add((left2, p2, right2));
            }

            (int, int, int)[] jobs = intervals.ToArray();
            jobs = jobs.OrderBy(x => x.Item1).ToArray();    

            int highest = 0;
            for (int i = 0; i < jobs.Count(); ++i)
            {
                var (t11, dt1, t12) = jobs[i];
                double s = dt1;

                highest = Math.Max(highest, 1);

                for (int j = 1; j < jobs.Count() - i; ++j)
                {
                    var (t21, dt2, t22) = jobs[j];

                    t12 = Math.Max(t12, t22);
                    s += dt2;
                    
                    int highest2 = (int)Math.Ceiling(s / (t12 - t11));
                    highest = Math.Max(highest, highest2);
                }
            }
            
            return highest;
        }
    }
}