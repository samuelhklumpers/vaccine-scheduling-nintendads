namespace implementation
{
    interface ISolverOffline
    {
        Solution solve(OfflineProblem problem);
    }
    interface IHospitalSolverOffline
    {
        HospitalSolution solve(OfflineProblem problem);
    }

    class Hospitalizer : IHospitalSolverOffline
    {
        public ISolverOffline solver;

        public Hospitalizer(ISolverOffline solver) {
            this.solver = solver;
        }

        public HospitalSolution solve(OfflineProblem problem) {
            return this.solver.solve(problem).AddHospitals(problem);
        }
    }
}
