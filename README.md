# Axoloop Calculator

An open-source matrix and expression calculator for the terminal, written in C#.

It started as a sandbox for practising Clean Architecture in .NET and grew into
something genuinely useful — which turned out to be the interesting part. Every
feature had to earn its place in the layering, and the ones that didn't fit
revealed where the model was wrong. The code is public because that process is
worth reading, arguing with, and improving.

```
Axoloop> a := [4,7;2,6]
╭───┬───╮
│ 4 │ 7 │
│ 2 │ 6 │
╰───┴───╯

Axoloop> a * inverse(a)
╭───┬───╮
│ 1 │ 0 │
│ 0 │ 1 │
╰───┴───╯

Axoloop> solve(x^2 = 4, x)
x = -2
x = 2

Axoloop> f(x) := x^2
f(x) defined

Axoloop> ndiff(f)(3)
6

Axoloop> /plotweb ndiff(f) -10 10
```

## Features

**Expressions** — full precedence with right-associative exponentiation, unary
minus, postfix factorial, parentheses. Comparison and logical operators with
short-circuit evaluation.

**Values** — reals, booleans and matrices in a single polymorphic type system.
Variables persist across the session; mathematical constants are write-protected.

**User-defined functions** — `y(x) := 2*x + 5`, including recursion with `if`.
Functions are first-class values: a bare name like `y` evaluates to a callable,
so it can be passed to another function, returned from one, or called again
immediately — `ndiff(y)(3)` chains a second call directly onto the result of
the first.

**Linear algebra** — determinant, inverse, rank, trace, transpose, dot and cross
products, eigenvalues and eigenvectors, reshape, identity and fill constructors.

**Equation and linear-system solving** — `solve(equation, unknown)` numerically
finds the real roots of an equation written as-is (`solve(x^2 = 4, x)`), with an
explicit-domain overload to isolate one root among several. `linsolve` and
`linsolvegen` solve `a*x = b` by Gauss-Jordan elimination, covering all three
cases — a unique solution, no solution, or infinitely many, returned as a
particular solution plus a null-space basis; `rref` and `nullspace` expose the
underlying steps directly.

**Calculus** — `ndiff(f, x)` for the numerical first derivative, `ndiff(f, n, x)`
for the nth via higher-order finite-difference stencils, and `integral(f, a, b)`
for a definite integral by Simpson's rule. `ndiff(f)`, `integral(f)` and
`integral(f, a)` return a derivative or antiderivative as a callable function
rather than a single value, so it can be composed or plotted directly.

**Plotting** — ASCII rendering in the terminal, or an interactive HTML view with
zoom, pan, point inspection, and detected zeros and local extrema. `/plot`,
`/plotweb`, `/zeros` and `/extrema` accept any function-valued expression, not
just a defined name — `/plotweb ndiff(f) -10 10` plots a derivative directly.
`plot(f, xMin, xMax)` returns raw samples as a matrix instead of rendering.

**Built-in documentation** — every function carries its own signature, description
and examples; `/help <name>`, `/functions` and a generated web reference all come
from that single source.

## Getting started

Requires the .NET 10 SDK (a preview build was used during development).

```bash
git clone https://github.com/sidramon/Axoloop-Calculator.git
cd Axoloop-Calculator
dotnet run --project Presentation
```

Type `/help` to get started, `exit` to quit.

## Architecture

Four projects, with dependencies pointing strictly inward:

```
Presentation ──┐
               ├──> Application ──> Domain
Infrastructure ┘
```

**Domain** holds the language and the mathematics: tokenizer, parser, evaluator,
value types, operators, and numerical algorithms. No dependencies, no I/O.

**Application** orchestrates use cases and declares the contracts it needs
(`IPlotRenderer`, `IViewLauncher`, `IDocumentationRenderer`) without knowing how
they are fulfilled.

**Infrastructure** implements those contracts — HTML generation, file system,
process launching.

