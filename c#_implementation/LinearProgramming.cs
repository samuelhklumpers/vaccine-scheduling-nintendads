using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgramming
    {
        public static void Solve(OfflineProblem problem)
        {
            //TOEVOEGEN: partial filled solution meegeven voor verder in de boom.

            Solver solver = new Solver("vaccine_scheduling", Solver.OptimizationProblemType.CLP_LINEAR_PROGRAMMING);

            int max_hospitals_upperbound = problem.patient_data.Count; //hospitals upperbound is when every patient gets its own hospital
            int max_time_upperbound = calculate_upperbound_time(problem); //time upperbound is from 0 until the end of the last 2nd dose interval

            //Create variable for number of hospitals.
            Variable numHospitals = solver.MakeNumVar(0.0, max_hospitals_upperbound, "numHospitals");

            int max_i = problem.number_of_patients;
            int max_j = max_hospitals_upperbound;
            int max_t = max_time_upperbound;

            //create all boolean variables for x 
            Variable[,,] x = init_3d_boolean_variable_vector(solver, max_i, max_j, max_t);//x_ijt is one if patient j is in hospital i at time t

            //sum patients <=1, for every hospital at any time
            add_constraint_no_two_people_per_hospital_at_all_times(solver, x, max_i, max_j, max_t);
            //sum hospitals <=1, for every patient at any time
            add_constraint_no_two_hospitals_per_patient_at_all_times(solver, x, max_i, max_j, max_t);
            //sum hospitals sum times = p_1 + p_2, for every patient
            add_constraint_total_vaccination_time_equals_p1_plus_p2(solver, x, max_i, max_j, max_t, problem.processing_time_first_dose + problem.processing_time_second_dose);
            //TODO same vaccine in same hospital

            //Create variable lists as each patient has that variable
            Variable[] t1 = init_variables_vector(solver, max_i, max_t); // t1,j starting time of vaccine 1 of patient i
            Variable[] e1 = init_variables_vector(solver, max_i, max_t); // e1,j ending time of vaccine 1 of patient i
            Variable[] t2 = init_variables_vector(solver, max_i, max_t); // t2,j starting time of vaccine 2 of patient i
            Variable[] e2 = init_variables_vector(solver, max_i, max_t); // e2,j ending time of vaccine 2 of patient i


            /*for (int i = 0; i < max_i; i++)
            {
                Patient p = problem.patient_data[i];
                Constraint ct_t1_in_timeslot = solver.MakeConstraint(p.first_timeslot_first_dose, p.last_timeslot_first_dose - problem.processing_time_first_dose + 1);
                Constraint ct_e1_in_timeslot = solver.MakeConstraint(p.first_timeslot_first_dose + problem.processing_time_first_dose - 1, p.last_timeslot_first_dose);
                ct_t1_in_timeslot.SetCoefficient(t1[i], 1);
                ct_e1_in_timeslot.SetCoefficient(e1[i], 1);

                //every patients gets their vaccination planned
                Constraint ct_fill_1st_vaccine_slot = solver.MakeConstraint(problem.processing_time_first_dose, problem.processing_time_first_dose);

                for (int j = 0; j < max_j; j++)
                {
                    for (int t = 0; t < max_t; t++)
                    {
                        ct_fill_1st_vaccine_slot.SetCoefficient(x[i, j, t], 1);
                    }
                }


            }*/

            //Constraint ct = solver.MakeConstraint(x[])

            /*

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

            Constraint c3 = solver.Add(IEnumerable.sum(t1[j] => t1[j] + problem.processing_time_first_dose, x[j][i][t] for all j, t there is an i));
*/

            Console.WriteLine("Number of variables = " + solver.NumVariables());
            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals.
            solver.Minimize(numHospitals);

            solver.Solve();

            Console.WriteLine("Solution:");

            Console.WriteLine("Objective value = " + solver.Objective());
            foreach (var variable in solver.variables())
            {
                Console.WriteLine(variable.Name() + ": " + variable.SolutionValue());
            }

        }
        static private int calculate_upperbound_time(OfflineProblem problem)
        {
            int max_time_upperbound = 0;
            foreach (Patient p in problem.patient_data)
            {
                max_time_upperbound = Math.Max(p.delay_between_doses + p.last_timeslot_first_dose + p.second_dose_interval + problem.gap, max_time_upperbound);
            }
            return max_time_upperbound;
        }
        static private Variable[,,] init_3d_boolean_variable_vector(Solver solver, int i_max, int j_max, int t_max)
        {
            //x_ijt is one if patient j is in hospital i at time t
            Variable[,,] x = new Variable[i_max, j_max, t_max];
            //fill x with valid variables inside the solvers context
            for (int i = 0; i < i_max; i++)
            {
                for (int j = 0; j < j_max; j++)
                {
                    for (int t = 0; t < t_max; t++)
                    {
                        x[i, j, t] = solver.MakeBoolVar("x: " + i.ToString() + " " + j.ToString() + " " + t.ToString());
                        Constraint x_constraint = solver.MakeConstraint(0, 1);
                        x_constraint.SetCoefficient(x[i, j, t], 1);
                    }
                }
            }
            return x;
        }
        static private Variable[] init_variables_vector(Solver solver, int length, int ub)
        {
            Variable[] vector = new Variable[length];
            for (int i = 0; i < length; i++)
            {
                vector[i] = solver.MakeIntVar(0, ub, i.ToString());
            }
            return vector;
        }
        static private void add_constraint_no_two_people_per_hospital_at_all_times(Solver solver, Variable[,,] x, int i_max, int j_max, int t_max)
        {
            //add constraint which makes sure that every hospital at any time has <= 1 patients
            for (int t = 0; t < t_max; t++)
            {
                for (int j = 0; j < j_max; j++)
                {
                    Constraint ct_num_patients_per_hospital = solver.MakeConstraint(0, 1);
                    for (int i = 0; i < i_max; i++)
                    {
                        ct_num_patients_per_hospital.SetCoefficient(x[i, j, t], 1);
                    }
                }
            }
        }
        static private void add_constraint_no_two_hospitals_per_patient_at_all_times(Solver solver, Variable[,,] x, int i_max, int j_max, int t_max)
        {
            //add constraint which makes sure that every patient at any time has <= 1 hospital
            for (int t = 0; t < t_max; t++)
            {
                for (int i = 0; i < i_max; i++)
                {
                    Constraint ct_num_hospitals_per_patient = solver.MakeConstraint(0, 1);
                    for (int j = 0; j < j_max; j++)
                    {
                        ct_num_hospitals_per_patient.SetCoefficient(x[i, j, t], 1);
                    }
                }
            }
        }
        static private void add_constraint_total_vaccination_time_equals_p1_plus_p2(Solver solver, Variable[,,] x, int i_max, int j_max, int t_max, int total_vaccination_time)
        {
            //add constraint which makes sure that every patient has exactly p_1 + p_2 timeslots
            for (int i = 0; i < i_max; i++)
            {
                Constraint ct_num_days_vaccinating = solver.MakeConstraint(total_vaccination_time, total_vaccination_time);
                for (int j = 0; j < j_max; j++)
                {
                    for (int t = 0; t < t_max; t++)
                    {
                        ct_num_days_vaccinating.SetCoefficient(x[i, j, t], 1);
                    }
                }
            }
        }
    }
}