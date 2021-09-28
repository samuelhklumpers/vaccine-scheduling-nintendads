using System.Collections.Generic;

namespace implementation
{
    class Parser
    {
        public static HospitalSolution Parse_solution(string file)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = lines[i].Replace(" ", "");
            }

            List<RegistrationWithHospital> registrations = new List<RegistrationWithHospital>();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] patient = lines[i].Split(",");
                var registration = new RegistrationWithHospital(int.Parse(patient[0]), int.Parse(patient[1]), int.Parse(patient[2]), int.Parse(patient[3]));
                registrations.Add(registration);
            }

            int machines = int.Parse(lines[lines.Length - 1]);

            return new HospitalSolution(machines, registrations);
        }

        public static OfflineProblem Parse_problem_offline(string file)
        {
            string[] data = System.IO.File.ReadAllLines(file);
            int processing_time_first_dose = int.Parse(data[0]);
            int processing_time_second_dose = int.Parse(data[1]);
            int gap = int.Parse(data[2]);
            int number_of_patients = int.Parse(data[3]);

            List<Patient> patients_data = new List<Patient>();
            for (int i = 4; i < data.Length; i++)
            {
                string[] patient_data = data[i].Split(", ");
                Patient patient = new Patient(int.Parse(patient_data[0]), int.Parse(patient_data[1]), int.Parse(patient_data[2]), int.Parse(patient_data[3]), processing_time_first_dose, processing_time_second_dose, gap);
                patients_data.Add(patient);
            }

            OfflineProblem problem = new OfflineProblem(processing_time_first_dose, processing_time_second_dose, gap, number_of_patients, patients_data);
            return problem;
        }
        public static OnlineProblem Parse_problem_online(string file)
        {
            string[] data = System.IO.File.ReadAllLines(file);
            int processing_time_first_dose = int.Parse(data[0]);
            int processing_time_second_dose = int.Parse(data[1]);
            int gap = int.Parse(data[2]);

            List<Patient> patients_data = new List<Patient>();
            for (int i = 3; i < data.Length; i++)
            {
                string[] patient_data = data[i].Split(", ");
                Patient patient = new Patient(int.Parse(patient_data[0]), int.Parse(patient_data[1]), int.Parse(patient_data[2]), int.Parse(patient_data[3]), processing_time_first_dose, processing_time_second_dose, gap);
                patients_data.Add(patient);
            }

            OnlineProblem problem = new OnlineProblem(processing_time_first_dose, processing_time_second_dose, gap, patients_data);
            return problem;
        }
    }
}
