from random import randrange
from problem import Problem
from patient import Patient
from solution import Solution


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


def three_quarters(n=10, w=2):
    # this test case is designed to break programs that use a static heuristic to allocate their timeslots

    # for example, n=3, w=1
    # t  0 1 2 3 4 5 6 7 8 9 . .
    # p1 - - . . . . - - . . . .
    # p2 . . - - . . . . - - . .
    # p3 . . . . - - . . . . - -

    # p1 + + _ _ _ _ * * _ _ _ _ _
    # p2 + + + + + + * * * * * _ _ 
    # p3 _ _ + + + + + + * * * * *
    two = 2 * w
    four = 4 * w

    p1 = p2 = two
    p = Problem(p1, p2, 0)
    s = Solution(1)

    p.add_patient(Patient(0, p1 - 1, two * (n - 1), p2))
    s.add_patient(0, 0, p1 - 1 + two * (n - 1), 0)

    for i in range(1, n):
        p.add_patient(Patient((i - 1) * two, (i - 1) * two + p1 - 1 + i * four, 0, p2 + four - 1))
        s.add_patient((i - 1) * two + two, 0, p1 + (i - 1) * two + two * (n - 1) + two, 0)

    p.write_to_file_as_offline_problem("../data/offline/three_quarters.txt")
    p.write_to_file_as_online_problem("../data/online/three_quarters.txt", randomized=False)
    s.write_solution("../data/solutions/offline/three_quarters.txt")


def backtracker(n=10, w=4):
    # this test case is designed to make bruteforcers reach the last choice before uncovering the suboptimality
    # and making them backtrack all the way to the first patient.
    # note that this is almost three_quarters in reverse

    # for example
    # t  0 1 2 3 4 5 6 7 8
    # p1 - - . . - - - . .
    # p2 . . - - . . - - -

    # oh no!

    # p1 . . - - . . . - - -
    # p2 - - . . - - - . . .

    # larger
    # constraints (+ 1st ok, x user gap, * 2nd ok, _ no):
    # t  0 1 2 3 4 5 6 7 8 9
    # p1 + + + + + + * * * * * * * * *
    # p2 + + + + + x * * * * * * * * _
    # p3 + + + + + + * * * * * * * * _

    # need the x to force backtrack to p1, not p2

    # p1 - - . . . . - - - . . . . .
    # p2 . . - - . . . . . - - - . .
    # p3 . . . . - - . . . . . - - -

    # oh no!

    # t  0 1 2 3 4 5 6 7 8 9
    # p1 . . . . - - . . . . . . - - -
    # p2 . . - - . . . . . - - - . . .
    # p3 - - . . . . - - - . . . . . .
    
    p1 = w
    p2 = w + 1
    p = Problem(p1, p2, 0)

    d = p1 - 1
    d += p1 * (n - 1)

    L = p2
    L += p2 * (n - 1)

    s = Solution(1)

    p.add_patient(Patient(0, d, 0, L))
    s.add_patient(p1 * (n - 1), 0, p1 * (n - 1) + p1 + p2 * (n - 1), 0)

    for i in range(1, n - 1):
        p.add_patient(Patient(0, d - 1, 1, L))
        s.add_patient(p1 * (n - 1 - i), 0, d + p2 * (n - 1 - i), 0)
    p.add_patient(Patient(0, d, 0, L - 1))
    s.add_patient(0, 0, d, 0)
    
    p.write_to_file_as_offline_problem("../data/offline/backtracker.txt")
    p.write_to_file_as_online_problem("../data/online/backtracker.txt")
    s.write_solution("../data/solutions/offline/backtracker.txt")



example_test_case()
three_quarters()
backtracker()
completely_random_n_patients(5)
completely_random_n_patients(20)
completely_random_n_patients(100)
big_numbers()
