using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgramming2
    {
        public static void Solve(OfflineProblem problem)
        {
            //TOEVOEGEN: partial filled solution meegeven voor verder in de boom.

            List<Job> jobs = new List<Job>();

            for (int i = 0; i < problem.patient_data.Count; i++)
            {
                Patient patient = problem.patient_data[i];
                Job job1 = new Job(patient, 1, i * 2);
                Job job2 = new Job(patient, 2, i * 2 + 1);

                jobs.Add(job1);
                jobs.Add(job2);
            }

            Solver solver = new Solver("vaccine_scheduling", Solver.OptimizationProblemType.SCIP_MIXED_INTEGER_PROGRAMMING);

            int max_hospitals_upperbound = problem.patient_data.Count; //hospitals upperbound is when every patient gets its own hospital
            int max_time_upperbound = calculate_upperbound_time(problem); //time upperbound is from 0 until the end of the last 2nd dose interval

            //Create variable for number of hospitals.
            //Variable numHospitals = solver.MakeNumVar(0.0, max_hospitals_upperbound, "numHospitals");

            int max_j = jobs.Count;
            int max_h = max_hospitals_upperbound;
            int max_t = max_time_upperbound;

            //Create variable lists as each patient has that variable
            Variable[] t = init_variables_vector(solver, max_j, max_t); // tj starting time of job j

            Variable[,] z = init_2d_boolean_variable_z(solver, max_j); // z_j,j' --> 1 if job j starts before job j'
            Variable[,] y = init_2d_boolean_variable_y(solver, max_j, max_h); // y_j_h --> 1 when job j in hospital h
            Variable[,] samehospitals = init_2d_boolean_variable_samehospitals(solver, max_j);

            //Add the constraints to the solver
            add_constraints_z(solver, z, t, jobs, max_t); // calculate which jobs are before other jobs
            add_constraints_samehospital(solver, samehospitals, y, jobs, max_h);
            add_constraints_y(solver, y, jobs, max_h); //
            add_constraint_interval_vaccines(solver, problem, t, jobs);
            add_constraint_no_two_patients_at_the_same_time(solver, problem, t, z, y, samehospitals, jobs, max_h, max_t);

            Console.WriteLine("Number of variables = " + solver.NumVariables());
            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals.
            solver.Minimize(new LinearExpr());


            solver.Solve();

            Console.WriteLine("Solution:");

            Console.WriteLine("Objective value = " + solver.Objective().Value());
            Console.WriteLine("mat z:");
            int[,] resulting_matrix_z = new int[max_j, max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 'z')
                {
                    resulting_matrix_z[int.Parse(data[1]), int.Parse(data[2])] = (int)variable.SolutionValue();
                }
            }
            for (int i = 0; i < resulting_matrix_z.Length; i++)
            {
                if (i % max_j == 0)
                {
                    Console.WriteLine();
                }
                Console.Write(resulting_matrix_z[i / max_j, i % max_j] + " ");

            }
            Console.WriteLine();

            Console.WriteLine("mat y:");
            int[,] resulting_matrix_y = new int[max_j, max_h];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 'y')
                {
                    resulting_matrix_y[int.Parse(data[1]), int.Parse(data[2])] = (int)variable.SolutionValue();
                }
            }
            for (int i = 0; i < resulting_matrix_y.Length; i++)
            {
                if (i % max_h == 0)
                {
                    Console.WriteLine();
                }
                Console.Write(resulting_matrix_y[i / max_j, i % max_h] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("mat t:");
            int[] resulting_vector_t = new int[max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] != 'y' && data[0][0] != 'z' && data[0] != "samehospital: ")
                {
                    Console.WriteLine(variable.Name() + ": " + variable.SolutionValue());
                }

            }
        }
        static private int calculate_upperbound_time(OfflineProblem problem)
        {
            int max_time_upperbound = 0;
            foreach (Patient p in problem.patient_data)
            {
                max_time_upperbound = Math.Max(p.delay_between_doses + p.last_timeslot_first_dose + p.second_dose_interval + problem.gap + 1, max_time_upperbound);
            }
            return max_time_upperbound;
        }

        static private Variable[,] init_2d_boolean_variable_z(Solver solver, int j_max)
        {
            //z_j,j' is one if job j is before job j'
            Variable[,] z = new Variable[j_max, j_max];
            //fill z with valid variables inside the solvers context
            for (int i = 0; i < j_max; i++)
            {
                for (int j = 0; j < j_max; j++)
                {
                    z[i, j] = solver.MakeIntVar(0, 1, "z: " + i.ToString() + " " + j.ToString());
                }
            }
            return z;
        }

        static private Variable[,] init_2d_boolean_variable_y(Solver solver, int j_max, int h_max)
        {
            //y_j,h is one if job j is in hospital h
            Variable[,] y = new Variable[j_max, h_max];
            //fill y with valid variables inside the solvers context
            for (int j = 0; j < j_max; j++)
            {
                for (int h = 0; h < h_max; h++)
                {
                    y[j, h] = solver.MakeIntVar(0, 1, "y: " + j.ToString() + " " + h.ToString());
                }
            }
            return y;
        }

        static private Variable[,] init_2d_boolean_variable_samehospitals(Solver solver, int j_max)
        {
            //y_j,h is zero if j and j' in same hospital, one otherwise
            Variable[,] samehospitals = new Variable[j_max, j_max];
            //fill y with valid variables inside the solvers context
            for (int j = 0; j < j_max; j++)
            {
                for (int h = 0; h < j_max; h++)
                {
                    samehospitals[j, h] = solver.MakeIntVar(0, 1, "samehospital: " + j.ToString() + " " + h.ToString());
                }
            }
            return samehospitals;
        }

        static private Variable[] init_variables_vector(Solver solver, int length, int ub)
        {
            Variable[] vector = new Variable[length];
            for (int i = 0; i < length; i++)
            {
                vector[i] = solver.MakeIntVar(0, ub, i.ToString()); //TODO check if this can be relaxed
            }
            return vector;
        }

        static private void add_constraints_z(Solver solver, Variable[,] z, Variable[] t, List<Job> jobs, int max_t)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }

                    solver.Add(z[j, k] + z[k, j] <= 1); //Only 1 job can be before the other or they are at the same time //TODO check maybe change to <= 1
                    solver.Add(t[j] >= t[k] + 1 - (max_t + 1) * (z[j, k]));
                    solver.Add(t[k] >= t[j] + 1 - (max_t + 1) * (1 - z[j, k]));
                }
            }
        }
        static private void add_constraints_samehospital(Solver solver, Variable[,] samehospitals, Variable[,] y, List<Job> jobs, int max_h)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }
                    for (int h = 0; h < max_h; h++)
                    {
                        //bovenste: als allebei 0 dan 1, maar nog niet goed in andere gevallen.
                        //solver.Add(y[j, h] + y[k, h] - 1 <= samehospitals[j, k]); //samehospitals must be 0 when y[j,h] and y[k,h] are both 1
                        solver.Add(samehospitals[j, k] >= y[j, h]);
                        solver.Add(samehospitals[j, k] >= y[k, h]);
                        solver.Add(samehospitals[j, k] <= y[k, h] + y[j,h]);

                    }
                }
            }
        }

        static private void add_constraints_y(Solver solver, Variable[,] y, List<Job> jobs, int max_h)
        {

            for (int j = 0; j < jobs.Count; j++)
            {
                Constraint ct_one_hospital_per_job = solver.MakeConstraint(1, 1);

                for (int h = 0; h < max_h; h++)
                {
                    ct_one_hospital_per_job.SetCoefficient(y[j, h], 1);
                }
            }
        }

        static private void add_constraint_interval_vaccines(Solver solver, OfflineProblem problem, Variable[] t, List<Job> jobs)
        {
            foreach (Job j in jobs)
            {
                if (j.vaccine == 1)
                {
                    solver.Add(t[j.id] >= j.patient.first_timeslot_first_dose);
                    solver.Add(t[j.id] <= j.patient.last_timeslot_first_dose - problem.processing_time_first_dose + 1);
                }
                else if (j.vaccine == 2)
                {
                    solver.Add(t[j.id] >= t[jobs[j.id - 1].id] + problem.processing_time_first_dose - 1 + j.patient.delay_between_doses + problem.gap);
                    solver.Add(t[j.id] <= t[jobs[j.id - 1].id] + problem.processing_time_first_dose - 1 + j.patient.delay_between_doses + problem.gap + (j.patient.second_dose_interval - problem.processing_time_second_dose + 1));
                }
            }
        }


        static private void add_constraint_no_two_patients_at_the_same_time(Solver solver, OfflineProblem problem, Variable[] t, Variable[,] z, Variable[,] y, Variable[,] samehospitals, List<Job> jobs, int h_max, int t_max)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }


                    if (jobs[j].vaccine == 1)
                    {
                        //CONTROLEREN GOEIE Z
                        //MOET OOK CONSTRAINT VOOR ANDERSOM, DIE GELDIG IS ALS J' VOOR J

                        //solver.Add(t[j] - t[k] - (t_max + 1) * z[k, j] - (t_max + 1) * samehospitals[j, k] <= -1 - problem.processing_time_first_dose + 1);

                    }

                    else if (jobs[j].vaccine == 2)
                    {
                        //CONTROLEREN GOEIE Z
                       // solver.Add(t[j] - t[k] - (t_max + 1) * z[k, j] - (t_max + 1) * samehospitals[j, k] <= -1 - problem.processing_time_second_dose + 1);

                    }
                }
            }
        }
    }



    public class Job
    {
        public Patient patient;
        public int vaccine;
        public int id;



        public Job(Patient patient, int vaccine, int id)
        {
            this.patient = patient;
            this.vaccine = vaccine;
            this.id = id;
        }


        public override string ToString()
        {
            string part1 = "Patient: " + this.patient.ToString() + " ";
            string part2 = "Vaccine: " + this.vaccine + " ";
            string part3 = "ID: " + this.id + " ";
            return part1 + part2 + part3;
        }
    }
}