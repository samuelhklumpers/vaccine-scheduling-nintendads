using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgramming
    {
        static void Solve(OfflineProblem problem)
        {
            //TOEVOEGEN: partial filled solution meegeven voor verder in de boom.

            Solver solver = new Solver("vaccine_scheduling", Solver.CLP_LINEAR_PROGRAMMING);

            //Create variable for number of hospitals.
            Variable numHospitals = solver.MakeNumVar(0.0, int.MaxValue, "numHospitals"); 
            
            //Create variable lists as each patient has that variable
            Variable[] t1 = new Variable[problem.number_of_patients];
            Variable[] t2 = new Variable[problem.number_of_patients];
            Variable[] h1 = new Variable[problem.number_of_patients];
            Variable[] h2 = new Variable[problem.number_of_patients];

            //Create constraint lists as each patient has that constraint
            Constraint[] c1 = new Constraint[problem.number_of_patients];
            Constraint[] c2 = new Constraint[problem.number_of_patients];
            Constraint[] c3 = new Constraint[problem.number_of_patients];
            Constraint[] c4 = new Constraint[problem.number_of_patients];
            Constraint[] c5 = new Constraint[problem.number_of_patients];
            Constraint[] c6 = new Constraint[problem.number_of_patients];


            int i = 0;
            foreach (Patient pat in problem.patient_data)
            {
                
                // Create the variables for the vaccine times and hospitals.
                t1[i] = solver.MakeNumVar(0.0, int.maxValue, t1[i]);
                t2[i] = solver.MakeNumVar(0.0, int.maxValue, t2[i]);
                h1[i] = solver.MakeNumVar(0.0, int.maxValue, h1[i]);
                h2[i] = solver.MakeNumVar(0.0, int.maxValue, h2[i]);


                // Create two constraint, indicting in which intervals the times can fall.
                c1[i] = solver.Add(pat.first_timeslot_first_dose <= t1[i] <= pat.last_timeslot_first_dose - problem.processing_time_first_dose + 1);
                c2[i] = solver.Add(t1[i] + problem.processing_time_first_dose + problem.gap + pat.delay_between_doses <= t2[i] <= t1[i] + problem.processing_time_first_dose + problem.gap + pat.delay_between_doses + pat.second_dose_interval);
                //Create 4 constraints to ensure that the
                constraint c3 = solver.Add(h1[i] in range(numHospitals));//h1 moet in beschikbare hospitals zitten (binnen range van numHospitals)
                constraint c4 = solver.Add(h2[i] in range(numHospitals));//h2 moet in beschikbare hospitals zitten (binnen range van numHospitals)
                constraint c5 = solver.Add();//t1 moet beschikbare tijd van h1 hebben--> niet twee patienten op zelfde tijd in zelfde hospital
                constraint c6 = solver.Add();//t2 moet beschikbare tijd van h2 hebben

                //constraints voor waarden van variabelen toevoegen
                i++;
            }

            

            Console.WriteLine(solver.variables());

            // Create the objective function, minimizing the number of hospitals.
            solver.minimizing(numHospitals); 

            solver.Solve();

            Console.WriteLine("Solution:");
            Console.WriteLine("Objective value = " + solver.objectiveValue());
            foreach(Patient pat in problem.patient_data)
            {
                //Console.WriteLine("x = " + x.SolutionValue());
                //Console.WriteLine("y = " + y.SolutionValue());
                
            }


        }
        


    }
}