namespace implementation
{
    interface IOfflineSolver
    {
        Solution solve(OfflineProblem problem);
    }
    interface IOffline2DSolver
    {
        Solution2D solve(OfflineProblem problem);
    }

    class Wrapper2D : IOffline2DSolver
    {
        public IOfflineSolver solver;

        public Wrapper2D(IOfflineSolver solver) {
            this.solver = solver;
        }

        public Solution2D solve(OfflineProblem problem) {
            return this.solver.solve(problem).AddHospitals(problem);
        }
    }
}
