using System;
using System.Collections.Generic;

namespace implementation
{
    class ForwardMinimizeOnline : IOnlineSolver
    {
        public Solution2D Step(Solution2D sol, Patient p, Parameters problem)
        {
            
            // a list containing all (current) assignments.
            // points from START_TIME to P1 or P2.
            List<SortedList<int, int>> assignments = new List<SortedList<int, int>>();
            // add every starting_time in here!

            int h_max = 0;
            foreach(Doses2D d in sol.hospitals)
            {
                h_max = Math.Max(d.h1, d.h2);
                while(assignments.Count <= h_max) // "<=" because h_max is an index.
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
                if(a.t2 + problem.p2 > max_ass)
                    max_ass = a.t2 + problem.p2;
            int max_pat = 0; // max of new assignment.
            max_pat = p.d1 + problem.g + p.x + p.L;
            
            int t_max = Math.Max(max_ass, max_pat); // t_max is the maximum of both.

            // default first. Then, overwrite one row, and write it back after?
            bool[,] scoresP1 = new bool[h_max,t_max + 1]; // if t_max = 13, that means i = 13. Means 14 elements!
            bool[,] scoresP2 = new bool[h_max,t_max + 1];
            // bytes would have worked too. 1 bool uses as much memory as 1 byte.

            // start with every row default instantiated.
            for (int h = 0; h < h_max; h++)
            {
                assignScoresRow(ref scoresP1, assignments, h, problem.p1);
                assignScoresRow(ref scoresP2, assignments, h, problem.p2);
            }

            // This is not the most beautiful code.

            // first consider all the options within the existing hospitals - h_max - 1
            List<Doses2D> possibleComb = new List<Doses2D>();
            for (int t1 = p.r1; t1 <= p.d1 - problem.p1 + 1; t1++)
                // check whether can be placed in ANY hospital.
                for (int h1 = 0; h1 < h_max - 1; h1++)
                    // if can be placed there.
                    // luckily for us, we have the 'scores' list to tell us whether we can place there!
                    if(scoresP1[h1,t1])
                        // for every placement of the second jab
                        for (int t2 = t1 + problem.p1 + problem.g + p.x; t2 <= t1 + problem.p1 + problem.g + p.x + p.L - problem.p2; t2++)
                            for (int h2 = 0; h2 < h_max - 1; h2++)
                                if(scoresP2[h2,t2])
                                    possibleComb.Add(new Doses2D(t1, h1, t2, h2));
            // after those, the ones with a new hospital (substitute h1, respective h2, with h_max - 1)
            for (int t1 = p.r1; t1 <= p.d1 - problem.p1 + 1; t1++)
                if(scoresP1[h_max - 1,t1])
                    for (int t2 = t1 + problem.p1 + problem.g + p.x; t2 <= t1 + problem.p1 + problem.g + p.x + p.L - problem.p2; t2++)
                        for (int h2 = 0; h2 < h_max - 1; h2++)
                            if(scoresP2[h2,t2])
                                possibleComb.Add(new Doses2D(t1, h_max - 1, t2, h2));
            for (int t1 = p.r1; t1 <= p.d1 - problem.p1 + 1; t1++)
                for (int h1 = 0; h1 < h_max; h1++) // ONLY ONE NOT -1. SUCH THAT LAST CASE - BOTH IN NEW H - IS ALSO CONSIDERED!
                    if(scoresP1[h1,t1])
                        for (int t2 = t1 + problem.p1 + problem.g + p.x; t2 <= t1 + problem.p1 + problem.g + p.x + p.L - problem.p2; t2++)
                            if(scoresP2[h_max - 1,t2])
                                possibleComb.Add(new Doses2D(t1, h1, t2, h_max - 1));
            // Together, these sum all the possibilities. However, all the ones using a new h are pushed to the back.
            // This makes sure the other h's have priority if they happen to have the same score.

            Doses2D best_dose = new Doses2D(-1, -1, -1, -1);
            double best_score = -1; // automatically overwritten by any.
            foreach(Doses2D d in possibleComb)
            {
                // we found a succesful placement for Jab1 and Jab2.

                // This is the 'forward' part - check the score again given
                //   that this pair is added.
                assignments[d.h1].Add(d.t1, problem.p1);
                assignments[d.h2].Add(d.t2, problem.p2);
                // automatically sorted.

                // sorting is needed for assigning scores to a row (assignScoresRow)
                //   that is the most efficient way of calculating it, rather (given an efficient sort).

                // do your calculations again, but only for the changed hospital(s).
                assignScoresRow(ref scoresP1, assignments, d.h1, problem.p1);
                assignScoresRow(ref scoresP1, assignments, d.h2, problem.p1);
                assignScoresRow(ref scoresP2, assignments, d.h1, problem.p2);
                assignScoresRow(ref scoresP2, assignments, d.h2, problem.p2);

                // calulate the new root = final score!
                // check whether this is larger than the largest until now. If so, keep.
                double new_score = calculateScore(scoresP1) + calculateScore(scoresP2);
                if(new_score > best_score)
                {
                    best_score = new_score;
                    best_dose = new Doses2D(d.t1, d.h1, d.t2, d.h2);
                }

                // now throw them out again (the 'backward' or 'undo' phase), and re-calculate the rows back to old.
                assignments[d.h1].Remove(d.t1);
                assignments[d.h2].Remove(d.t2);
                assignScoresRow(ref scoresP1, assignments, d.h1, problem.p1);
                assignScoresRow(ref scoresP1, assignments, d.h2, problem.p1);
                assignScoresRow(ref scoresP2, assignments, d.h1, problem.p2);
                assignScoresRow(ref scoresP2, assignments, d.h2, problem.p2);
            }
            // OLD: if no possible combination of assignments... add a new hospital.
            // the default 'best_score' was initiated to be placed in a new hospital, so we don't
            //   have to do anything else.

            // NEW: since we add a hospital from the start, the only way it is -1 is if no place fit..?
            if(best_score == -1)
                // assignments.Add(new SortedList<int, int>());
                throw new Exception("You seem to not be able to put one in any hospital...?");

            sol.hospitals.Add(best_dose);


            h_max = 0;
            foreach(Doses2D d in sol.hospitals)
            {
                int max_d = Math.Max(d.h1, d.h2);
                if(max_d > h_max)
                    h_max = max_d;
            }
            h_max++; // because we check INDEXES - those are always one lower than the count!
            
            // AHHH. You need to re- and re-make new SOLUTION2D's!
            Solution2D new_sol = new Solution2D(h_max, sol.hospitals);
            return new_sol;
        }

        // given a placed assignment, calculates the new score
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

        // given an assigmnent, makes the necessary preparations and calculations in order to calculate the new score.
        // it fills the rows with TRUE if a p1 or p2 can start there.
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
            // so first write everything to false
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