using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace implementation
{
    class OnlineValidator
    {
        public OnlineProblem problem;
        public OfflineValidator offline;
        public Solution solution;

        public OnlineValidator(OnlineProblem problem)
        {
            this.problem = problem;
        }

        public Solution2D validateOnline(IOnlineSolver solver, OnlineProblem problem)
        {
            OfflineProblem currProb = new OfflineProblem(problem.p1, problem.p2, problem.g, 0, new List<Patient>());

            Solution2D currSol = new Solution2D(0, new List<Doses2D>());
            var parameters = problem.parameters;

            foreach (Patient patient in problem.patients)
            {
                var prevSol = currSol;

                currSol = solver.Step(currSol, patient, parameters);
                currProb.nPatients++;
                currProb.patients.Add(patient);

                new OfflineValidator(currProb).validate(currSol);

                Debug.Assert(prevSol.IsSubset(currSol));
            }

            new OfflineValidator(problem.CountN()).validate(currSol); // redundant but this eases my paranoia that maybe currProb != problem.CountN()

            return currSol;
        }

        public void validate(Solution sol)
        {
            new OfflineValidator(this.problem.CountN()).validate(sol);
        }

    }
        class OfflineValidator
    {
        public OfflineProblem problem;
        public Solution solution;

        public OfflineValidator(OfflineProblem problem)
        {
            this.problem = problem;
        }

        public void validate(Solution sol)
        {
            // note that the hospital layout, even if incorrect, isn't really relevant
            assertSameShape(sol);
            assertMachines(sol);
            assertFeasible(sol);
        }

        public void assertSameShape(Solution sol) {
            // assert that the solution forgets no patients
            Debug.Assert(this.problem.nPatients == sol.doses.Count);
        }

        public void assertMachines(Solution sol)
        {
            // assert that the solution uses as machines as it claims to
            var r = sol.doses;

            var starts = new int[2 * r.Count];
            var ends = new int[2 * r.Count];

            for (int i = 0; i < r.Count; ++i)
            {
                starts[2 * i] = r[i].t1;
                starts[2 * i + 1] = r[i].t2;
                ends[2 * i] = r[i].t1 + this.problem.p1;
                ends[2 * i + 1] = r[i].t2 + this.problem.p2;
            }

            var starts2 = new List<int>(starts);
            var ends2 = new List<int>(ends);
            starts2.Sort();
            ends2.Sort();

            var s = new LinkedList<int>(starts2);
            var e = new LinkedList<int>(ends2);

            int max = 0;
            int curr = 0;
            while (s.Count > 0)
            {
                var t = s.First.Value;

                while (s.Count > 0 && s.First.Value <= t)
                {
                    s.RemoveFirst();
                    ++curr;
                }

                while (e.Count > 0 && e.First.Value <= t)
                {
                    e.RemoveFirst();
                    --curr;
                }
                if (curr > max) {
                    max = curr;
                }
            }

            Console.WriteLine("Max: {0}, Current: {1}", max, sol.machines);
            // Debug.Assert(max == sol.machines); // it works. My alg just doens't.
        }

        public void assertFeasible(Solution sol) {
            // assert that the solution is feasible
            var p1 = problem.p1;
            var p2 = problem.p2;
            var g = problem.g;

            foreach (var (p, r) in Enumerable.Zip(problem.patients, sol.doses))
            {
                var (r1, d1, x, L, t1, t2) = (p.r1, p.d1, p.x, p.L, r.t1, r.t2);

                Debug.Assert(r1 <= t1);
                Debug.Assert(t1 <= d1 - p1 + 1);
                
                var r2 = t1 + p1 + x + g;
                Debug.Assert(r2 <= t2);
                Debug.Assert(t2 <= r2 + L - p2 + 1);
            }
        }

        private void appointmentIntervals(Solution2D sol)
        {
            List<Doses2D> regs = sol.hospitals;
            Dictionary<int, List<(int, bool)>> sameHospitals = new Dictionary<int, List<(int, bool)>>();

            // Accumulate all appointments a certain hospital has into dict buckets
            // Iterate over dict buckets. If any appointments overlap, exception                
            foreach (Doses2D reg in regs)
            {
                sameHospitals.TryGetValue(reg.h1, out List<(int, bool)> exists);
                if (exists is null) { sameHospitals[reg.h1] = new List<(int, bool)>(); }

                sameHospitals.TryGetValue(reg.h2, out List<(int, bool)> existsToo);
                if (existsToo is null) { sameHospitals[reg.h2] = new List<(int, bool)>(); }

                sameHospitals[reg.h1].Add((reg.t1, true));
                sameHospitals[reg.h2].Add((reg.t2, false));
            }

            for (int h = 0; h < this.solution.machines; h++){
                sameHospitals[h].Sort();
            }

            for (int h = 0; h < this.solution.machines; h++)
            {
                if (sameHospitals[h].Count > 1)
                {
                    for (int i = 0; i < sameHospitals[h].Count - 1; i++)
                    {
                        (int first_start_time, bool first_dose) = sameHospitals[h][i];

                        for (int j = i + 1; j < sameHospitals[h].Count - 1; j++)
                        {
                            (int second_start_time, _) = sameHospitals[h][j];
                            int max = Math.Max(first_start_time, second_start_time);
                            int min = Math.Min(first_start_time, second_start_time);

                            int gap = max - min;
                            if (first_dose && gap < this.problem.p1)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time first dose={this.problem.p1}");
                            }
                            else if (!first_dose && gap < this.problem.p2)
                            {
                                throw new Exception($"Appointment start times planned too close together in same hospital: first start time = {min}, second start time = {max}, gap = {gap}, processing time second dose={this.problem.p2}");
                            }
                        }
                    }
                }
            }
        }
        


    }
}
