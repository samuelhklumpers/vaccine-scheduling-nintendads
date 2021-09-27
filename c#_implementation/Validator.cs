using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public static void validate(OfflineProblem problem, Solution solution)
        {
            if (problem.number_of_patients != solution.regs.Count)
            {
                throw new Exception($"solution and problem have differing numbers of patients: #regs={solution.regs.Count}, #patients={problem.number_of_patients}");
            }
            // TODO assert each registration is valid
            // assert the number of machines is correct
        }
    }

    class OnlineValidator
    {
        public static void validate(OnlineProblem problem, Solution solution)
        {
            if (problem.patient_data.Count != solution.regs.Count)
            {
                throw new Exception($"solution and problem have differing numbers of patients: #regs={solution.regs.Count}, #patients={problem.patient_data.Count}");
            }
            // TODO assert each registration is valid
            // assert the number of machines is correct
        }
    }
}
