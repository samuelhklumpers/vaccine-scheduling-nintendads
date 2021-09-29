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
            Variable[] t1 = init_variables_vector(solver, max_i, max_t-problem.processing_time_first_dose+1); // t1,j starting time of vaccine 1 of patient i
            Variable[] t2 = init_variables_vector(solver, max_i, max_t-problem.processing_time_second_dose+1); // t2,j starting time of vaccine 2 of patient i

            // nog verder geimplementeerd worden. constraints toevoegen: per patient, per ziekenhuis constraint, sommen over tijd vaccinatie
            // als de som >= aan processing_time_1 * y[i,j,1] en constraint <= aan dat. dan als y =1 moet het processing time zijn, anders moet het 0 zijn.
            // dan nog constraint op y toevoegen dat per patient, per hospital en prik er maar 1 op 1 kan staan. en dat die overeen moet komen met x (dan andere constraints 
            // miss wel niet nodig, als het overeenkomt met x kan je y al gebruiken dat er maar 1 hospital mag zijn per patient per prik)
            Variable[,,] y =  init_3d_boolean_variable_vector(solver, max_i, max_j, 2);//y_ijt is one if patient j is in hospital i for vaccine t

            //of je gaat over tijd van 1 prik heen, telt het aantal hospitals die gebruikt worden, dit mag 1 zijn.

            for (int i = 0; i < max_i; i++)
            {
                Patient p = problem.patient_data[i];
                Constraint ct_t1_in_timeslot = solver.MakeConstraint(p.first_timeslot_first_dose, p.last_timeslot_first_dose - problem.processing_time_first_dose + 1);
                ct_t1_in_timeslot.SetCoefficient(t1[i], 1);

                //every patients gets their vaccination planned
                Constraint ct_fill_1st_vaccine_slot = solver.MakeConstraint(problem.processing_time_first_dose, problem.processing_time_first_dose);
                for (int j = 0; j < max_j; j++)
                {
                    for (int t = 0; t < problem.processing_time_first_dose - 1; t++) //over goeie tijd loopen
                    {
                        ct_fill_1st_vaccine_slot.SetCoefficient(x[i, j, t+(int)t1[i].SolutionValue()], 1); //DIT CHECKEN!!!
                    }
                }

               
                for (int j = 0; j < max_j; j++)
                {
                    Constraint ct_1st_vaccine_slot_one_hospital = solver.MakeConstraint(0, problem.processing_time_first_dose);
                    Constraint ct_1st_vaccine_slot_one_hospital_2 = solver.MakeConstraint(1, problem.processing_time_first_dose-1); //juist niet in dit interval

                    for (int t = 0; t < max_t; t++)
                    {
                        ct_1st_vaccine_slot_one_hospital.SetCoefficient(x[i, j, t], 1);
                        ct_1st_vaccine_slot_one_hospital_2.SetCoefficient(x[i, j, t], 1);
                    }
                }


                Constraint ct_t2_in_timeslot = solver.MakeConstraint(/*t1[i] +*/ problem.processing_time_first_dose-1 + problem.gap + p.delay_between_doses, /*t1[i] +*/ problem.processing_time_first_dose-1 + problem.gap + p.delay_between_doses + p.second_dose_interval);
                ct_t2_in_timeslot.SetCoefficient(t2[i], 1);

                //every patients gets their vaccination planned
                Constraint ct_fill_2nd_vaccine_slot = solver.MakeConstraint(problem.processing_time_second_dose, problem.processing_time_second_dose);

                for (int j = 0; j < max_j; j++)
                {
                    for (int t = 0; t < max_t; t++)
                    {
                        ct_fill_2nd_vaccine_slot.SetCoefficient(x[i, j, t], 1);
                    }
                }

            }

            //weet niet of dit kan werken, maar dan zou je objective op minimalize numhospitals kunnen laten
            solver.Add(numHospitals.SolutionValue() <= calculate_num_hospitals(x, max_i, max_j, max_t));
            solver.Add(numHospitals.SolutionValue() >= calculate_num_hospitals(x, max_i, max_j, max_t));

            Console.WriteLine("Number of variables = " + solver.NumVariables());
            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals.
            solver.Minimize(numHospitals);

            //solver.Minimize(max(sum(x[i,j,t])));

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

        static private double calculate_num_hospitals(Variable[,,] x, int i_max, int j_max, int t_max)
        {
            //calculate the number of hospitals used
            double max_hospitals = 0;
            for (int t = 0; t < t_max; t++)
            {
                double num_hospital = 0;
                for (int i = 0; i < i_max; i++)
                {
                   
                    for (int j = 0; j < j_max; j++)
                    {
                        num_hospital += x[i,j,t].SolutionValue();
                    }
                }
                max_hospitals = max(max_hospitals, num_hospital);
            }

            return max_hospitals;

        }
    }
}