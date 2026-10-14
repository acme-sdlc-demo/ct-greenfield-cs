# Project conventions (C# template)

Read this before changing anything. Rules marked *enforced* are checked by the platform
and fail the step if broken; the rest are checked by the reviewers.

## Stack and dependencies
- C# on .NET 8 (`net8.0`), nullable reference types on, warnings are errors (`Directory.Build.props`).
- Tests use xUnit. Add no package unless an approved design names it.

## Layout
- Code lives in `src/Core/` (namespace `Core`); add a project under `src/` only if an approved design names it,
  and add it to `Project.sln`.
- Tests live in `tests/Core.Tests/`, one `<Type>Tests.cs` file per type under test.

## Coding conventions
- One public type per file, named as the file. Give every public member an XML doc comment (`///`).
- Validate arguments where they enter: throw `ArgumentNullException`, `ArgumentException` or
  `ArgumentOutOfRangeException` naming the parameter (`nameof`); never return `null` or `-1` to signal an error.
- Prefer immutable types (`record`, `readonly`, `init`); no mutable static state, no `Console` in library code,
  no empty `catch`.
- Whitespace as `.editorconfig` says (*enforced*: `dotnet format whitespace` in the gate).

## Testing
- Every public member has xUnit tests for normal cases, edge cases and each exception it throws.
- Tests are deterministic: no network, no sleeps, no dependence on the clock or test order.
- Never delete, skip or weaken an existing test to make the checks pass (*enforced*: protect_tests).
- `dotnet build` and `dotnet test` pass (*enforced*: the Self-test gate).

## Boundaries
- Change only the files your requirement needs; other developers work on this repo in parallel.
- Don't edit CLAUDE.md, `Directory.Build.props`, `.editorconfig`, project files' settings or CI config unless the
  requirement is about them.
- Don't commit or merge; the platform commits your changes and merges approved work.

## Security
- No secrets, tokens or credentials in code, tests or logs.
- No `Process.Start` with interpolated input, no `BinaryFormatter`, no MD5 or SHA1 for security, no string-built SQL.
