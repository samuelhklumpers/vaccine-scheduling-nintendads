namespace implementation
{
    public interface IOnlineSolver
    {
        ///Print the solution
        Solution2D solve(OnlineProblem problem);

        Solution2D Step(Solution2D partial, Patient nextPatient, Parameters parameters);
    }
}