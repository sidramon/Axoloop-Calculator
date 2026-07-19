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

Axoloop> f(x) := sin(x) / x
f(x) defined

Axoloop> /plotweb f -10 10
```

## Features

**Expressions** — full precedence with right-associative exponentiation, unary
minus, postfix factorial, parentheses. Comparison and logical operators with
short-circuit evaluation.

**Values** — reals, booleans and matrices in a single polymorphic type system.
Variables persist across the session; mathematical constants are write-protected.

**User-defined functions** — `y(x) := 2*x + 5`, including recursion with `if`.

**Linear algebra** — determinant, inverse, rank, trace, transpose, dot and cross
products, eigenvalues and eigenvectors, reshape, identity and fill constructors.

**Plotting** — ASCII rendering in the terminal, or an interactive HTML view with
zoom, pan, point inspection, and detected zeros and local extrema.

**Built-in documentation** — every function carries its own signature, description
and examples; `/help <name>`, `/functions` and a generated web reference all come
from that single source.

## Getting started

Requires the .NET 8 SDK.

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

No complex numbers, so `eigvals` refuses matrices with complex eigenvalues and
`sqrt(-1)` is an error. Everything is numeric — there is no symbolic layer, so no
algebraic differentiation, factoring or `solve`. `det` uses cofactor expansion,
which is fine for hand-typed matrices and unusable beyond roughly 10×10. The web
plot zooms over pre-computed samples, so it cannot resolve detail past the sampled
domain.

## Contributing

Suggestions, issues and pull requests are all welcome — including ones that
challenge the architecture rather than extend it. If you think a layer boundary is
in the wrong place or an abstraction isn't earning its keep, that's a conversation
worth having.

A few things that would be genuinely useful:

- **Complex numbers** — the largest missing piece, and the one that unblocks
  several others
- **Symbolic layer** — derivatives, factoring, equation solving
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
