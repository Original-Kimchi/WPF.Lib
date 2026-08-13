# Repository Guidelines

## Project Structure & Module Organization

The repository contains a .NET 8 WPF application and reusable theme and MVVM libraries. `WPFControls.UI.sln` is the solution entry point. `WPFControls.UI/` contains the application, while `WPF.Lib/WPF.Lib.Theme/` and `WPF.Lib/WPF.Lib.MVVM/` contain the reusable libraries. Light and dark resource dictionaries live under `WPF.Lib/WPF.Lib.Theme/Theme/`. Keep both theme files aligned when adding resource keys. Application startup lives in `App.xaml` and `App.xaml.cs`; the initial window is defined by `MainWindow.xaml` and its code-behind. Keep each control's XAML and code-behind together and use feature-oriented folders such as `Controls/`, `ViewModels/`, and `Converters/` as the application grows. There is currently no test project.

## Build, Test, and Development Commands

Run commands from the repository root:

```powershell
dotnet restore WPFControls.UI.sln
dotnet build WPFControls.UI.sln --configuration Debug
dotnet run --project WPFControls.UI/WPFControls.UI.csproj
dotnet test WPFControls.UI.sln
dotnet format WPFControls.UI.sln --verify-no-changes
```

`restore` resolves NuGet dependencies, `build` compiles the solution, and `run` launches the desktop application on Windows. `test` runs all test projects once they exist. Use `dotnet format` before submitting changes; omit `--verify-no-changes` to apply safe formatting fixes.

## Coding Style & Naming Conventions

Use four spaces in C# and XAML; do not use tabs. Retain nullable reference types and implicit usings. Follow standard .NET naming: `PascalCase` for namespaces, types, methods, properties, and named XAML elements; `camelCase` for parameters and locals; `_camelCase` for private fields. Name controls and paired files consistently, for example `NumericInput.xaml` and `NumericInput.xaml.cs`. Prefer bindings and commands over event-heavy code-behind, and keep reusable brushes, styles, and templates in resource dictionaries.

When creating or changing a themed control style, verify it in both light and dark themes. Check foreground/background contrast for the control and all template parts, including selected, hover, focused, disabled, popup, and nested-item states. Template text must inherit or explicitly bind to the appropriate dynamic theme foreground brush; do not rely on the WPF system default foreground.

## Testing Guidelines

Add tests in a sibling project such as `WPFControls.UI.Tests/`, named after the production assembly. Prefer xUnit unless the solution adopts another framework. Name test classes `<TypeName>Tests` and tests `Method_Scenario_ExpectedResult`. Keep UI-independent behavior in view models or services so it can be tested without launching WPF. New behavior and bug fixes should include focused tests where practical.

## Commit & Pull Request Guidelines

History currently contains only `Initial commit`, so no established convention exists. Use concise, imperative subjects such as `Add numeric input validation`; keep each commit focused. Pull requests should explain intent, summarize verification commands, link relevant issues, and include screenshots or a short recording for visible UI changes. Call out breaking API, resource, or configuration changes explicitly.
