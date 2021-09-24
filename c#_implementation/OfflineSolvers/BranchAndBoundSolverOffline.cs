using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace implementation
{
    class BranchAndBoundSolverOffline : ISolverOffline
    {

        public Solution solve(OfflineProblem problem)
        {
            //tak is lege solution, hier lower bound pakken met LP en upper bound met bruteforce oplossing
            //dan branchen door random 1 persoon in te vullen, dit gaat DF. Hier opnieuw LP en bruteforce, alleen dan geef je ze partial oplossing mee. 
            
            
            return new Solution(0, new List<Registration>());
        }




        



    }
}