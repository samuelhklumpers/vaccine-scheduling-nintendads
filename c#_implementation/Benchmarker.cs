using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


namespace implementation
{
    public class TroubleMaker // more commonly known as Arbitrary
    {
        // generate a random problem, with $n patients, and parameters within the given bounds
        public static OfflineProblem RandomProblem(int n, (int, int) p1B, (int, int) p2B, (int, int) gB, (int, int) rShiftB, (int, int) dd1B, (int, int) dd2B, (int, int) xB) {
            var rand = new Random();
            
            var p1 = rand.Next(p1B.Item1, p1B.Item2);
            var p2 = rand.Next(p2B.Item1, p2B.Item2);
            var g = rand.Next(gB.Item1, gB.Item2);
            
            var lastR = 0;

            var ps = new List<Patient>();

            for (int i = 0; i < n; ++i) {
                var r1 = lastR;
                var d1 = r1 + p1 + rand.Next(dd1B.Item1, dd1B.Item2);
                
                var xi = rand.Next(xB.Item1, xB.Item2);

                var L = p2 + rand.Next(dd2B.Item1, dd2B.Item2);

                var patient = new Patient(r1, d1, xi, L, p1, p2, g);

                ps.Add(patient);
                lastR += rand.Next(rShiftB.Item1, rShiftB.Item2);
            }

            OfflineProblem p = new OfflineProblem(p1, p2, g, n, ps);

            return p;
        }

        // generate a sane random problem with $n patients
        public static OfflineProblem RandomProblemPreset(int n) {
            var p1B = (1, 6);
            var p2B = (1, 6);
            var gB = (1, 6);
            var rShiftB = (0, 3);
            var dd1B = (1, 10);
            var dd2B = (1, 10);
            var xB = (0, 10);

            return RandomProblem(n, p1B, p2B, gB, rShiftB, dd1B, dd2B, xB);
        }
    }

    class Benchmarker
    {
        public bool silent;
        public bool init;

        public Benchmarker(bool silent, bool init)
        {
            this.silent = silent;
            this.init = init;
        }

        // test the $solvers $n times on problems of $m patients
        public static void RandomTest(IOfflineSolver[] solvers, int n, int m) {

            for (int i = 0; i < n; ++i)
            {
                var p = TroubleMaker.RandomProblemPreset(m);
                var validator =  new OfflineValidator(p);

                foreach (var solver in solvers) {
                    var sol = solver.solve(p);
                    validator.validate(sol);
                }
            }
        }

        public static void RandomTestOnline(IOnlineSolver[] solvers, int n, int m, bool ratio)
        {

            for (int i = 0; i < n; ++i)
            {
                var pOffline = TroubleMaker.RandomProblemPreset(m);
                var p = pOffline.ForgetN();
                var validator = new OnlineValidator(p);

                var opt = -1;
                if (ratio)
                    opt = new IntSatSolver().solve(pOffline).machines;

                foreach (var solver in solvers)
                {
                    Console.WriteLine("running " + solver.GetType().ToString());
                    var sol = solver.solve(p);
                    validator.validate(sol);

                    if (ratio)
                        Console.WriteLine($"ratio: {(double)sol.machines / opt}");
                }
            }
        }

        // run a benchmark on the $solvers, stopping when the first solver has run for more than $stop seconds in total 
        public Benchmark BenchmarkAll(IOfflineSolver[] solvers, double stop)
        {
            double[] ts = new double[solvers.Count()];
            double[] tInit = new double[solvers.Count()];

            List<double>[] result = new List<double>[solvers.Count()];
            List<OfflineProblem> testcases = new List<OfflineProblem>();

            int n = 2;
            var timer = new Stopwatch();


            for (int i = 0; i < solvers.Count(); ++i)
            {
                result[i] = new List<double>();
            }

            if (this.init)
            {
                for (int i = 0; i < solvers.Count(); ++i)
                {
                    // estimate the initialization time of the solver, to subtract from the running total
                    tInit[i] = Benchmarker.BenchmarkInit(solvers[i], 3);
                }
            }

            while (ts.All<double>(t => t < stop))
            {
                var p = TroubleMaker.RandomProblemPreset(n);
                var validator = new OfflineValidator(p);

                testcases.Add(p);

                Console.WriteLine($"benchmarking n = {n}");

                for (int i = 0; i < ts.Count(); ++i)
                {
                    Console.WriteLine("running " + solvers[i].GetType().ToString());
                    timer.Start();
                    var sol = solvers[i].solve(p);
                    timer.Stop();

                    if (!this.silent)
                    {
                        try
                        {
                            validator.validate(sol);
                        }
                        catch
                        {
                            Console.WriteLine("exception in " + solvers[i].GetType().ToString());
                            throw;
                        }
                    }

                    double dt = timer.Elapsed.TotalSeconds;
                    dt -= tInit[i];

                    Console.WriteLine($"passed {dt}s");

                    timer.Reset();
                    ts[i] += dt;
                    result[i].Add(dt);
                }

                n += 1; // or *= 2 if you're daring
            }

            return new Benchmark(solvers, result, testcases);
        }

        // estimate the initialization time of $solver, averaged over $n runs
        public static double BenchmarkInit(IOfflineSolver solver, int n) {
            var timer = new Stopwatch();
            var p = TroubleMaker.RandomProblemPreset(1);

            timer.Start();
            for (int i = 0; i < n; ++i)
            {
                solver.solve(p);
            }
            timer.Stop();

            return timer.Elapsed.TotalSeconds / n;
        }

        public class Benchmark
        {
            public IOfflineSolver[] solvers;
            public List<double>[] result;
            public List<OfflineProblem> testcases;


            public Benchmark(IOfflineSolver[] solvers, List<double>[] result, List<OfflineProblem> testcases)
            {
                this.solvers = solvers;
                this.result = result;
                this.testcases = testcases;
            }

            public override string ToString()
            {
                var str = $"number: " + String.Join(" ", testcases.Select(c => c.nPatients)) + "\n";

                for (int i = 0; i < this.result.Count(); ++i)
                {
                    var r = this.result[i];
                    str += $"solver {i + 1}: " + String.Join(" ", r.Select<double, String>(v => v.ToString("F2"))) + "\n";
                }

                str += "\n\n";

                for (int i = 0; i < this.solvers.Count(); ++i)
                {
                    str += $"solver {i + 1} = " + this.solvers[i].GetType().ToString() + "\n";
                }

                return str;
            }
        }
    }
}
