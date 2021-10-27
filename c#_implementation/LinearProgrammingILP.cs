using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class LinearProgrammingILP
    {
        public static (bool, bool, int?, Solution) Solve(OfflineProblem problem, Dictionary<string, double> partial_solution, int timelimit)
        {
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

            int max_j = jobs.Count;
            int max_h = max_hospitals_upperbound;
            int max_t = max_time_upperbound;

            //Create the decision variables
            Variable[] t = init_variables_vector(solver, max_j, max_t, "t"); // tj starting time of job j
            Variable[,] z = init_2d_boolean_variable_z(solver, max_j); // z_j,j' --> 1 if job j starts before job j'
            Variable[,] s = init_2d_boolean_variable_s(solver, max_j); //one if jobs j,k in same hospital

            //Add the constraints to the solver
            add_constraints_z(solver, z, t, jobs, max_t); // calculate which jobs are before other jobs
            add_constraint_interval_vaccines(solver, problem, t, jobs);  // Make sure the two doses of one patient are in the correct interval
            add_constraint_no_two_patients_at_the_same_time(solver, problem, t, z, s, jobs, max_h, max_t); // makes sure that there are no patients planned at the same time in the same hospital.


            foreach (var item in partial_solution)
            {
                string variable_string = item.Key;
                double value = item.Value;

                var variable = solver.LookupVariableOrNull(variable_string);

                if (variable == null)
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
            for (int i = 0; i < max_j; i++)
            {
                for (int j = 0; j < max_j; j++)
                {
                    objective.SetCoefficient(s[i, j], 1);
                }
            }
            objective.SetMaximization();
            solver.SetTimeLimit(timelimit);

            Solver.ResultStatus status = solver.Solve();


            //Decide the status of the ILP after the timelimit is done. Then we can see whether a solution has been found and whether it is optimal.
            bool feasibleNoSolution = false;
            bool someSolution = false;
            int? upperboundHospitals = null;
            Solution sol = null;


            if (status == Solver.ResultStatus.OPTIMAL)
            {
                someSolution = true;
                (int upperbound, Solution solution) = calculate_upperbound_and_solution_ilp(solver, problem, max_j, max_h);
                sol = solution;
                upperboundHospitals = upperbound;
            }

            else if (status == Solver.ResultStatus.NOT_SOLVED)
            {
                feasibleNoSolution = true;
                someSolution = false;

            }

            else if (status == Solver.ResultStatus.FEASIBLE)
            {
                feasibleNoSolution = false;
                someSolution = true;
                (int upperbound, Solution solution) = calculate_upperbound_and_solution_ilp(solver, problem, max_j, max_h);
                sol = solution;
                upperboundHospitals = upperbound;
            }

            else
            {
                feasibleNoSolution = false;
                someSolution = false;
            }

            return (feasibleNoSolution, someSolution, upperboundHospitals, sol);

        }
        //Creates the solution with assigned hospitals from the solved ILP
        static private (int, Solution) calculate_upperbound_and_solution_ilp(Solver solver, OfflineProblem problem, int max_j, int max_h)
        {
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
            int current_time = 0;
            int curent_max = 0;
            foreach ((int, int, int) job in chronological_jobs)
            {
                for (int i = 0; i < hospital_available.Length; i++)
                {
                    hospital_available[i] = Math.Max(0, hospital_available[i] - (job.Item1 - current_time));
                }
                current_time = job.Item1;
                for (int i = 0; i < hospital_available.Length; i++)
                {
                    if (hospital_available[i] == 0)
                    {
                        hospital_available[i] = job.Item2 == 0 ? problem.p1 : problem.p2;
                        hospital_numbers[job.Item3] = i;
                        break;
                    }
                }

                int max = 0;
                for (int i = 0; i < hospital_available.Length; i++)
                {
                    max += hospital_available[i] != 0 ? 1 : 0;
                }
                if (max > curent_max)
                {
                    curent_max = max;
                }
            }
            List<Doses> registrations = new List<Doses>();
            for (int i = 0; i < hospital_numbers.Length / 2; i++)
            {
                registrations.Add(new Doses(chronological_jobs_copy[i * 2].Item1, chronological_jobs_copy[i * 2 + 1].Item1));
            }
            Solution sol = new Solution((hospital_numbers.Length > 0) ? hospital_numbers.Max() + 1 : 0, registrations);
            return (curent_max, sol);
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

        static private Variable[,] init_2d_boolean_variable_s(Solver solver, int j_max)
        {
            //samehospitals[j,j'] is zero if j and j' in same hospital, one otherwise
            Variable[,] s = new Variable[j_max, j_max];
            //fill samehospitals with valid variables inside the solvers context
            for (int j = 0; j < j_max; j++)
            {
                for (int h = 0; h < j_max; h++)
                {
                    s[j, h] = solver.MakeIntVar(0, 1, "samehospital: " + j.ToString() + " " + h.ToString());
                }
            }
            return s;
        }

        //Creates a vector with variables that must have a value between 0 and a given upperbound.
        static private Variable[] init_variables_vector(Solver solver, int length, int ub, String name)
        {
            Variable[] vector = new Variable[length];
            for (int i = 0; i < length; i++)
            {
                vector[i] = solver.MakeIntVar(0, ub, name + i.ToString()); 
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
                    //Constraints that set z[j,k] to one if job j starts before job k. 
                    solver.Add(t[j] >= t[k] + 1 - (max_t + 1) * (z[j, k]));
                    solver.Add(t[k] >= t[j] - (max_t + 1) * (1 - z[j, k]));
                }
            }
        }

        //Adds constraints for each job to ensure the job is scheduled in the correct interval.
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
                    solver.Add(t[j.id] <= t[jobs[j.id - 1].id] + problem.p1 + j.patient.x + problem.g + (j.patient.L - 1 - problem.p2 + 1));
                }
            }
        }


        static private void add_constraint_no_two_patients_at_the_same_time(Solver solver, OfflineProblem problem, Variable[] t, Variable[,] z, Variable[,] s, List<Job> jobs, int h_max, int t_max)
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
                        solver.Add(t[j] - t[k] - (t_max + 1) * (1 - z[j, k]) - (t_max + 1) * (1 - s[j, k]) <= -problem.p1);
                    }

                    else if (jobs[j].vaccine == 2)
                    {
                        solver.Add(t[j] - t[k] - (t_max + 1) * (1 - z[j, k]) - (t_max + 1) * (1 - s[j, k]) <= -problem.p2);
                    }
                }
            }
        }
    }
}