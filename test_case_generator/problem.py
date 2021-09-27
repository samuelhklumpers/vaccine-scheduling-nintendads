import random


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

    def write_to_file_as_offline_problem(self, filename, randomized=False):
        # write the problem as an offline problem to a given file
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses) + "\n")
            file1.write(str(len(self.patient_list)))

            if randomized:
                random.shuffle(self.patient_list)

            for patient in self.patient_list:
                file1.write("\n" + str(patient))

    def write_to_file_as_online_problem(self, filename, randomized=False):
        # write the problem as an online problem to a given file
        with open(filename, "w") as file1:
            file1.write(str(self.processing_time_first_dose) + "\n")
            file1.write(str(self.processing_time_second_dose) + "\n")
            file1.write(str(self.gap_between_doses))

            if randomized:
                random.shuffle(self.patient_list)

            for patient in self.patient_list:
                file1.write("\n" + str(patient))
