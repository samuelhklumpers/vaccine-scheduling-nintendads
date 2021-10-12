:- use_module(library(clpfd)).
:- initialization main.


unmarshal_jobs([], []).
unmarshal_jobs(JM, J) :-
    JM = [R1, D1, X, L|JMT],
    unjob(H, R1, D1, X, L),
    J = [H|JT],
    unmarshal_jobs(JMT, JT).


main :-
    % read in problem
    current_prolog_flag(argv, Argv),
    
    maplist(term_to_atom, Args, Argv),
    [M, P1, P2, G|Tail] = Args,
    unmarshal_jobs(Tail, Js),

    % check if problem is satisfiable with M slots
    satisfy(P1, P2, G, Js, Sat, M),
    % return satisfiability as 0/1
    write(Sat),
    halt(0).


satisfy(P1, P2, G, J, Sat, M) :-
    packing(M, P1, P2, G, J, Vs),
    label(Vs) -> Sat = 1; Sat = 0.


add(X, Y, Z):-
    Z #= X + Y.


add3(X, Y, Z, W):-
    W #= X + Y + Z.


packing(M, P1, P2, G, Js, Vs) :-
    same_length(Js, T1s),
    same_length(Js, T2s),
    same_length(Js, H1s),
    same_length(Js, H2s),

    % hospital choices
    H1s ins 1..M,   % 0-based indexing doesn't apply here
    H2s ins 1..M,   % because these aren't indices

    append([T1s, T2s, H1s, H2s], Vs),
    transpose(Js, JsT),
    JsT = [Ts, Xs, Ls],

    transpose(Ts, TsT),
    TsT = [R1s, D0s],

    write(T1s),
    write(H1s),
    write(T2s),
    write(H2s),

    maplist(add(1 - P1), D0s, D1s),

    % feasability
    maplist(#=<, R1s, T1s),
    maplist(#=<, T1s, D1s),

    maplist(add3(P1 + G), T1s, Xs, R2s),
    maplist(add3(1 - P2), R2s, Ls, D2s),

    maplist(#=<, R2s, T2s),
    maplist(#=<, T2s, D2s),

    % is the problem satisfiable, with at most M machines?
    fits(P1, P2, T1s, H1s, T2s, H2s).


fits(P1, P2, T1s, H1s, T2s, H2s) :-
    maplist(fits2(P1, P2, T2s, H2s), T1s, H1s),
    fits3(P1, T1s, H1s),
    fits3(P2, T2s, H2s).

fits2(P1, P2, T2s, H2s, T1, H1) :-
    maplist(no_overlap(P1, P2, T1, H1), T2s, H2s).

fits3(_, [], []).
fits3(P1, [T1|T1s], [H1|H1s]):-
    maplist(no_overlap(P1, P1, T1, H1), T1s, H1s),
    fits3(P1, T1s, H1s).

no_overlap(P1, P2, T1, H1, T2, H2) :-
    H1 #= H2 #==> (T2 + P2 #=< T1 #\/ T1 + P1 #=< T2).

test(N, M, Sat) :-
    test_case(N, P1, P2, G, J),
    satisfy(P1, P2, G, J, Sat, M).

test2(N, Vs, M) :-
    test_case(N, P1, P2, G, J),
    packing(M, P1, P2, G, J, Vs).

test_case(0, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 3,
    J = [[[0, 2], 1, 2], [[0, 2], 1, 2]].

test_case(1, P1, P2, G, J) :-
    P1 = 1,
    P2 = 1,
    G = 3,
    J = [[[0, 2], 1, 2], [[1, 3], 2, 3], [[0, 2], 1, 2], [[1, 3], 2, 3]].
    
test_case(2, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5]].
         
test_case(3, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5],
         [[5, 15], 5, 3]].

test_case(4, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5],
         [[5, 15], 5, 3],  [[6, 16], 5, 4]].

test_case(5, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5],
         [[5, 15], 5, 3],  [[6, 16], 5, 4], [[6, 16], 5, 4], [[6, 16], 4, 5]].
    
test_case(6, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5],
         [[5, 15], 5, 3],  [[6, 16], 5, 4], [[6, 16], 5, 4], [[6, 16], 4, 5],
         [[10, 20], 1, 1], [[0, 2], 0, 3],  [[8, 12], 1, 3], [[9, 13], 2, 5]].
