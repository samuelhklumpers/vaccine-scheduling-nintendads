:- use_module(library(clpfd)).
:- initialization main.

% See bottom of file if unfamiliar with predicate logic or Prolog

main :-
    current_prolog_flag(argv, Argv),
    %X = Argv,
    maplist(term_to_atom, X, Argv),
    [P1, P2, G|T] = X,
    unmarshal_jobs(T, J),
    solve(P1, P2, G, J, Sol, M),
    marshal_solution(M, Sol, Out),
    write(Out),
    halt(0).


unmarshal_jobs([], []).
unmarshal_jobs(JM, J) :-
    JM = [R1, D1, X, L|JMT],
    unjob(H, R1, D1, X, L),
    J = [H|JT],
    unmarshal_jobs(JMT, JT).

marshal_solution(M, Sol, S) :-
    flatten(Sol, FSol),
    Out = [M|FSol],
    atomic_list_concat(Out, ' ', S).

% state the offline test_case, variables as in assignment (but capitalized).
% J is a list of tuples ((R1, D1), X, L).
% Sol is a list of tuples (T1, T2), so that the feasability conditions hold:
% R1[i] <= Sol[i][0] <= D1[i],
% Sol[i][0] + G + X[i] <= Sol[i][1] <= Sol[i][0] + G + X[i] + L[i].
offline_problem(P1, P2, G, J, Sol) :-
    same_length(J, Sol),                    % as many allocations as patients
    maplist(length_flip(2), Sol),           % 2 appointments per patient
    maplist(single_job(P1, P2, G), J, Sol).  % solve all appointments for each patient  

length_flip(L, Xs) :-
    length(Xs, L).

% equivalent to `Var in Lower..Upper`, but Lower and Upper may be uninstantiated
my_in(Var, Lower, Upper) :-
    Lower #=< Var, Var #=< Upper.

% state the test_case for a single job.
single_job(P1, P2, G, Ji, Soli) :-
    unjob(Ji, R1, D0, X, L),    % Ji is a tuple ((R1, D1), X, L)
    unsol(Soli, T1, T2),        % Soli is a tuple (T1, T2)
    R2 #= T1 + G + X + P1,
    D2 #= R2 + L - P2 + 1,
    D1 #= D0 - P1 + 1,
    my_in(T1, R1, D1),          % R1 <= T1 <= D1
    my_in(T2, R2, D2).          % T1 + G + X <= T2 <= T1 + G + X + L        

% tuple unpacking helpers
unsol([T1, T2], T1, T2).
unjob([[R, D], X, L], R, D, X, L).

% given a solution, find the number of hospitals necessary
machines(P1, P2, Sol, M) :-
    transpose(Sol, [Row11, Row12]),     % transform the list of pairs Sol, into a pair of lists
    maplist(tuple(P1), Row11, Row21),   % and add the relevant processing times 
    maplist(tuple(P2), Row12, Row22),
    append(Row21, Row22, Ts),           % and concatenate the new lists of pairs (P, T)
    machines2(Ts, M).                   
 
tuple(X, Y, T) :-
    T = [X, Y].
    
% given a list of pairs (P, T), find the number of machines
machines2(Ts, M) :-
    maplist(machines3(Ts), Ts, Ms), % find the number of machines in use at each relevant time
    maximum(Ms, M).                 % and take the largest concurrently active number


% list max
maximum([], 0).
maximum([H|T], Max) :- 
    maximum(T, Max2),
    Max #= max(H, Max2).

% find the number of machines in use at T2 (appointments spanning over T2)
machines3([], _, 0).
machines3([[P1, T1]|Tail], [P2, T2], M) :-
    machines3(Tail, [P2, T2], M2),
    machines4([P1, T1], [P2, T2], V),
    M #= M2 + V.                        % sum 1 for all appointments that hit T2
    
% if T2 is in [T1..T1 + P1] then 1 else 0
machines4([P1, T1], [_, T2], V) :-
    V #<==> T1 #=< T2 #/\ T2 #=< T1 + P1.

% solver
solve(P1, P2, G, J, Sol, M) :-
    offline_problem(P1, P2, G, J, Sol),
    machines(P1, P2, Sol, M),
    append(Sol, Vs),
    labeling([bisect, min(M)], Vs).

% "pretty" print the solution to test case N
pretty(N, M) :-
    test(N, Sol, M),
    append(Sol, Vs),
    labeling([bisect, min(M)], Vs),         % iterate solutions for all variables, starting from lowest number of machine
    maplist(portray_clause, Sol).

% state test case N
test(N, Sol, M) :-
    test_case(N, P1, P2, G, J),
    offline_problem(P1, P2, G, J, Sol),
    machines(P1, P2, Sol, M).

% test cases
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

% clpfd struggles with longer intervals, so this one takes very long
% (highlights the exponential runtime)
test_case(4, P1, P2, G, J) :-
    P1 = 2,
    P2 = 2,
    G = 10,
    J = [[[0, 10], 5, 10], [[2, 11], 4, 8], [[3, 12], 5, 6], [[4, 13], 4, 5],
         [[5, 15], 5, 3],  [[6, 16], 5, 4]].

% this one will probably never return  
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
    
% attack step or enum branching (compare with bisect)
test_case(7, P1, P2, G, J) :-
    P1 = 1000,
    P2 = 1000,
    G = 1,
    J = [[[0, 1000], 0, 1000], [[1, 2001], 0, 1000], [[2, 3002], 0, 1000]].


% For the uninitiated, Prolog is a "logic programming language".
% This essentially means that it is declarative (no control flow/loops)
% and functions/procedures are replaced with predicates.

% Predicates are written in lowercase and take the form
% > predicate(Variables, ...) :-
% >     body.
% or
% > fact(Variables, ...).

% Note that variables are _always_ capitalized.
% The body consists of comma separated statements and _always_ ends with a period (.).
% A statement can be a unification (LHS = RHS) or an applied predicate.

% A program is executed by querying a predicate,
% Prolog will then attempt to prove that predicate for the given variables,
% while instantiating the free variables.

% Example:
% > is_one(1).
% querying
% > is_one(X).
% results in
% > X = 1.

% Note: Prolog is lazy in the sense that
% > infinite_ones(X) :-
% >     X = [1|X].
% doesn't kill you on query but gives X = [1|X].

% To run, get SWI-Prolog here https://www.swi-prolog.org/