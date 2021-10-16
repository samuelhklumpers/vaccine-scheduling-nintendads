using System;
using System.Collections.Generic;

namespace implementation
{
    class ForwardMinimizeOnline : IOnlineSolver
    {

        // For now, don't consider the option of putting either p1 or p2 in a new hospital.
        // Only put them both in when no combination of them can be in old ones.

        // I think I'm done... how do I check this?

        public Solution2D solve(OnlineProblem problem)
        {

            // a list containing all (current) assignments.
            List<List<Dose2D>> assignments = new List<List<Dose2D>>();
            assignments.Add(new List<Dose2D>());
            List<Doses2D> final_assignments = new List<Doses2D>();
            
            foreach(Patient p in problem.patients) // this is the loop! Every patient...
            {
                // for finding the maximum t_value.
                int max_ass = 0; // max of current assignments.
                foreach(Doses2D a in final_assignments)
                    if(a.t2 + problem.p2 < max_ass)
                        max_ass = a.t2 + problem.p2;
                int max_pat = 0; // max of new assignment.
                max_pat = p.d1 + problem.p1 + problem.g + p.x + p.L + problem.p2 + 1; // TODO: EVERYWHERE +1?
                Console.WriteLine("curr ass: " + max_pat);
                Console.WriteLine("prev ass: " + max_ass);
                
                int t_max = Math.Max(max_ass, max_pat); // t_max is the maximum of both.
                Console.WriteLine("T_max: " + t_max);

                int h_max = assignments.Count;
                Console.WriteLine("H_max: " + h_max);

                // default first. Then, overwrite one row, and write it back after?
                bool[,] scoresP1 = new bool[h_max,t_max];
                bool[,] scoresP2 = new bool[h_max,t_max];
                // bytes would have worked too. 1 bool uses as much memory as 1 byte.

                // TODO: is the next placement... also +x??? Jup!

                // start with every row default instantiated.
                for (int h = 0; h < h_max; h++)
                {
                    assignScoresRow(ref scoresP1, assignments, h, problem.p1);
                    assignScoresRow(ref scoresP2, assignments, h, problem.p2);
                }

                Console.WriteLine("Default scores assigned!");

                // TODO: lol. Doses2D cannot be compared xD

                // now, go over all possible combinations of p1,p2.
                // check if they can be placed anyway.
                Doses2D best_dose = new Doses2D(p.r1, h_max, p.r1 + problem.g + p.x,h_max);
                // assign him default the first possible place in a new hospital.
                // only when no assignment could be done, will this remain, and thus be placed.
                double best_score = -1;

                // TODO <=? Pretty sure.
                for (int t1 = p.r1; t1 <= p.d1; t1++)
                {
                    // check whether can be placed in ANY hospital.
                    for (int h1 = 0; h1 < h_max; h1++)
                    {
                        // if can be placed there.
                        // luckily for us, we have the 'scores' list to tell us whether we can place there!
                        if(scoresP1[h1,t1])
                        {
                            // TODO <=?
                            // for every placement of the second jab
                            for (int t2 = t1 + problem.g + p.x; t2 <= t1 + problem.g + p.x + p.L; t2++)
                            {
                                for (int h2 = 0; h2 < h_max; h2++)
                                {
                                    if(scoresP2[h2,t2])
                                    {
                                        // we found a succesful placement for Jab1 and Jab2.
                                        Console.WriteLine("Succesful pair found!");

                                        // add this new assignment to the list, sort it again.
                                        Dose2D new_dose1 = new Dose2D(t1, h1);
                                        Dose2D new_dose2 = new Dose2D(t2, h2);
                                        assignments[h1].Add(new_dose1);
                                        assignments[h2].Add(new_dose2);
                                        assignments[h1].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t)); // really inefficient. For now, fine. TODO
                                        assignments[h2].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t));
                                        // TODO: Sort on what?

                                        // keep the reference, to remove it after.
                                        // do your calculations again, but only for the changed hospital(s).
                                        // for both changed hospitals, update both rows.
                                        assignScoresRow(ref scoresP1, assignments, h1, problem.p1);
                                        assignScoresRow(ref scoresP1, assignments, h2, problem.p1);
                                        assignScoresRow(ref scoresP2, assignments, h1, problem.p2);
                                        assignScoresRow(ref scoresP2, assignments, h2, problem.p2);

                                        // calulate the new root
                                        // check whether this is larger than before. If so, keep.
                                        double new_score = calculateScore(scoresP1) + calculateScore(scoresP2);
                                        if(new_score > best_score)
                                        {
                                            best_score = new_score;
                                            best_dose = new Doses2D(t1, h1, t2, h2);
                                        }

                                        // now throw them out again, and re-calculate the values.
                                        assignments[h1].Remove(new_dose1);
                                        assignments[h2].Remove(new_dose2);
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

                if(best_score == -1) // so no possible combination of assignments... add a new hospital.
                    assignments.Add(new List<Dose2D>());

                final_assignments.Add(best_dose);

                Dose2D d1 = new Dose2D(best_dose.t1, best_dose.h1);
                Dose2D d2 = new Dose2D(best_dose.t2, best_dose.h2);
                assignments[d1.h].Add(d1);
                assignments[d2.h].Add(d2);
                assignments[d1.h].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t)); // not needed if added to new hospital, but can't hurt.
                assignments[d2.h].Sort((Dose2D a, Dose2D b) => a.t.CompareTo(b.t));

                // since we might need to do this 10.000 times per assignment, it is indeed VERY helpful to
                // calculate and save all hospitals before, and re-use these values. Make it a function!
                // can we? for OPTIMIZATION moment TODO       
            }

            // Now all are finished.

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

        public void assignScoresRow(ref bool[,] scores, List<List<Dose2D>> assignments, int h, int p) // the scores to fill, what row to fill, and the p1/p2.
        {
            // TODO: check all <, <= etc. at the end before running!
            // DONE: checked all here. Seems fine.

            int t;

            // If you want to overwrite, make sure to overwrite ALL values = write 0's too!
            for (t = 0; t < scores.GetLength(1); t++)
                scores[h,t] = false;

            int ass_count = assignments[h].Count;

            if(ass_count == 0) // special case for empty assignments_list.
            {
                for (t = 0; t < scores.GetLength(1) - p; t++)
                    scores[h,t] = true;
                return;
            }

            // one case for 0-first.
            for (t = 0; t <= assignments[h][0].t - p; t++)
                scores[h,t] = true;

            // one case for final-t_max.
            Dose2D last = assignments[h][assignments[h].Count - 1];
            for (t = last.t + p; t <= scores.GetLength(1) - p; t++)
                scores[h,t] = true;

            // the middle (if any)
            if(ass_count == 1)
                return;
            
            for (int a = 0; a < assignments[h].Count - 2; a++) // (current, next). -2 is last with a next!
            {
                Dose2D current = assignments[h][a];
                Dose2D next    = assignments[h][a+1];
                for (t = current.t + p; t <= next.t - p; t++)
                    scores[h,t] = true;
            }
            
        }
    }
}