**Presentation** owns the REPL and every rendering decision. `Program.cs` is the
composition root, and the only file in the solution that sees concrete
implementations from every layer.

The practical consequence: swapping the HTML renderer for a PNG one, or the
console front-end for a web API, touches one project and leaves the language and
the mathematics untouched.

## Design decisions worth arguing about

Operators, functions and special forms are polymorphic and registered by symbol or
name, so adding an operation is a new class rather than an edit to the parser.

Function calls are resolved at evaluation time, not at parse time. This is what
allows user-defined functions to exist at all, and it keeps the parser free of any
function registry.

`if`, `and` and `or` cannot be ordinary functions: arguments are evaluated before a
call, which would defeat short-circuiting and make every recursion infinite. They
are dedicated AST nodes with lazy evaluation.

Floating-point noise is cleaned at display time only. `sin(pi)` returns `1.22e-16`
internally and shows as `0` — the domain keeps the honest value, the presentation
layer makes it readable.

Function scoping is dynamic rather than lexical: a function sees globals as they
are at call time. That suits a REPL, but it is a defensible thing to disagree with.

## Known limits

`diff` is the only exact, symbolic operation so far — everything else is
numeric, so no factoring or algebraic equation solving; `solve` and `integral`
both work by approximation rather than exact manipulation. `solve` scans a
fixed domain (`[-100, 100]` by default) for sign changes, so a periodic
equation reports at most the first 10 of its roots even when infinitely many
exist, and a root outside the scanned domain is simply not found. `ndiff`'s
approximate derivatives lose precision quickly past first order — expect
noticeably fewer correct digits at `ndiff(f, 2, x)` than at `ndiff(f, x)`, and
worse still at order 3 or 4 — because higher-order finite-difference stencils
amplify rounding error, which is also why orders above 4 are rejected outright.
`integral(f)` and `integral(f, a)`
return a callable antiderivative that reruns a full quadrature on every
invocation, so sampling it at many points (e.g. for a plot) is proportionally
expensive. No complex numbers, so `eigvals` refuses matrices with complex
eigenvalues and `sqrt(-1)` is an error; `eigvecs` goes further still and only
supports symmetric matrices. The web plot zooms over pre-computed samples, so
it cannot resolve detail past the sampled domain.

## Contributing

Suggestions, issues and pull requests are all welcome — including ones that
challenge the architecture rather than extend it. If you think a layer boundary is
in the wrong place or an abstraction isn't earning its keep, that's a conversation
worth having.

A few things that would be genuinely useful:

- **Complex numbers** — the largest missing piece, and the one that unblocks
  several others, including `eigvecs` on non-symmetric matrices
- **Wider symbolic layer** — `diff` covers exact differentiation; factoring and
  exact equation solving are still open, and `solve`/`integral` remain
  numeric-only
- **Numerical limits** — an `EquationSolver` (Newton-Raphson with a bisection
  fallback) already exists in `Domain/Calculator/Algorithms`, built originally
  for the since-removed `fsolve`; it has no caller today but is a reasonable
  starting point for a `lim` function
- **LU-based `inverse`/`linsolve`** — `det` now factors via LU with partial
  pivoting (`Algorithms/LuDecomposition.cs`); `inverse` and `linsolve` still use
  full Gauss-Jordan elimination and could reuse the same factorization with
  forward/back substitution
- **Statistics** — descriptive functions and distributions
- **More renderers** — PNG output, or a web front-end reusing the existing use cases

New functions should carry their own documentation (the `IFunction` contract
requires it) and come with tests. Numerical comparisons need explicit tolerance —
never assert exact equality on doubles.

Open an issue first for anything that touches the layering, so the discussion
happens before the code.

## Built with

[Spectre.Console](https://spectreconsole.net) for terminal rendering and
[ReadLine.Reboot](https://github.com/EoflaOE/ReadLine.Reboot) for line editing
and history.

## License

MIT
