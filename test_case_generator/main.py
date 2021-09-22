from random import randrange


class Problem:
    def __init__(self, processing_time_first_dose, processing_time_second_dose, gap_between_doses) -> None:
        self.processing_time_first_dose = processing_time_first_dose
        self.processing_time_second_dose = processing_time_second_dose
        self.gap_between_doses = gap_between_doses
        self.patient_list = []

    def add_patient(self, patient):
        self.patient_list.append(patient)

    def write_to_file_as_offline_problem(self, filename):
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses) + "\n")
            file1.write(str(len(self.patient_list)))
            for patient in self.patient_list:
                file1.write("\n" + str(patient))

    def write_to_file_as_online_problem(self, filename):
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses))
            for patient in self.patient_list:
                file1.write("\n" + str(patient))


class Patient:
    def __init__(self, first_timeslot_first_dose, last_timeslot_first_dose, delay_between_doses, length_second_dose_interval):
        self.first_timeslot_first_dose = first_timeslot_first_dose
        self.last_timeslot_first_dose = last_timeslot_first_dose
        self.delay_between_doses = delay_between_doses
        self.length_second_dose_interval = length_second_dose_interval

    def __str__(self) -> str:
        return str(str(self.first_timeslot_first_dose) + ', ' + str(self.last_timeslot_first_dose) +
                   ', ' + str(self.delay_between_doses) + ', ' + str(self.length_second_dose_interval))


def completely_random_n_patients(n):
    p = Problem(randrange(0, 20), randrange(0, 20), randrange(0, 20))
    for _ in range(n):
        # check if processing time fits in intervals
        first_timeslot = randrange(0, 20)
        p.add_patient(Patient(first_timeslot, first_timeslot +
                              randrange(1, 20), randrange(0, 20), randrange(0, 20)))
    p.write_to_file_as_offline_problem("random_offline.txt")


def example_test_case_offline():
    p = Problem(2, 3, 1)
    p.add_patient(Patient(2, 7, 5, 4))
    p.add_patient(Patient(1, 2, 3, 3))
    p.add_patient(Patient(2, 4, 0, 5))
    p.write_to_file_as_offline_problem("assignment_offline.txt")


def example_test_case_online():
    p = Problem(2, 3, 1)
    p.add_patient(Patient(2, 7, 5, 4))
    p.add_patient(Patient(1, 2, 3, 3))
    p.add_patient(Patient(2, 4, 0, 5))
    p.write_to_file_as_online_problem("assignment_online.txt")


def big_numbers():
    p = Problem(100000, 100000, 100000)
    p.add_patient(Patient(100000, 200000, 100000, 100000))
    p.add_patient(Patient(200001, 300000, 100000, 100000))
    p.write_to_file_as_offline_problem("big_numbers.txt")


example_test_case_offline()
example_test_case_online()
completely_random_n_patients(20)
big_numbers()
