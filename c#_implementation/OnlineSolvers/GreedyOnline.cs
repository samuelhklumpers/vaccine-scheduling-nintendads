using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace implementation
{
    class GreedyOnline : IOnlineSolver
    {
        public int p1;
        public int p2;
        public int g;

        public List<int> overlaps1;
        public List<int> overlaps2;

        // (t, p)
        public List<List<(int, int)>> hospitals;


        public GreedyOnline(/*Parameters parameters*/)
        {
            /*this.p1 = parameters.p1;
            this.p2 = parameters.p2;
            this.g = parameters.g;*/

            this.overlaps1 = new List<int>();
            this.overlaps2 = new List<int>();

            // (t, p)
            this.hospitals = new List<List<(int, int)>>();
        }

        public void Reset()
        {
            this.overlaps1 = new List<int>();
            this.overlaps2 = new List<int>();

            this.hospitals = new List<List<(int, int)>>();
        }

        public Solution2D Step(Solution2D sol, Patient p, Parameters parameters)
        {
            this.p1 = parameters.p1;
            this.p2 = parameters.p2;
            this.g = parameters.g;

             var (r1, d1, x, L) = (p.r1, p.d1, p.x, p.L);

            if (overlaps1.Count < d1 + p1 + x + g + L + p2)
                overlaps1.AddRange(Enumerable.Repeat(0, d1 + p1 + x + g + L + p2 - overlaps1.Count));

            if (overlaps2.Count < d1 + p1 + x + g + L + p2)
                overlaps2.AddRange(Enumerable.Repeat(0, d1 + p1 + x + g + L + p2 - overlaps2.Count));

            var (m, mv) = (sol.doses.Count + 2, (-1, -1));
            for (int i = r1; i <= d1 - p1 + 1; ++i)
            {
                var machines = overlaps1[i];
                var r2 = i + p1 + g + x;
                var range2 = overlaps2.GetRange(r2, L - p2 + 1);

                var (machines2, i2) = range2.Select((x, i) => (x, r2 + i)).Min();

                machines = Math.Max(machines, machines2);

                if (machines < m)
                {
                    m = machines;
                    mv = (i, i2);
                }
            }

            var (t1, t2) = mv;

            Debug.Assert(t1 >= 0 && t2 >= 0);

            for (int i = Math.Max(0, t1 - p1 + 1); i <= t1 + p1 - 1; ++i)
            {
                overlaps1[i]++;
            }

            for (int i = Math.Max(0, t2 - p1 + 1); i <= t2 + p2 - 1; ++i)
            {
                overlaps1[i]++;
            }

            for (int i = Math.Max(0, t2 - p2 + 1); i <= t2 + p2 - 1; ++i)
            {
                overlaps2[i]++;
            }

            for (int i = Math.Max(0, t1 - p2 + 1); i <= t1 + p1 - 1; ++i)
            {
                overlaps2[i]++;
            }

            var (h1, h2) = (-1, -1);
            for (int h = 0; h < hospitals.Count; ++h)
            {
                if (hospitals[h].All(d => d.Item1 + d.Item2 - 1 < t1 || t1 + p1 - 1 < d.Item1))
                {
                    h1 = h;
                    hospitals[h1].Add((t1, p1));
                    break;
                }
            }

            if (h1 == -1)
            {
                h1 = hospitals.Count;
                hospitals.Add(new List<(int, int)>());

                hospitals[h1].Add((t1, p1));
            }

            for (int h = 0; h < hospitals.Count; ++h)
            {
                if (hospitals[h].All(d => d.Item1 + d.Item2 - 1 < t2 || t2 + p2 - 1 < d.Item1))
                {
                    h2 = h;
                    hospitals[h2].Add((t2, p2));
                    break;
                }
            }

            if (h2 == -1)
            {
                h2 = hospitals.Count;
                hospitals.Add(new List<(int, int)>());

                hospitals[h2].Add((t2, p2));
            }

            sol.hospitals.Add(new Doses2D(t1, h1, t2, h2));
            sol.doses.Add(new Doses(t1, t2));
            sol.machines = this.hospitals.Count();

            return sol;
        }
    }
}