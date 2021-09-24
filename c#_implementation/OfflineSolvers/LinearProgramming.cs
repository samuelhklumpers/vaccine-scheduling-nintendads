using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgramming
    {


        Solver solver = new Solver("vaccine_scheduling", Solver.CLP_LINEAR_PROGRAMMING);

        foreach (Patient pat in problem.patient_data)
        {
            // Create the variables for the vaccine times.
            Variable t1 = solver.MakeNumVar(0.0, int.maxValue, "t1");
            Variable t2 = solver.MakeNumVar(0.0, int.maxValue, "t2");

            // Create two linear constraint, indicting in which intervals the times can fall.
            Constraint ct = solver.MakeConstraint();
            ct.SetCoefficient(x, 1);
            ct.SetCoefficient(y, 1);

            Constraint c1 = solver.Add(pat.first_timeslot_first_dose <= t1 <= pat.last_timeslot_first_dose - problem.processing_time_first_dose);

        }


        // Create the objective function, minimizing the number of hospitals.
        solver.minimizing(numHospitals); 

        solver.Solve();

        Console.WriteLine("Solution:");
        Console.WriteLine("Objective value = " + solver.Objective().Value());
        Console.WriteLine("x = " + x.SolutionValue());
        Console.WriteLine("y = " + y.SolutionValue());


    }
}