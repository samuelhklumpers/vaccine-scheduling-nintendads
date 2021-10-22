using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class CallableSolver : IOfflineSolver
    {
        public String callable;
        public String[] args;

        // make a solver from an executable $callable, called with $args, followed by the problem in the format of marshalProblem 
        public CallableSolver(String callable, String[] args)
        {
            this.callable = callable;
            this.args = args;
        }
        public CallableSolver()
        {
            this.callable = "C:\\Program Files\\swipl\\bin\\swipl.exe";
            this.args = new String[] { "./Callables/constraint_programming.pl" };
        }

        // transform a OfflineProblem to a flat string representation
        public IEnumerable<String> marshalProblem(OfflineProblem problem)
        {
            int[] parameters = new int[] { problem.p1, problem.p2, problem.g };
            IEnumerable<String> values = parameters.ToList<int>().Select<int, String>(n => n.ToString());

            foreach (Patient p in problem.patients)
            {
                int[] vars = new int[] { p.r1, p.d1, p.x, p.L };
                IEnumerable<String> entry = vars.ToList<int>().Select<int, String>(n => n.ToString());
                values = values.Concat<String>(entry);
            }

            return values;
        }

        // load list of doses in string representation into an enumerable
        public IEnumerable<Doses> unmarshalRegs(int[] values)
        {
            if (values.Length % 2 != 0)
            {
                throw new Exception($"callable returned uneven number of doses: {values.Length}");
            }

            IEnumerable<Doses> doses = new List<Doses>();
            for (int i = 0; i < values.Length - 1; i += 2)
            {
                doses = doses.Append<Doses>(new Doses(values[i], values[i + 1]));
            }

            return doses;
        }

        // transform a solution in string representation to a Solution
        public Solution unmarshalSolution(String res)
        {
            var values = res.Split(' ').Select<String, int>(Int32.Parse).ToArray();
            int machines = values[0];
            int[] doses_ = values.Skip(1).ToArray();

            List<Doses> regs = unmarshalRegs(doses_).ToList();

            return new Solution(machines, regs);
        }

        // call the executable with additional $args
        public String call(IEnumerable<String> args)
        {
            // from: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=net-5.0#System_Diagnostics_Process_StandardOutput
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = this.callable;
            p.StartInfo.Arguments = String.Join(" ", this.args.Concat<String>(args));
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        public Solution solve(OfflineProblem problem)
        {
            var args = marshalProblem(problem);
            var res = call(args);
            var sol = unmarshalSolution(res);

            return sol;
        }
    }
}