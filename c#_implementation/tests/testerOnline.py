import json
import os
import subprocess
import time


def main():
    #label()
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

    data = jload("online.json")

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
    online = "online"
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
            
            res = run_one(solver, fn, online)
            runs[label] = res
        
            jdump(json_file, data)


def run_one(solver, test, online):
    exe = "../bin/Debug/net5.0/c#_implementation.exe"

    t0 = time.perf_counter()

    p = subprocess.Popen([exe, "case", online, solver, test])
    
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

    for i, fn in enumerate(os.listdir("online")):
        lab = alpha_label(i)
        labels[lab] = fn
    
    data = jload("online.json")
    data.setdefault("labels", labels)
    jdump("online.json", data)


if __name__ == "__main__":
    main()
