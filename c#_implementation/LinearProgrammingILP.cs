using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgrammingILP
    {
        public static Solution Solve(OfflineProblem problem)
        {
            //TOEVOEGEN: partial filled solution meegeven voor verder in de boom.

            List<Job> jobs = new List<Job>();

            for (int i = 0; i < problem.patients.Count; i++)
            {
                Patient patient = problem.patients[i];
                Job job1 = new Job(patient, 1, i * 2);
                Job job2 = new Job(patient, 2, i * 2 + 1);

                jobs.Add(job1);
                jobs.Add(job2);
            }

            Solver solver = new Solver("vaccine_scheduling", Solver.OptimizationProblemType.SCIP_MIXED_INTEGER_PROGRAMMING);
            int max_hospitals_upperbound = problem.patients.Count; //hospitals upperbound is when every patient gets its own hospital
            int max_time_upperbound = calculate_upperbound_time(problem); //time upperbound is from 0 until the end of the last 2nd dose interval

            //Create variable for number of hospitals.
            //Variable numHospitals = solver.MakeNumVar(0.0, max_hospitals_upperbound, "numHospitals");

            int max_j = jobs.Count;
            int max_h = max_hospitals_upperbound;
            int max_t = max_time_upperbound;

            //Create variable lists as each patient has that variable
            Variable[] t = init_variables_vector(solver, max_j, max_t, "t"); // tj starting time of job j
            Variable[,] z = init_2d_boolean_variable_z(solver, max_j); // z_j,j' --> 1 if job j starts before job j'
            Variable[,] samehospitals = init_2d_boolean_variable_samehospitals(solver, max_j); //one if jobs j,k in same hospital
            //Variable sameHospitalsSum = solver.MakeIntVar(0, max_j*max_j, "Hospital_sum");

            /*Constraint hospital_sum_constraint = solver.MakeConstraint(0, max_j*max_j);    
            for(int i = 0; i < max_j; i++){
                for(int j = 0; j < max_j; j++){
                     hospital_sum_constraint.SetCoefficient(samehospitals[i,j], 1);
                }   
            }*/

            //Add the constraints to the solver
            add_constraints_z(solver, z, t, jobs, max_t); // calculate which jobs are before other jobs
            //add_constraints_y(solver, y, jobs, max_h); //
            add_constraint_interval_vaccines(solver, problem, t, jobs);
            add_constraint_no_two_patients_at_the_same_time(solver, problem, t, z, samehospitals, jobs, max_h, max_t);

            Console.WriteLine("Number of variables = " + solver.NumVariables());
            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals.
            //solver.Maximize(sameHospitalsSum);

            Objective objective = solver.Objective();
            for (int i = 0; i < max_j; i++)
            {
                for (int j = 0; j < max_j; j++)
                {
                    objective.SetCoefficient(samehospitals[i, j], 1);
                }
            }
            objective.SetMaximization();
            solver.SetTimeLimit(1);
            solver.Solve();
            //Console.WriteLine("Solution:");


            int[] hospital_numbers = new int[max_j];
            List<(int, int, int)> chronological_jobs = new List<(int, int, int)>();
            for (int i = 0; i < solver.variables().Count; i++)
            {
                string[] data = solver.variables()[i].Name().Split(' ');
                if (data[0][0] == 't')
                {
                    chronological_jobs.Add(((int)solver.variables()[i].SolutionValue(), i % 2, i)); //fill list with start times and 0 when first dose, 1 when second dose, and job id
                }
            }
            List<(int, int, int)> chronological_jobs_copy = new List<(int, int, int)>(chronological_jobs);
            chronological_jobs.Sort();
            int[] hospital_available = new int[max_h];
            int done_job_index = 0;
            for (int i = 0; i < max_t; i++)
            {
                if (done_job_index >= chronological_jobs.Count) break;
                if (chronological_jobs[done_job_index].Item1 == i)
                {
                    for (int j = 0; j < hospital_available.Length; j++)
                    {
                        if (hospital_available[j] == 0)
                        {
                            hospital_available[j] = chronological_jobs[done_job_index].Item2 == 0 ? problem.p1 : problem.p2;
                            hospital_numbers[chronological_jobs[done_job_index].Item3] = j;
                            break;
                        }
                    }
                    done_job_index++;
                }
                for (int j = 0; j < hospital_available.Length; j++)
                {
                    hospital_available[j] = Math.Max(0, hospital_available[j] - 1);
                }
            }
            List<Doses> registrations = new List<Doses>();
            for (int i = 0; i < hospital_numbers.Length / 2; i++)
            {
                registrations.Add(new Doses(chronological_jobs_copy[i * 2].Item1, chronological_jobs_copy[i * 2 + 1].Item1));
            }
            Solution sol = new Solution(hospital_numbers.Max() + 1, registrations);
            /*Console.WriteLine(sol);





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

            Console.WriteLine();
            Console.WriteLine("mat t:");
            int[] resulting_vector_t = new int[max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 't')
                {
                    Console.WriteLine(variable.Name() + ": " + variable.SolutionValue());
                }

            }

            Console.WriteLine("mat samehospitals:");
            int[,] resulting_matrix_sh = new int[max_j, max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] != 'z' && data[0][0] != 'y' && data[0][0] != 't' && data[0][0] != 'H')
                {
                    resulting_matrix_sh[int.Parse(data[1]), int.Parse(data[2])] = (int)variable.SolutionValue();
                }
            }
            for (int i = 0; i < resulting_matrix_sh.Length; i++)
            {
                if (i % max_j == 0)
                {
                    Console.WriteLine();
                }
                Console.Write(resulting_matrix_sh[i / max_j, i % max_j] + " ");

            }
            Console.WriteLine();*/
            return sol;
            //Console.WriteLine("same hospital sum: " + sameHospitalsSum.SolutionValue());
        }
        static private int calculate_upperbound_time(OfflineProblem problem)
        {
            int max_time_upperbound = 0;
            foreach (Patient p in problem.patients)
            {
                max_time_upperbound = Math.Max(p.x + p.d1 + p.L + problem.g + 1, max_time_upperbound);
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

        static private Variable[] init_variables_vector(Solver solver, int length, int ub, String name)
        {
            Variable[] vector = new Variable[length];
            for (int i = 0; i < length; i++)
            {
                vector[i] = solver.MakeIntVar(0, ub, name + i.ToString()); //TODO check if this can be relaxed
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

                    //solver.Add(z[j, k] + z[k, j] == 1); //Only 1 job can be before the other or they are at the same time //TODO check maybe change to <= 1
                    solver.Add(t[j] >= t[k] + 1 - (max_t + 1) * (z[j, k]));
                    solver.Add(t[k] >= t[j] + 1 - (max_t + 1) * (1 - z[j, k]));
                }
            }
        }

        static private void add_constraint_interval_vaccines(Solver solver, OfflineProblem problem, Variable[] t, List<Job> jobs)
        {
            foreach (Job j in jobs)
            {
                if (j.vaccine == 1)
                {
                    solver.Add(t[j.id] >= j.patient.r1);
                    solver.Add(t[j.id] <= j.patient.d1 - problem.p1 + 1);
                }
                else if (j.vaccine == 2)
                {
                    solver.Add(t[j.id] >= t[jobs[j.id - 1].id] + problem.p1 + j.patient.x + problem.g);
                    solver.Add(t[j.id] <= t[jobs[j.id - 1].id] + problem.p1 + j.patient.x + problem.g + (j.patient.L - problem.p2 + 1));
                }
            }
        }


        static private void add_constraint_no_two_patients_at_the_same_time(Solver solver, OfflineProblem problem, Variable[] t, Variable[,] z, Variable[,] samehospitals, List<Job> jobs, int h_max, int t_max)
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

                        solver.Add(t[j] - t[k] - (t_max + 1) * (1 - z[j, k]) - (t_max + 1) * (1 - samehospitals[j, k]) <= -problem.p1);

                    }

                    else if (jobs[j].vaccine == 2)
                    {
                        //CONTROLEREN GOEIE Z
                        solver.Add(t[j] - t[k] - (t_max + 1) * (1 - z[j, k]) - (t_max + 1) * (1 - samehospitals[j, k]) <= -problem.p2);

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