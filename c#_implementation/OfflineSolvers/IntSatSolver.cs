using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Google.OrTools.Sat;


namespace implementation
{
    class IntSatSolver : IOfflineSolver
    {
        public Solution solve(OfflineProblem p)
        {
            var (p1, p2, g) = (p.p1, p.p2, p.g);
            var ps = p.patients;

            Solution success = null;

            int low = 0;
            int high = ps.Count + 1;
            while (low != high)
            {
                int m = (low + high) / 2;
                var res = Satisfy(m, p1, p2, g, ps);

                if (res != null)
                {
                    success = res;
                    high = m;
                }
                else
                {
                    low = m + 1;
                }
            }

            Debug.Assert(success != null); // please don't insert invalid problems

            return success;
        }

        public Solution Satisfy(int m, int p1, int p2, int g, List<Patient> ps)
        {
            CpModel model = new CpModel();
            int n = ps.Count;

            IntVar[] t1s = new IntVar[n];
            IntVar[] t2s = new IntVar[n];
            IntVar[] h1s = new IntVar[n];
            IntVar[] h2s = new IntVar[n];

            for (int i = 0; i < n; ++i)
            {
                var p = ps[i];

                var (left1, right1) = (p.r1, p.d1 - p1 + 1);
                var (left2, right2) = (left1 + p1 + g + p.x, right1 + p1 + g + p.x + p.L - p2 + 1);

                t1s[i] = model.NewIntVar(left1, right1, $"t1_{i}");
                t2s[i] = model.NewIntVar(left2, right2, $"t2_{i}");

                h1s[i] = model.NewIntVar(1, m, $"h1_{i}");
                h2s[i] = model.NewIntVar(1, m, $"h2_{i}");

                var r2 = t1s[i] + p1 + p.x + g;
                var d2 = r2 + p.L - p2 + 1;

                model.Add(r2 <= t2s[i]);
                model.Add(t2s[i] <= d2);
            }

            NoOverlapSquare(model, t1s, h1s, t2s, h2s, p1, p2);
            NoOverlapDiag(model, t1s, h1s, p1, 1);
            NoOverlapDiag(model, t2s, h2s, p2, 2);

            CpSolver solver = new CpSolver();

            var status = solver.Solve(model);
            var res = status == CpSolverStatus.Feasible || status == CpSolverStatus.Optimal;

            if (!res)
                return null;

            int[] t1v = t1s.Select(t => (int)solver.Value(t)).ToArray();
            int[] t2v = t2s.Select(t => (int)solver.Value(t)).ToArray();
            int[] h1v = h1s.Select(h => (int)solver.Value(h)).ToArray();
            int[] h2v = h2s.Select(h => (int)solver.Value(h)).ToArray();

            List<Doses2D> doses = new List<Doses2D>();

            for (int i = 0; i < t1v.Length; ++i)
            {
                doses.Add(new Doses2D(t1v[i], h1v[i], t2v[i], h2v[i]));
            }

            return new Solution2D(m, doses);
        }

        public void NoOverlapSquare(CpModel model, IntVar[] t1s, IntVar[] h1s, IntVar[] t2s, IntVar[] h2s, int p1, int p2)
        {
            for (int i = 0; i < t1s.Length; ++i)
            {
                for (int j = 0; j < t2s.Length; ++j)
                {
                    IntVar b = model.NewBoolVar($"b_{i}_{j}");

                    model.Add(h1s[i] == h2s[j]).OnlyEnforceIf(b); // bleh, we need to introduce a variable to indicate whether the hospitals are equal
                    model.Add(h1s[i] != h2s[j]).OnlyEnforceIf(b.Not()); // since we can only pass literals to OnlyEnforceIf, rather than boolean expressions

                    // maybe we can reify this multiplying with h1s == h2s?

                    IntVar left = model.NewBoolVar($"l_{i}_{j}");
                    IntVar right = model.NewBoolVar($"r_{i}_{j}");

                    model.Add(t1s[i] + p1 <= t2s[j]).OnlyEnforceIf(left);
                    model.Add(t2s[j] + p2 <= t1s[i]).OnlyEnforceIf(right);
                    model.Add(t1s[i] + p1 > t2s[j]).OnlyEnforceIf(left.Not());
                    model.Add(t2s[j] + p2 > t1s[i]).OnlyEnforceIf(right.Not());

                    model.AddBoolOr(new IntVar[] { left, right }).OnlyEnforceIf(b);
                }
            }
        }

        public void NoOverlapDiag(CpModel model, IntVar[] ts, IntVar[] hs, int p, int k)
        {
            for (int i = 0; i < ts.Length; ++i)
            {
                for (int j = i + 1; j < ts.Length; ++j)
                {
                    IntVar b = model.NewBoolVar($"b_{i}_{j}_{k}");

                    model.Add(hs[i] == hs[j]).OnlyEnforceIf(b);
                    model.Add(hs[i] != hs[j]).OnlyEnforceIf(b.Not());

                    IntVar left = model.NewBoolVar($"l_{i}_{j}_{k}");
                    IntVar right = model.NewBoolVar($"r_{i}_{j}_{k}");

                    model.Add(ts[i] + p <= ts[j]).OnlyEnforceIf(left);
                    model.Add(ts[j] + p <= ts[i]).OnlyEnforceIf(right);
                    model.Add(ts[i] + p > ts[j]).OnlyEnforceIf(left.Not());
                    model.Add(ts[j] + p > ts[i]).OnlyEnforceIf(right.Not());

                    model.AddBoolOr(new IntVar[] { left, right }).OnlyEnforceIf(b);
                }
            }
        }
    }
}