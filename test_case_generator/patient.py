
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
