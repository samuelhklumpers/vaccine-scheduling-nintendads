import json
import os
import subprocess
import time


def main():
    run_all()


def run_all():
    data = jload("offline.json")
    
    for solver in data["solvers"]:
        print("solver", solver)

        runs = data["runs"].setdefault(solver, {})
        
        for label, fn in data["labels"].items():
            print("case", fn)
            
            if label in runs:
                continue

            fn = os.path.join("offline", fn)
            fn = os.path.abspath(fn)
            
            res = run_one(solver, fn)
            runs[label] = res
        
            jdump("offline.json", data)


def run_one(solver, test):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    t0 = time.perf_counter()

    p = subprocess.Popen([exe, "case", solver, test])
    
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
    main()
