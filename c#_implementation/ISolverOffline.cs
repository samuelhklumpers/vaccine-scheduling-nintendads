<<<<<<< HEAD
using System.Globalization;
=======
>>>>>>> test_solutions
namespace implementation
{
    interface ISolverOffline
    {
<<<<<<< HEAD
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
=======
        ///Print the solution
        Solution solve(OfflineProblem problem);
    }
}
>>>>>>> test_solutions
