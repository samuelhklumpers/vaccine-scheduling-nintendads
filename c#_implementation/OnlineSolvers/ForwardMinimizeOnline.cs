using System;
using System.Collections.Generic;

namespace implementation
{
    class ForwardMinimizeOnline : IOnlineSolver
    {

        // FIXED L ISSUE.
        // FIXED + P1 ISSUE
        // ADDED SORTEDLIST to prevent "Sort".
        // CHANGED IT from Dose2D to Int. (should keep track of p1 / p2 though!)
        // FIXED D ISSUE.
        // FIXED P1,P2 ISSUE IN ASSIGNSCORESROW.

        // ADDED THE OPTION TO PUT ONLY 1 JAB IN A NEW HOSPITAL (before, he had to put both in).
        //   This is finnicky, as he does not care to e.g. put both jabs in H0, or both in H1.
        //   the only reason he puts them in 0, is because he checks hospitals in order {0,1,2,3..}
        //   and only picks a new best option based on >, not on >=. So.. bit finnicky, but should work.

        // after extensive testing of a specific case (back when he didn't add new hospital),
        //  it gave flawless answers until now.

        // This will definitely give run-time errors on large cases. It is already kinda slow now.
        // since... it has to consider ALL options, and makes a list the length of t_max for every h...
        // yeah, that takes some time.

        // I'm about to make it worse. Let's make this an= TRUE ONLINE ALG!



        // TODO: Optimize, check, test, comment.
        // Check = DONE DONE
        // Test = 1 case DONE, ......
        // Comment = mostly DONE.
        // Optimize = a bit DONE ......
        // Add the option to put only ONE in new hospital?

        public Solution2D Step(Solution2D sol, Patient p, Parameters problem)
        {
            // everything goes in here!

            bool writeline = false;

            
            // a list containing all (current) assignments.
            // points from START_TIME to P1 or P2.
            List<SortedList<int, int>> assignments = new List<SortedList<int, int>>();
            // add every starting_time in here!

            int h_max = 0;
            foreach(Doses2D d in sol.hospitals)
            {
                h_max = Math.Max(d.h1, d.h2);
                while(assignments.Count <= h_max)
                    assignments.Add(new SortedList<int, int>());
                assignments[d.h1][d.t1] = problem.p1;
                assignments[d.h2][d.t2] = problem.p2;
            }

            assignments.Add(new SortedList<int, int>()); // add a new hospital, such that the alg can place some there, if needed!

            h_max = assignments.Count; // the amount of hospitals used, +1 (a possible new one)


            // this way we can actually place them in a new one.
            // if this is not used, we delete it at the end again. The next patient will add one for themselves again.

            // for finding the maximum t_value.
            int max_ass = 0; // max of current assignments.
            foreach(Doses2D a in sol.hospitals)
                if(a.t2 + problem.p2 < max_ass)
                    max_ass = a.t2 + problem.p2;
            int max_pat = 0; // max of new assignment.

            // EXAMPLE: e.g. d1 = 4. Then 4 + p1 = 2 = 6. This is first place a new one can come.
            // add g, add x, and then, that is first time. L however cannot be <1, AND must be >= p2.
            // so the last place you can put J2 = NOT the last place in L!! It is + L - p2!!
            max_pat = p.d1 + problem.g + p.x + p.L;
            
            int t_max = Math.Max(max_ass, max_pat); // t_max is the maximum of both.
            if(writeline)
                Console.WriteLine("T_max: " + t_max);


            // default first. Then, overwrite one row, and write it back after?
            bool[,] scoresP1 = new bool[h_max,t_max + 1]; // if t_max = 13, that means i = 13. Means 14 elements!
            bool[,] scoresP2 = new bool[h_max,t_max + 1];
            // bytes would have worked too. 1 bool uses as much memory as 1 byte.

            if(writeline)
                Console.WriteLine("H_max: {0}", h_max);

            // start with every row default instantiated.
            for (int h = 0; h < h_max; h++)
            {
                assignScoresRow(ref scoresP1, assignments, h, problem.p1);
                assignScoresRow(ref scoresP2, assignments, h, problem.p2);
            }


            // FROM HERE,  THIS PART REMAINS UNCHANGED


            // now, go over all possible combinations of p1,p2.
            // check if they can be placed anyway.

            // OLD: assign him default the first possible place in a new hospital.
            // only when no assignment could be done, will this remain, and thus be placed.
            // Doses2D best_dose = new Doses2D(p.r1, h_max, p.r1 + problem.p1 + problem.g + p.x, h_max);

            Doses2D best_dose = new Doses2D(-1, -1, -1, -1);
            double best_score = -1; // automatically overwritten by any.

            for (int t1 = p.r1; t1 <= p.d1 - problem.p1 + 1; t1++)
            {
                // check whether can be placed in ANY hospital.
                for (int h1 = 0; h1 < h_max; h1++)
                {
                    // if can be placed there.
                    // luckily for us, we have the 'scores' list to tell us whether we can place there!
                    if(scoresP1[h1,t1])
                    {
                        // for every placement of the second jab
                        for (int t2 = t1 + problem.p1 + problem.g + p.x; t2 <= t1 + problem.p1 + problem.g + p.x + p.L - problem.p2; t2++)
                        {
                            for (int h2 = 0; h2 < h_max; h2++)
                            {
                                if(scoresP2[h2,t2])
                                {
                                    // we found a succesful placement for Jab1 and Jab2.

                                    // This is the 'forward' part - check the score again given
                                    //   that this pair is added.
                                    Dose2D new_dose1 = new Dose2D(t1, h1);
                                    Dose2D new_dose2 = new Dose2D(t2, h2);
                                    assignments[h1].Add(new_dose1.t, problem.p1);
                                    assignments[h2].Add(new_dose2.t, problem.p2);
                                    // assignments[h1].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t)); // really inefficient. For now, fine.
                                    // assignments[h2].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t));
                                    // sorting is needed for assigning scores to a row (assignScoresRow)
                                    //   that is the most efficient way of calculating it, rather (given an efficient sort).

                                    // do your calculations again, but only for the changed hospital(s).
                                    assignScoresRow(ref scoresP1, assignments, h1, problem.p1);
                                    assignScoresRow(ref scoresP1, assignments, h2, problem.p1);
                                    assignScoresRow(ref scoresP2, assignments, h1, problem.p2);
                                    assignScoresRow(ref scoresP2, assignments, h2, problem.p2);

                                    // calulate the new root = final score!
                                    // check whether this is larger than the largest until now. If so, keep.
                                    double new_score = calculateScore(scoresP1) + calculateScore(scoresP2);
                                    if(new_score > best_score)
                                    {
                                        best_score = new_score;
                                        best_dose = new Doses2D(t1, h1, t2, h2);
                                    }

                                    if(writeline)
                                        Console.WriteLine("Succesful pair found: {0} at h {1}, {2} at h {3}: SCORE = {4}..", t1, h1, t2, h2, new_score);


                                    // now throw them out again (the 'backward' or 'undo' phase), and re-calculate the rows back to old.
                                    assignments[h1].Remove(new_dose1.t);
                                    assignments[h2].Remove(new_dose2.t);
                                    assignScoresRow(ref scoresP1, assignments, h1, problem.p1);
                                    assignScoresRow(ref scoresP1, assignments, h2, problem.p1);
                                    assignScoresRow(ref scoresP2, assignments, h1, problem.p2);
                                    assignScoresRow(ref scoresP2, assignments, h2, problem.p2);
                                }
                            }
                        }
                    }
                }
            }
            // OLD: if no possible combination of assignments... add a new hospital.
            // the default 'best_score' was initiated to be placed in a new hospital, so we don't
            //   have to do anything else.

            // NEW: since we add a hospital from the start, the only way it is -1 is if no place fit..?
            if(best_score == -1)
                // assignments.Add(new SortedList<int, int>());
                throw new Exception("You seem to not be able to put one in any hospital...?");

            sol.hospitals.Add(best_dose);


            // AHHH. I am not adding them correctly. I need to re- and re-make a new SOLUTION2D!
            Solution2D new_sol = new Solution2D(h_max, sol.hospitals);
            return new_sol;
        }

