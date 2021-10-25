import json
import os
import subprocess
import time
import random


def main():
    run_all()
    #latex()
    input()


def latex():
    def row(vs):
        return " & ".join(vs) + "\\\\"

    def formatter(v):
        if isinstance(v, float):
            return f"{v:.2f}"
        elif isinstance(v, str):
            return v
        else:
            return "-"

    data = jload("offline.json")

    x = data["solvers"]
    y = list(data["labels"].keys())

    v = data["runs"]

    xn = len(x)

    fmt = "c" + xn * "|c"

    print("\\begin{tabular}{" + fmt + "}")
    print(row([""] + x))

    for lab in y:
        vals = [lab] + [v[i].get(lab, None) for i in x]
        out = [formatter(val) for val in vals]

        print("\\hline")
        print(row(out))     
    print("\\end{tabular}")

    print("\\begin{tabular}{r|r}")
    print("\\hline")
    for k, v in data["labels"].items():
        print(row([k, v]))
        print("\\hline")
    print("\\end{tabular}")


def run_all():
    offline = "offline"
    json_file = offline + ".json"

    data = jload(json_file)
    
    for solver in data["solvers"]:
        print("solver", solver)

        runs = data["runs"].setdefault(solver, {})
        
        for label, fn in data["labels"].items():
            print("case", fn)
            
            if label in runs:
                continue

            fn = os.path.join(offline, fn)
            fn = os.path.abspath(fn)
            
            res = run_one(solver, fn, offline)
            runs[label] = res
        
            jdump(json_file, data)


def run_one(solver, test, offline):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    t0 = time.perf_counter()

    p = subprocess.Popen([exe, "case", offline, solver, test])
    
    try:
        p.wait(60)
        retcode = p.poll()
    except:
        p.kill()
        return None

    if retcode != 0:
        return None

    t1 = time.perf_counter()
    dt = t1 - t0

    return dt


def series(solver, seed=None, timeout=None, tries=None):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    if seed is None:
        seed = random.randint(-1 << 30, 1 << 30)

    if timeout is None:
        timeout = 1.0

    if tries is None:
        tries = 10

    try:
        out = subprocess.check_output([exe, "series", "offline", solver, str(timeout), str(tries), str(seed)])
    except:
        out = None

    out = json.loads(out)

    filename = "series.json"
    try:
        data = jload(ratio_file)
    except:
        data = {}

    tries = str(tries)
    seed = str(seed)
    
    point = data.setdefault(solver, {}).setdefault(tries, {})
    point[seed] = out
    
    jdump(filename, data)


def ratios(solver, seed=None):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    runs = 20
    size = 10

    if seed is None:
        seed = random.randint(-1 << 30, 1 << 30)

    try:
        out = subprocess.check_output([exe, "ratio", "online", solver, str(runs), str(size), str(seed)])
    except:
        out = None

    print(out)

    if out is not None:
        avg, worst = out.split()
        avg = float(avg)
        worst = float(worst)
        out = (avg, worst)

    ratio_file = "ratio.json"
    try:
        data = jload(ratio_file)
    except:
        data = {}

    runs = str(runs)
    size = str(size)
    seed = str(seed)
    
    point = data.setdefault(solver, {}).setdefault(runs, {}).setdefault(size, {})
    point[seed] = out
    
    jdump(ratio_file, data)


def read_file(fn):
    with open(fn, mode="r", encoding="utf-8") as f:
        return f.read()


def write_file(fn, text):
    with open(fn, mode="w", encoding="utf-8") as f:
        f.write(text)


def jload(fn):
    return json.loads(read_file(fn))


def jdump(fn, x):
    return write_file(fn, json.dumps(x, indent=4))


def alpha_label(i):
    ret = ""
    a = ord('a')
    
    i, r = i // 26, i % 26
    ret = chr(a + r) + ret
    while i > 0:
        i, r = i // 26, i % 26
        ret = chr(a + r) + ret
    
    return ret


def label():
    labels = {}

    for i, fn in enumerate(os.listdir("offline")):
        lab = alpha_label(i)
        labels[lab] = fn
    
    data = jload("offline.json")
    data.setdefault("labels", labels)
    jdump("offline.json", data)


if __name__ == "__main__":
    #main()

    #for s in ["greedy", "forward", "verygreedy"]:
    #    ratios(s, -160261352)

    series("ilp", 765292910, 10.0, 10)
