using System;
using System.Collections.Generic;

namespace implementation
{
    class ForwardMinimizeOnline : IOnlineSolver
    {

        // For now, don't consider the option of putting either p1 or p2 in a new hospital.
        // Only put them both in when no combination of them can be in old ones.

        // I think I'm done... how do I check this?

        // FIXED L ISSUE.
        // FIXED + P1 ISSUE
        // ADDED SORTEDLIST to prevent "Sort".
        // CHANGED IT from Dose2D to Int. (should keep track of p1 / p2 though!)
        // FIXED D ISSUE.
        // FIXED P1,P2 ISSUE IN ASSIGNSCORESROW.

        //   after extensive testing of a specific case, it gives flawless answers until now.



        // TODO: Optimize, check, test, comment.
        // Check = DONE DONE
        // Test = 1 case DONE, ......
        // Comment = mostly DONE.
        // Optimize = a bit DONE ......
        // Add the option to put only ONE in new hospital? TODO


        public Solution2D solve(OnlineProblem problem)
        {

            // a list containing all (current) assignments.
            // points from START_TIME to P1 or P2.
            List<SortedList<int, int>> assignments = new List<SortedList<int, int>>();
            assignments.Add(new SortedList<int, int>());
            List<Doses2D> final_assignments = new List<Doses2D>();
            
            foreach(Patient p in problem.patients) // this is the loop! Every patient...
            {
                // for finding the maximum t_value.
                int max_ass = 0; // max of current assignments.
                foreach(Doses2D a in final_assignments)
                    if(a.t2 + problem.p2 < max_ass)
                        max_ass = a.t2 + problem.p2;
                int max_pat = 0; // max of new assignment.

                // Recall: e.g. d1 = 4. Then 4 + p1 = 2 = 6. This is first place a new one can come.
                // add g, add x, and then, that is first time. L however cannot be <1, AND must be >= p2.
                // so the last place you can put J2 = NOT the last place in L!! It is + L - p2!!
                max_pat = p.d1 + problem.g + p.x + p.L;
                
                int t_max = Math.Max(max_ass, max_pat); // t_max is the maximum of both.
                Console.WriteLine("T_max: " + t_max);

                int h_max = assignments.Count;

                // default first. Then, overwrite one row, and write it back after?
                bool[,] scoresP1 = new bool[h_max,t_max + 1]; // if t_max = 13, that means i = 13. Means 14 elements!
                bool[,] scoresP2 = new bool[h_max,t_max + 1];
                // bytes would have worked too. 1 bool uses as much memory as 1 byte.

                Console.WriteLine("H_max: {0}", h_max);

                // start with every row default instantiated.
                for (int h = 0; h < h_max; h++)
                {
                    assignScoresRow(ref scoresP1, assignments, h, problem.p1);
                    assignScoresRow(ref scoresP2, assignments, h, problem.p2);
                }

                Console.WriteLine("Default scores assigned!");

                // now, go over all possible combinations of p1,p2.
                // check if they can be placed anyway.

                // assign him default the first possible place in a new hospital.
                // only when no assignment could be done, will this remain, and thus be placed.
                Doses2D best_dose = new Doses2D(p.r1, h_max, p.r1 + problem.p1 + problem.g + p.x, h_max);
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

                                        Console.WriteLine("Succesful pair found: {0} at h {1}, {2} at h {3}: SCORE = {4}..", t1, h2, t2, h2, new_score);


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
                // if no possible combination of assignments... add a new hospital.
                // the default 'best_score' was initiated to be placed in a new hospital, so we don't
                //   have to do anything else.
                if(best_score == -1)
                    assignments.Add(new SortedList<int, int>());

                final_assignments.Add(best_dose);

                // now add them for real.
                Dose2D d1 = new Dose2D(best_dose.t1, best_dose.h1);
                Dose2D d2 = new Dose2D(best_dose.t2, best_dose.h2);
                assignments[d1.h].Add(d1.t, problem.p1);
                assignments[d2.h].Add(d2.t, problem.p2);
                // assignments[d1.h].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t)); // not needed if added to new hospital, but can't hurt.
                // assignments[d2.h].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t));
            }

            // if all patients are done, we are finished.
            Solution2D sol = new Solution2D(assignments.Count, final_assignments);
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