        public Solution2D solve(OnlineProblem problem)
        {

            Solution2D sol = new Solution2D(0, new List<Doses2D>());
            foreach(Patient p in problem.patients) // this is the loop! Every patient...
            {
                Solution2D new_sol = Step(sol, p, problem.parameters); // TODO: This should be a variable function.
                sol = new_sol;
            }

            return sol;
        }

        public double calculateScore(bool[,] scores)
        {
            double v = 0;
            for (int t = 0; t < scores.GetLength(1); t++)
            {
                int r = 0;
                for (int h = 0; h < scores.GetLength(0); h++)
                    r += scores[h,t] ? 1 : 0;

                v += Math.Sqrt(r);
            }
            return v;
        }

        public void assignScoresRow(ref bool[,] scores, List<SortedList<int, int>> assignments, int h, int p) // the scores to fill, what row to fill, and the p1/p2.
        {
            int t;
            int ass_count = assignments[h].Count;

            if(ass_count == 0) // special case for empty assignments_list - you can put anywhere!
            {
                for (t = 0; t < scores.GetLength(1); t++)
                    scores[h,t] = true;
                return;
            }

            // If you want to completely overwrite, make sure to overwrite ALL values = write 0's too!
            for (t = 0; t < scores.GetLength(1); t++)
                scores[h,t] = false;

            var ass_times = assignments[h].Keys;
            var ass_ps = assignments[h].Values;

            // one case for 0 to first.
            for (t = 0; t <= ass_times[0] - p; t++)
                scores[h,t] = true;

            // ASSUME AFTER T_MAX IS STILL SPACE!

            // one case for final to t_max.
            int lastT = ass_times[ass_count - 1],    lastP = ass_ps[ass_count - 1];
            for (t = lastT + lastP; t < scores.GetLength(1); t++)
            // So I do need the doses. I need to know if this is a P1 or a P2!!
                scores[h,t] = true;

            if(ass_count == 1) // special case for 1-element assignments_list. (currently impossible, due to 2 doses per patient + cannot put 1 in new.)
                return;
            
            // the middle (if any)
            for (int a = 0; a < ass_count - 1; a++) // (current, next). -2 is last with a next!
            {
                int currentT = ass_times[a],   currentP = ass_ps[a];
                int nextT    = ass_times[a+1];
                for (t = currentT + currentP; t <= nextT - p; t++)
                    scores[h,t] = true;
            }
            
        }
    }
}