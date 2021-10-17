using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class IntegerLinearProgramming
    {
        public static void Solve(OfflineProblem problem, Dictionary<string, double> partial_solution)
        {
            List<Job2> jobs = new List<Job2>();

            //Create the jobs, each job is one vaccine. 
            for (int i = 0; i < problem.patient_data.Count; i++)
            {
                Patient patient = problem.patient_data[i];
                Job2 job1 = new Job2(patient, 1, i * 2);
                Job2 job2 = new Job2(patient, 2, i * 2 + 1);

                jobs.Add(job1);
                jobs.Add(job2);
            }

            Solver solver = new Solver("vaccine_scheduling", Solver.OptimizationProblemType.SCIP_MIXED_INTEGER_PROGRAMMING);

            int max_hospitals_upperbound = problem.patient_data.Count; //hospitals upperbound is when every patient gets its own hospital
            int max_time_upperbound = calculate_upperbound_time(problem); //time upperbound is from 0 until the end of the last 2nd dose interval

            int max_j = jobs.Count;
            int max_h = max_hospitals_upperbound;
            int max_t = max_time_upperbound;

            //Create the decision variables
            Variable[] t = init_variables_vector(solver, max_j, max_t, "t"); // tj starting time of job j
            Variable[,] z = init_2d_boolean_variable_z(solver, max_j); // z_j,j' --> 1 if job j starts before job j'
            Variable[] y = init_variables_vector(solver, max_j, max_h-1, "y"); // y_j --> hospital number of job j
            Variable[,] samehospitals = init_2d_boolean_variable_samehospitals(solver, max_j); //one if jobs j and k in same hospital
            Variable[,] compare = init_2d_boolean_variable_compare(solver, max_j); //Compare[j,k] is zero if y[j] is bigger than y[k] and one otherwise, used together with sameHospitals.

            //Add the constraints to the solver
            add_constraints_z(solver, z, t, jobs, max_t); // calculate which jobs are before other jobs
            add_constraints_samehospital(solver, samehospitals, y, compare, jobs, max_h); //Used to check whether the patients are in the same hospital
            add_constraint_interval_vaccines(solver, problem, t, jobs); //Makes sure the two doses of one patient are in the correct interval
            add_constraint_no_two_patients_at_the_same_time(solver, problem, t, z, y, samehospitals, jobs, max_h, max_t); //To make sure that there are no patients planned at the same time in the same hospital.
            add_constraints_compare(solver, compare, y, jobs, max_h); //Used to set sameHospitals.

            //CODE RUNS BUT RESULTS IN INFEASIBLE PROBLEM.
            foreach (var item in partial_solution)
            {
                string variable_string = item.Key;
                double value = item.Value;

                var variable = solver.LookupVariableOrNull(variable_string);

                if(variable == null)
                {
                    Console.WriteLine("No variable with name: " + variable_string);
                    continue;
                }

                solver.Add(variable == value);


            }

            Console.WriteLine("Number of variables = " + solver.NumVariables());
            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals by maximizing the number of equal hospitals among the different jobs.
            Objective objective = solver.Objective();
            for(int i = 0; i < max_j; i++){
                for(int j = 0; j < max_j; j++){
                     objective.SetCoefficient(samehospitals[i,j], 1);
                }   
            }
            objective.SetMaximization();


            solver.Solve();

            Console.WriteLine("Solution:");

            //Print the solutions variables as an array
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

            Console.WriteLine("mat compare:");
            int[,] resulting_matrix_compare = new int[max_j, max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 'c')
                {
                    resulting_matrix_compare[int.Parse(data[1]), int.Parse(data[2])] = (int)variable.SolutionValue();
                }
            }
            for (int i = 0; i < resulting_matrix_compare.Length; i++)
            {
                if (i % max_j == 0)
                {
                    Console.WriteLine();
                }
                Console.Write(resulting_matrix_compare[i / max_j, i % max_j] + " ");

            }
            Console.WriteLine();

            Console.WriteLine("mat y:");
            HashSet<int> hospitals_used = new HashSet<int>();
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 'y' )
                {
                    Console.WriteLine(variable.Name() + ": " + variable.SolutionValue());

                    hospitals_used.Add((int)variable.SolutionValue());
                    
                }

            }
            Console.WriteLine();
            Console.WriteLine("mat t:");
            int[] resulting_vector_t = new int[max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] == 't' )
                {
                    Console.WriteLine(variable.Name() + ": " + variable.SolutionValue());
                }

            }

            Console.WriteLine("mat samehospitals:");
            int[,] resulting_matrix_sh = new int[max_j, max_j];
            foreach (var variable in solver.variables())
            {
                string[] data = variable.Name().Split(' ');
                if (data[0][0] != 'z' && data[0][0] != 'y' && data[0][0] != 't' &&data[0][0] != 'H')
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
            Console.WriteLine();

            Console.WriteLine("Number of hospitals used: " + hospitals_used.Count());
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

        static private Variable[,] init_2d_boolean_variable_compare(Solver solver, int j_max)
        {
            //compare[j,j'] is one if job j is in a lower or equal numbered hospital as job j'
            Variable[,] compare = new Variable[j_max, j_max];
            //fill compare with valid variables inside the solvers context
            for (int i = 0; i < j_max; i++)
            {
                for (int j = 0; j < j_max; j++)
                {
                    compare[i, j] = solver.MakeIntVar(0, 1, "compare: " + i.ToString() + " " + j.ToString());
                }
            }
            return compare;
        }

        static private Variable[,] init_2d_boolean_variable_samehospitals(Solver solver, int j_max)
        {
            //samehospitals[j,j'] is zero if j and j' in same hospital, one otherwise
            Variable[,] samehospitals = new Variable[j_max, j_max];
            //fill samehospitals with valid variables inside the solvers context
            for (int j = 0; j < j_max; j++)
            {
                for (int h = 0; h < j_max; h++)
                {
                    samehospitals[j, h] = solver.MakeIntVar(0, 1, "samehospital: " + j.ToString() + " " + h.ToString());
                }
            }
            return samehospitals;
        }

        //Creates a vector with variables that must have a value between 0 and a given upperbound.
        static private Variable[] init_variables_vector(Solver solver, int length, int ub, String name)
        {
            Variable[] vector = new Variable[length];
            for (int i = 0; i < length; i++)
            {
                vector[i] = solver.MakeIntVar(0, ub, name + i.ToString()); //TODO check if this can be relaxed
            }
            return vector;
        }


        static private void add_constraints_z(Solver solver, Variable[,] z, Variable[] t, List<Job2> jobs, int max_t)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }
                    //Constraints that set z[j,k] to one if job j starts before job k. 
                    solver.Add(t[j] >= t[k] + 1 - (max_t + 1) * (z[j, k]));
                    solver.Add(t[k] >= t[j] + 1 - (max_t + 1) * (1 - z[j, k]));
                }
            }
        }

        //Add the constraint for each job pair that Compare needs to be one if y[j] is smaller than y[k].
        //It also ensures that if y[j] and y[k] are equal, than compare[j,k] needs to be zero.
        static private void add_constraints_compare(Solver solver, Variable[,] compare, Variable[] y, List<Job2> jobs, int max_h)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }

                    //if compare needs to be zero because samehospitals is zero, y[j] and y[k] need to be different.
                    solver.Add(y[j] >= y[k] + 1 - (max_h + 1) * (compare[j, k]));
                    
                }
            }
        }
        static private void add_constraints_samehospital(Solver solver, Variable[,] samehospitals, Variable[] y, Variable[,] compare, List<Job2> jobs, int max_h)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }

                    //sameHospitals is one if both y[j] and y[k] are equal. 
                    solver.Add(compare[j,k] + compare[k,j] <= samehospitals[j,k] + 1); //if samehospitals is zero, one of the compares needs to be zero
                    solver.Add((y[j] - y[k]) + (max_h + 1) >= (max_h + 1) * samehospitals[j, k]); //if same hospital is one, y[j] and y[k] need to be the same
                    solver.Add((y[k] - y[j]) + (max_h + 1) >= (max_h + 1) * samehospitals[j, k]); //need this one for above constraint to hold, otherwise can also be one
                    // if y[k] smaller than y[j]. --> this minimizes the number of hospitals as we maximize samehospitals. 

                }
            }
        }

        //Adds constraints for each job to ensure the job is scheduled in the correct interval.
        static private void add_constraint_interval_vaccines(Solver solver, OfflineProblem problem, Variable[] t, List<Job2> jobs)
        {
            foreach (Job2 j in jobs)
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


        static private void add_constraint_no_two_patients_at_the_same_time(Solver solver, OfflineProblem problem, Variable[] t, Variable[,] z, Variable[] y, Variable[,] samehospitals, List<Job2> jobs, int h_max, int t_max)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                for (int k = 0; k < jobs.Count; k++)
                {
                    if (jobs[j].id == jobs[k].id) // Don't add the constraints for the same job as this will be infeasible
                    {
                        continue;
                    }

                    //Ensures that no job is scheduled within the processing time of a previous job that is in the same hospital. 
                    if (jobs[j].vaccine == 1)
                    {
                        solver.Add(t[j] - t[k] - (t_max + 1) * (1-z[j,k]) - (t_max + 1) * (1-samehospitals[j, k]) <= - problem.processing_time_first_dose);

                    }

                    else if (jobs[j].vaccine == 2)
                    {
                        solver.Add(t[j] - t[k] - (t_max + 1) * (1-z[j,k]) - (t_max + 1) * (1-samehospitals[j, k]) <= - problem.processing_time_second_dose);
                    }
                }
            }
        }
    }



    public class Job2
    {
        public Patient patient;
        public int vaccine;
        public int id;



        public Job2(Patient patient, int vaccine, int id)
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