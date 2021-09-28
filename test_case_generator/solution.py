class Solution:
    def __init__(self, machines):
        self.machines = machines
        self.patients = []

    def add_patient(self, t1, m1, t2, m2):
        self.patients.append([t1, m1, t2, m2])
        
    def write_solution(self, fn):
        with open(fn, "w") as f:
            for p in self.patients:
                f.write(", ".join(str(x) for x in p) + "\n")
        
            f.write(str(self.machines))
