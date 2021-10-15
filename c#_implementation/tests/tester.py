import json
import os
import subprocess
import time


def main():
    #run_all()
    latex()


def latex():
    def row(vs):
        return " & ".join(vs) + "\\\\"

    def formatter(v):
        if isinstance(v, float):
            return f"{v:.1e}"
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

    print("\\begin{longtable}{" + fmt + "}")
    print(row([""] + x))

    for lab in y:
        vals = [lab] + [v[i].get(lab, None) for i in x]
        out = [formatter(val) for val in vals]

        print("\\hline")
        print(row(out))     
    print("\\end{longtable}")

    print("\\begin{longtable}{r|r}")
    print("\\hline")
    for k, v in data["labels"].items():
        print(row([k, v]))
        print("\\hline")
    print("\\end{longtable}")


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
