using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace implementation
{
    class VeryGreedyOnline : IOnlineSolver
    {
        public int p1;
        public int p2;
        public int g;

        public List<int> machines;
        public Dictionary<int, int> histogram;

        // (t, p)
        public List<List<(int, int)>> hospitals;


        public VeryGreedyOnline()
        {
            this.machines = new List<int>();
            this.histogram = new Dictionary<int, int>();

            // (t, p)
            this.hospitals = new List<List<(int, int)>>();
        }

        public void Reset()
        {
            this.machines = new List<int>();

            this.histogram = new Dictionary<int, int>();
            this.hospitals = new List<List<(int, int)>>();
        }

        public Solution2D Step(Solution2D sol, Patient p, Parameters parameters)
        {
            void EnsureSafe(int d1, int x, int L)
            {
                if (machines.Count < d1 + p1 + x + g + L + p2)
                    machines.AddRange(Enumerable.Repeat(0, d1 + p1 + x + g + L + p2 - machines.Count));
            }

            Dictionary<int, int> Histogram(int i, int j, int p1, int p2)
            {
                var hist = new Dictionary<int, int>(this.histogram);

                for (int k = 0; k < p1; ++k)
                {
                    int m = this.machines[i + k];

                    if (m > 0)
                        hist[m]--;

                    hist[m + 1] = 1 + hist.GetValueOrDefault(m + 1, 0);
                }

                for (int k = 0; k < p2; ++k)
                {
                    int m = this.machines[j + k];

                    if (m > 0)
                        hist[m]--;

                    hist[m + 1] = 1 + hist.GetValueOrDefault(m + 1, 0);
                }

                return hist;
            }

            bool Lexicographical(Dictionary<int, int> a, Dictionary<int, int> b)
            {
                if (b == null)
                {
                    return true;
                }

                var keys = new HashSet<int>(a.Keys);
                keys.UnionWith(b.Keys);

                var keys2 = keys.ToList<int>();
                keys2.Sort((i, j) => j - i);

                foreach (int k in keys2)
                {
                    var (ak, bk) = (a.GetValueOrDefault(k, 0), b.GetValueOrDefault(k, 0));

                    if (ak > 0 && ak >= bk)
                    {
                        return false;
                    }

                    if (bk > 0 && ak < bk)
                    {
                        return true;
                    }
                }

                return false; // only reached when a and b are both only zeros
            }

            this.p1 = parameters.p1;
            this.p2 = parameters.p2;
            this.g = parameters.g;

            var (r1, d1, x, L) = (p.r1, p.d1, p.x, p.L);
            EnsureSafe(d1, x, L);


            Dictionary<int, int> m = null;
            var mv = (-1, -1);
            for (int i = r1; i <= d1 - p1 + 1; ++i)
            {
                var r2 = i + p1 + g + x;
                var d2 = r2 + L - p2 + 1;

                for (int j = r2; j <= d2; ++j)
                {
                    Dictionary<int, int> m2 = Histogram(i, j, p1, p2);

                    if (Lexicographical(m2, m))
                    {
                        m = m2;
                        mv = (i, j);
                    }
                }
            }

            var (t1, t2) = mv;

            Debug.Assert(t1 >= 0 && t2 >= 0);

            this.histogram = Histogram(t1, t2, p1, p2);

            for (int i = 0; i < p1; ++i)
            {
                machines[t1 + i]++;
            }

            for (int i = 0; i < p2; ++i)
            {
                machines[t2 + i]++;
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