from random import randrange
from problem import Problem
from patient import Patient


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
    # this case is the example from the assignment
    p = Problem(2, 3, 1)
    p.add_patient(Patient(2, 7, 5, 4))
    p.add_patient(Patient(1, 2, 3, 3))
    p.add_patient(Patient(2, 4, 0, 5))
    p.write_to_file_as_offline_problem("../data/offline/from_assignment.txt")
    p.write_to_file_as_online_problem("../data/online/from_assignment.txt")


def big_numbers():
    # this test case is mostly meant to break programs that allocate based on the amount of timeslots
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
