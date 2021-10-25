using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace implementation
{
    class IntegerLinearProgramming
    {
        //First boolean is true if the ILP is feasible but no solution is found.        
        //Second boolean is true if the ILP found some solution. 
        //int is the upperbound if a suboptimal solution is found. Is null if infeasible or no solution
        //Solution is not null if an optimal solution is found. (thus done within time)
        public static (bool, bool, int?, Solution) Solve(OfflineProblem problem, Dictionary<string, double> partial_solution, int timeLimit)
        {
            List<Job> jobs = new List<Job>();

            //Create the jobs, each job is one vaccine. 
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
            Variable[] y = init_variables_vector(solver, max_j, max_h - 1, "y"); // y_j --> hospital number of job j
            Variable[,] samehospitals = init_2d_boolean_variable_samehospitals(solver, max_j); //one if jobs j and k in same hospital
            Variable[,] compare = init_2d_boolean_variable_compare(solver, max_j); //Compare[j,k] is zero if y[j] is bigger than y[k] and one otherwise, used together with sameHospitals.

            //Add the constraints to the solver
            add_constraints_z(solver, z, t, jobs, max_t); // calculate which jobs are before other jobs
            add_constraints_samehospital(solver, samehospitals, y, compare, jobs, max_h); //Used to check whether the patients are in the same hospital
            add_constraint_interval_vaccines(solver, problem, t, jobs); //Makes sure the two doses of one patient are in the correct interval
            add_constraint_no_two_patients_at_the_same_time(solver, problem, t, z, y, samehospitals, jobs, max_h, max_t); //To make sure that there are no patients planned at the same time in the same hospital.
            add_constraints_compare(solver, compare, y, jobs, max_h); //Used to set sameHospitals.

            //Add extra constraints to comply to the partial solution given by branch and bound.
            foreach (var item in partial_solution)
            {
                string variable_string = item.Key;
                double value = item.Value;

                var variable = solver.LookupVariableOrNull(variable_string);

                if (variable == null)
                {
                    //Console.WriteLine("No variable with name: " + variable_string);
                    continue;
                }

                solver.Add(variable == value);
            }

            //Console.WriteLine("Number of variables = " + solver.NumVariables());
            //Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, minimizing the number of hospitals by maximizing the number of equal hospitals among the different jobs.
            Objective objective = solver.Objective();
            for (int i = 0; i < max_j; i++)
            {
                for (int j = 0; j < max_j; j++)
                {
                    objective.SetCoefficient(samehospitals[i, j], 1);
                }
            }
            objective.SetMaximization();
            solver.SetTimeLimit(timeLimit);

            Solver.ResultStatus status = solver.Solve();

            bool feasibleNoSolution = false;
            bool someSolution = false;
            int? upperboundHospitals = null;
            Solution sol = null;


            if (status == Solver.ResultStatus.OPTIMAL)
            {
                someSolution = true;

                List<Doses2D> doses = new List<Doses2D>();
                HashSet<int> hospitals_used = new HashSet<int>();


                for (int i = 0; i < problem.patients.Count; i++)
                {
                    Patient patient = problem.patients[i];

                    var firstHospital = solver.LookupVariableOrNull("y" + (i * 2));
                    var secondHospital = solver.LookupVariableOrNull("y" + (i * 2 + 1));
                    var firstTime = solver.LookupVariableOrNull("t" + (i * 2));
                    var secondTime = solver.LookupVariableOrNull("t" + (i * 2 + 1));

                    hospitals_used.Add((int)firstHospital.SolutionValue());
                    hospitals_used.Add((int)secondHospital.SolutionValue());

                    Doses2D dose = new Doses2D((int)firstTime.SolutionValue(), (int)firstHospital.SolutionValue(), (int)secondTime.SolutionValue(), (int)secondHospital.SolutionValue());

                    doses.Add(dose);
                }

                upperboundHospitals = hospitals_used.Count;

                sol = new Solution2D(hospitals_used.Count, doses);

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

                List<Doses2D> doses = new List<Doses2D>();
                HashSet<int> hospitals_used = new HashSet<int>();


                for (int i = 0; i < problem.patients.Count; i++)
                {
                    Patient patient = problem.patients[i];

                    var firstHospital = solver.LookupVariableOrNull("y" + (i * 2));
                    var secondHospital = solver.LookupVariableOrNull("y" + (i * 2 + 1));
                    var firstTime = solver.LookupVariableOrNull("t" + (i * 2));
                    var secondTime = solver.LookupVariableOrNull("t" + (i * 2 + 1));

                    hospitals_used.Add((int)firstHospital.SolutionValue());
                    hospitals_used.Add((int)secondHospital.SolutionValue());

                    Doses2D dose = new Doses2D((int)firstTime.SolutionValue(), (int)firstHospital.SolutionValue(), (int)secondTime.SolutionValue(), (int)secondHospital.SolutionValue());

                    doses.Add(dose);
                }

                upperboundHospitals = hospitals_used.Count;

                sol = new Solution2D(hospitals_used.Count, doses);
            }

            else
            {
                feasibleNoSolution = false;
                someSolution = false;
            }

            return (feasibleNoSolution, someSolution, upperboundHospitals, sol);
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
                    //Constraints that set z[j,k] to one if job j starts before job k or at the same time. 
                    solver.Add(t[j] >= t[k] + 1 - (max_t + 1) * (z[j, k]));
                    solver.Add(t[k] >= t[j] - (max_t + 1) * (1 - z[j, k]));
                }
            }
        }

        //Add the constraint for each job pair that Compare needs to be one if y[j] is smaller than y[k].
        //It also ensures that if y[j] and y[k] are equal, than compare[j,k] needs to be zero.
        static private void add_constraints_compare(Solver solver, Variable[,] compare, Variable[] y, List<Job> jobs, int max_h)
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
        static private void add_constraints_samehospital(Solver solver, Variable[,] samehospitals, Variable[] y, Variable[,] compare, List<Job> jobs, int max_h)
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
                    solver.Add(compare[j, k] + compare[k, j] <= samehospitals[j, k] + 1); //if samehospitals is zero, one of the compares needs to be zero
                    solver.Add((y[j] - y[k]) + (max_h + 1) >= (max_h + 1) * samehospitals[j, k]); //if same hospital is one, y[j] and y[k] need to be the same
                    solver.Add((y[k] - y[j]) + (max_h + 1) >= (max_h + 1) * samehospitals[j, k]); //need this one for above constraint to hold, otherwise can also be one
                    // if y[k] smaller than y[j]. --> this minimizes the number of hospitals as we maximize samehospitals. 

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
                    solver.Add(t[j.id] <= t[jobs[j.id - 1].id] + problem.p1 + j.patient.x + problem.g + j.patient.L - 1 - problem.p2 + 1);
                }
            }
        }


        static private void add_constraint_no_two_patients_at_the_same_time(Solver solver, OfflineProblem problem, Variable[] t, Variable[,] z, Variable[] y, Variable[,] samehospitals, List<Job> jobs, int h_max, int t_max)
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
}