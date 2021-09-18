using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class CallableSolverOffline : ISolverOffline
    {
        public String callable;

        public CallableSolverOffline(String callable) {
            this.callable = callable;
        }

        public IEnumerable<String> marshalProblem(OfflineProblem problem) {
            int[] pars = new int[] {problem.processing_time_first_dose, problem.processing_time_second_dose, problem.gap} ;
            IEnumerable<String> values = pars.ToList<int>().Select<int, String>(n => n.ToString());

            foreach (Patient pat in problem.patient_data)
            {
                int[] vars = new int[] {pat.first_timeslot_first_dose, pat.last_timeslot_first_dose, pat.delay_between_doses, pat.second_dose_interval};
                IEnumerable<String> entry = vars.ToList<int>().Select<int, String>(n => n.ToString());
                values = values.Concat<String>(entry);
            }

            return values;
        } 

        public List<Registration> unmarshalRegs(List<int> values) {
            if (values.Count % 2 != 0)
            {
                throw new Exception($"callable returned uneven number of datapoints: {values.Count}");
            }

            List<Registration> regs = new List<Registration>();
            for (int i = 0; i < values.Count - 1; i += 2)
            {
                regs.Append<Registration>(new Registration(values[i], values[i + 1]));
            }

            return regs;
        }

        public Solution unmarshalSolution(String res) {
            var values = res.Split(' ').ToList<String>().Select<String, int>(Int32.Parse).ToList<int>();
            int machines = values[0];
            values.RemoveAt(0);
            
            List<Registration> regs = unmarshalRegs(values);

            return new Solution(machines, regs);
        } 

        public String call(IEnumerable<String> args) {
            // from: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=net-5.0#System_Diagnostics_Process_StandardOutput

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = this.callable;
            p.StartInfo.Arguments = String.Join(' ', args);
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
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