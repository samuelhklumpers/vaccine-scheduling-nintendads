using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


namespace implementation
{
    public class TroubleMaker // more commonly known as Arbitrary
    {
        public Random random;

        public TroubleMaker()
        {
            this.random = new Random();
        }

        public TroubleMaker(int seed)
        {
            this.random = new Random(seed);
        }

        // generate a random problem, with $n patients, and parameters within the given bounds
        public OfflineProblem RandomProblem(int n, (int, int) p1B, (int, int) p2B, (int, int) gB, (int, int) rShiftB, (int, int) dd1B, (int, int) dd2B, (int, int) xB) {
            var rand = this.random;
            
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
        public OfflineProblem RandomProblemPreset(int n) {
            var p1B = (1, 6);
            var p2B = (1, 6);
            var gB = (1, 6);
            var rShiftB = (0, 3);
            var dd1B = (1, 10);
            var dd2B = (1, 10);
            var xB = (0, 10);

            return this.RandomProblem(n, p1B, p2B, gB, rShiftB, dd1B, dd2B, xB);
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

            var trouble = new TroubleMaker();

            for (int i = 0; i < n; ++i)
            {
                var p = trouble.RandomProblemPreset(m);
                var validator =  new OfflineValidator(p);

                foreach (var solver in solvers) {
                    var sol = solver.solve(p);
                    validator.validate(sol);
                    Console.WriteLine("----");
                }

                Console.WriteLine("====");
            }
        }

        public static string BenchmarkSeries(IOfflineSolver solver, double timeout, int tries, int seed)
        {
            var trouble = new TroubleMaker(seed);
            var timer = new Stopwatch();
            var times = new List<double>();

            int n = 1;
            while (true)
            {

                timer.Start();
                for (int i = 0; i < tries; ++i)
                {
                    var p = trouble.RandomProblemPreset(n);
                    solver.solve(p);
                }
                timer.Stop();

                double dt = timer.Elapsed.TotalSeconds / tries;
                times.Add(dt);
                timer.Reset();

                if (dt > timeout)
                {
                    break;
                }

                ++n;
            }

            var ret = String.Join(", ", times.Select<double, string>(x => x.ToString()));
            ret = "[" + ret + "]";

            return ret;
        }


        public static (double, double) RandomRatioOnline(IOnlineSolver solver, int runs, int size, int seed)
        {
            var (cumOpt, cumAlg) = (0, 0);
            double worst = 0.0;

            var trouble = new TroubleMaker(seed);
            var opt = new IntSatSolver();

            for (int i = 0; i < runs; ++i)
            {
                var p = trouble.RandomProblemPreset(size);

                var sol = new OnlineValidator(p.ForgetN()).validateOnline(solver);

                var currAlg = sol.machines;
                var currOpt = opt.solve(p).machines;

                double ratio = (double)currAlg / currOpt;

                if (ratio > worst)
                {
                    worst = ratio;
                }

                cumAlg += currAlg;
                cumOpt += currOpt;
            }

            return ((double)cumAlg / cumOpt, worst);
        }

        public static void RandomTestOnline(IOnlineSolver[] solvers, int n, int m, bool ratio)
        {
            var trouble = new TroubleMaker();

            for (int i = 0; i < n; ++i)
            {
                var pOffline = trouble.RandomProblemPreset(m);
                var p = pOffline.ForgetN();
                var validator = new OnlineValidator(p);

                var opt = -1;
                if (ratio)
                    opt = new IntSatSolver().solve(pOffline).machines;

                foreach (var solver in solvers)
                {
                    Console.WriteLine("running " + solver.GetType().ToString());
                    
                    var sol = validator.validateOnline(solver, p);

                    if (ratio)
                        Console.WriteLine($"ratio: {(double)sol.machines / opt}");

                    Console.WriteLine("----");
                }


                Console.WriteLine("====");
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

            var trouble = new TroubleMaker();

            while (ts.All<double>(t => t < stop))
            {
                var p = trouble.RandomProblemPreset(n);
                var validator = new OfflineValidator(p);

                testcases.Add(p);

                var pigeons = Bounds.PigeonHole(p);

                Console.WriteLine($"benchmarking n = {n}");
                Console.WriteLine($"pigeonhole = {pigeons}");
                Console.WriteLine("==========");

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

                    Console.WriteLine($"machines {sol.machines}");
                    Console.WriteLine($"passed {dt}s");

                    timer.Reset();
                    ts[i] += dt;
                    result[i].Add(dt);
                }

                n += 1; // or *= 2 if you're daring

                Console.WriteLine("---------");
            }

            return new Benchmark(solvers, result, testcases);
        }

        // estimate the initialization time of $solver, averaged over $n runs
        public static double BenchmarkInit(IOfflineSolver solver, int n) {
            var trouble = new TroubleMaker();

            var timer = new Stopwatch();
            var p = trouble.RandomProblemPreset(1);

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
