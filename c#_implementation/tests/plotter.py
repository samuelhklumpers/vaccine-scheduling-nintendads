import matplotlib.pyplot as plt
import numpy as np
import json


def offline_random_line(fn, runs, seed):
    data = jload(fn)

    fig = plt.figure()
    for solver, subdata in data.items():
        y = np.array(subdata[str(runs)][str(seed)])
        x = np.arange(1, 1 + len(y))

        plt.plot(x, y, label=solver)

    plt.xlabel("$n$")
    plt.ylabel("$t$ (s)")

    plt.tight_layout()

    plt.legend() 
    plt.show()
    


def offline_test_scatter(fn):
    data = jload(fn)
    groups = data["runs"]

    x = list(data["labels"].keys())

    top = 0
    ys = {}
    for g, vs in groups.items():
        y = []

        for xi in x:
            v = vs[xi]

            if v is None:
                v = -1

            if v > top:
                top = v
            
            y.append(v)

        ys[g] = y

    top *= 1.1

    x = np.array(x, dtype=object)

    plt.figure()
    c = 0

    cmap = plt.cm.viridis
    
    for g, y in ys.items():
        y = np.array(y, dtype=float)

        ok = y >= 0
        bad = y < 0

        x_ok = x[ok]
        x_bad = x[bad]

        y_ok = y[ok]
        y_bad = y[bad]
        y_bad[:] = top
        
        plt.scatter(x_ok, y_ok, label=g, color=cmap(c))
        plt.scatter(x_bad, y_bad, marker='x', color=cmap(c))

        c += 1 / (len(ys) - 1)
        
    plt.legend()
    plt.show()
    

    
def online_random_line(fn, runs=10, seed=123):
    data = jload(fn)
    
    seed = str(seed)

    fig = plt.figure()
    for solver, subdata in data.items():
        y = []
        x = []

        points = subdata[str(runs)]

        for size, samples in points.items():
            if seed in samples:
                sample = samples[seed]

                if sample is not None:
                    y += [sample[0]]
                    x += [size]

        x = np.array(x)
        y = np.array(y)

        plt.plot(x, y, label=solver)

    plt.xlabel("$n$")
    plt.ylabel("$t$ (s)")

    plt.tight_layout()

    plt.legend() 
    plt.show()


def online_test_scatter(fn):
    groups = jload(fn)

    x = list(list(groups.values())[0].keys())

    ys = {}
    for g, vs in groups.items():
        y = []

        for xi in x:
            v = vs[xi]

            y.append(v)

        ys[g] = y


    x = np.array(x, dtype=object)

    c = 0
    cmap = plt.cm.viridis
    
    plt.figure()
    for g, y in ys.items():
        y = np.array(y, dtype=float)

        bad = np.isnan(y)
        ok = ~bad

        x_ok = x[ok]
        x_bad = x[bad]

        y_ok = y[ok]
        y_bad = y[bad]
        
        plt.scatter(x_ok, y_ok, label=g, color=cmap(c))

        c += 1 / (len(ys) - 0.9)
        
    plt.legend()
    plt.show()


def main():
    #offline_random_line("offline_random2.json", 10, 123)
    #offline_test_scatter("offline-180s.json")
    #online_random_line("online_random.json")
    online_test_scatter("online_test.json")




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


if __name__ == "__main__":
    main()
