import json
import os
import subprocess
import time
import traceback
import random


def main():
    """
    for solver in ["ilp", "sat", "bf"]:
        offline_random_time("bf", 123, timeout=1.0)
    """

    """

    for solver in ["lexi", "forward"]:
        for size in range(5, 12):
            online_random_ratio(solver, seed=123, size=size, runs=10)
            # seed is fixed, but the updated seed is reused, so runs=10 has a point
    """


    """
    labels = jload("online_labels.json")
    
    for label, fn in labels.items():
        fn = os.path.join("online", fn)
        fn = os.path.abspath(fn)
        online_test_ratio("forward", fn, label)
    """

    #latex_online()

    #online_label_table()

    #offline_test_time()

    online_testcases_machines()

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

    data = jload("online_test.json")

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


def latex_online():
    def row(vs):
        return " & ".join(vs) + "\\\\"

    def formatter(v):
        if isinstance(v, float):
            return f"{v:.2f}"
        elif isinstance(v, str):
            return v
        else:
            return "-"

    data = jload("online_test.json")

    x = list(data.keys())
    y = data[x[0]].keys()

    xn = len(x)

    fmt = "c" + xn * "|c"

    print("\\begin{tabular}{" + fmt + "}")
    print(row([""] + x))

    for lab in y:
        vals = [lab] + [data[i].get(lab, None) for i in x]
        out = [formatter(val) for val in vals]

        print("\\hline")
        print(row(out))     
    print("\\end{tabular}")

def online_label_table():
    def row(vs):
        return " & ".join(vs) + "\\\\"

    def formatter(v):
        if isinstance(v, float):
            return f"{v:.2f}"
        elif isinstance(v, str):
            return v
        else:
            return "-"
        
    data = jload("online_labels.json")
    
    print("\\begin{tabular}{r|r}")
    print("\\hline")
    for k, v in data.items():
        print(row([k, v]))
        print("\\hline")
    print("\\end{tabular}")
    

def offline_test_time():
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
        p.wait(300)
        retcode = p.poll()
    except:
        p.kill()
        return None

    if retcode != 0:
        return None

    t1 = time.perf_counter()
    dt = t1 - t0

    return dt


def offline_random_time(solver, seed=None, timeout=1.0, tries=10): # tries=10
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    if seed is None:
        seed = random.randint(-1 << 30, 1 << 30)

    try:
        out = subprocess.check_output([exe, "series", "offline", solver, str(timeout), str(tries), str(seed)])
    except:
        out = None

    out = json.loads(out)

    filename = "offline_random2.json"
    try:
        data = jload(filename)
    except:
        data = {}

    tries = str(tries)
    seed = str(seed)
    
    point = data.setdefault(solver, {}).setdefault(tries, {})
    point[seed] = out
    
    jdump(filename, data)


def online_random_ratio(solver, seed=None, size=10, runs=20):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    if seed is None:
        seed = random.randint(-1 << 30, 1 << 30)

    try:
        out = subprocess.check_output([exe, "ratio", "online", solver, str(runs), str(size), str(seed)], timeout=60)
    except:
        traceback.print_exc()
        out = None

    print(out)

    if out is not None:
        avg, worst = out.split()
        avg = float(avg)
        worst = float(worst)
        out = (avg, worst)

    ratio_file = "online_random.json"
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

def online_testcases_machines():
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    online = "online-machines"
    json_file = online + ".json"

    data = jload(json_file)

    for solver in data["solvers"]:
        print("solver", solver)
        
        runs = data["runs"].setdefault(solver, {})
        
        for label, fn in data["labels"].items():
            print("case", fn)

            if label in runs:
                continue

            fn = os.path.join(online, fn)
            fn = os.path.abspath(fn)

            try:
                out = subprocess.check_output([exe, "case", "online", solver, fn], timeout=60)
            except:
                traceback.print_exc()
                out = None

            if out is not None:
                out = float(out)
            print(out)
            runs[label] = out
    
            jdump(json_file, data) 


def online_test_ratio(solver, testFile, label):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    try:
        print(" ".join([exe, "compete", "online", solver, testFile]))
        out = subprocess.check_output(
            [exe, "compete", "online", solver, testFile],
            timeout=60,
            creationflags=subprocess.CREATE_NO_WINDOW
        )
    except:
        out = None

    print(out)
        
    if out is not None:
        out = float(out)

    ratio_file = "online_test.json"
    try:
        data = jload(ratio_file)
    except:
        data = {}
    
    point = data.setdefault(solver, {})
    point[label] = out
    
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


def label(which):
    labels = {}

    for i, fn in enumerate(os.listdir(which)):
        lab = alpha_label(i)
        labels[lab] = fn
    
    jdump(which + "_labels.json", labels)


if __name__ == "__main__":
    main()
