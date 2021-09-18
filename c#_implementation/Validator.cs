using System;
using System.Collections.Generic;

namespace implementation
{
    class OfflineValidator
    {
        public void validate(OfflineProblem problem, Solution solution) {
            if (problem.number_of_patients != solution.regs.Count) {
                throw new Exception($"solution and problem have differing numbers of patients: #regs={solution.regs.Count}, #patients={problem.number_of_patients}");
            }
            // assert each registration is valid
            // assert the number of machines is correct
        }
    }
}
