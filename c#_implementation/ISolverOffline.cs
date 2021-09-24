namespace implementation
{
    interface ISolverOffline
    {
        Solution solve(OfflineProblem problem);
    }
    interface IHospitalSolverOffline : ISolverOffline
    {
        new HospitalSolution solve(OfflineProblem problem) { return ((ISolverOffline)this).solve(problem).AddHospitals(problem); }
        // how do I do this without the ((ISolverOffline)this) hack
        // base doesn't seem to work here
    }
}
