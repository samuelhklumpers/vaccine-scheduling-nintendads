from random import randrange


class Problem:
    def __init__(self, processing_time_first_dose, processing_time_second_dose, gap_between_doses):
        # assert that all input is valid
        assert processing_time_first_dose >= 0
        assert processing_time_second_dose >= 0
        assert gap_between_doses >= 0

        self.processing_time_first_dose = processing_time_first_dose
        self.processing_time_second_dose = processing_time_second_dose
        self.gap_between_doses = gap_between_doses
        self.patient_list = []

    def add_patient(self, patient):
        # assert that the given patient details are valid within this problem
        assert patient.last_timeslot_first_dose - \
            patient.first_timeslot_first_dose + 1 >= self.processing_time_first_dose
        assert patient.length_second_dose_interval >= self.processing_time_second_dose

        self.patient_list.append(patient)

    def write_to_file_as_offline_problem(self, filename):
        # write the problem as an offline problem to a given file
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses) + "\n")
            file1.write(str(len(self.patient_list)))
            for patient in self.patient_list:
                file1.write("\n" + str(patient))

    def write_to_file_as_online_problem(self, filename):
        # write the problem as an online problem to a given file
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses))
            for patient in self.patient_list:
                file1.write("\n" + str(patient))


class Patient:
    def __init__(self, first_timeslot_first_dose, last_timeslot_first_dose, delay_between_doses, length_second_dose_interval):
        # assert that all patient details are valid
        assert first_timeslot_first_dose >= 0
        assert last_timeslot_first_dose >= 0
        assert length_second_dose_interval >= 0
        assert delay_between_doses >= 0

        self.first_timeslot_first_dose = first_timeslot_first_dose
        self.last_timeslot_first_dose = last_timeslot_first_dose
        self.delay_between_doses = delay_between_doses
        self.length_second_dose_interval = length_second_dose_interval

    def __str__(self) -> str:
        return str(str(self.first_timeslot_first_dose) + ', ' + str(self.last_timeslot_first_dose) +
                   ', ' + str(self.delay_between_doses) + ', ' + str(self.length_second_dose_interval))


def completely_random_n_patients(n):
    p = Problem(randrange(1, 20), randrange(1, 20), randrange(1, 20))
    for _ in range(n):
        # generate patient data that would fit in the problem, paying special attention to intervals and processing times
        first_timeslot_first_dose = randrange(0, 20)
        last_timeslot_first_dose = randrange(
            first_timeslot_first_dose + p.processing_time_first_dose, first_timeslot_first_dose + p.processing_time_first_dose + 20)
        delay_between_doses = randrange(1, 20)
        length_second_dose_interval = randrange(
            p.processing_time_second_dose, p.processing_time_second_dose + 20)

        p.add_patient(Patient(first_timeslot_first_dose, last_timeslot_first_dose,
                              delay_between_doses, length_second_dose_interval))
    p.write_to_file_as_offline_problem(
        "../data/offline/random_" + str(n) + ".txt")
    p.write_to_file_as_online_problem(
        "../data/online/random_" + str(n) + ".txt")


def example_test_case():
    p = Problem(2, 3, 1)
    p.add_patient(Patient(2, 7, 5, 4))
    p.add_patient(Patient(1, 2, 3, 3))
    p.add_patient(Patient(2, 4, 0, 5))
    p.write_to_file_as_offline_problem("../data/offline/from_assignment.txt")
    p.write_to_file_as_online_problem("../data/online/from_assignment.txt")


def big_numbers():
    p = Problem(100_000_000, 100_000_000, 100_000_000)
    p.add_patient(Patient(0, 99_999_999, 100_000_000, 100_000_000))
    p.add_patient(Patient(100_000_000, 199_999_999, 100_000_000, 100_000_000))
    p.write_to_file_as_offline_problem("../data/offline/big_numbers.txt")
    p.write_to_file_as_online_problem("../data/online/big_numbers.txt")


example_test_case()
completely_random_n_patients(5)
completely_random_n_patients(20)
completely_random_n_patients(100)
big_numbers()
