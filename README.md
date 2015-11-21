# Stylize
Stylize is designed to simplify style enforcement by transitioning from violation detection to automatic enforcement.  It uses Roslyn (Microsoft.CodeAnalysis) to scan for and fix any potential violations in a .NET solution.  Additionally, style enforcement may be skipped for documents which match custom criteria (e.g., by file name, project name, presence of attribute(s), presence of pending changes, semantically invalid code, or user-defined criteria).

## Getting Started
1. Clone the repository and build Stylize
2. Run bin/[Debug|Release]/Product/Stylize/Stylize.exe (see also [Sample Configuration](https://github.com/nicholjy/stylize/wiki/Sample-Configuration))

## Current Rule Set
#### C# Rules
* Convert ambiguous implicit variables to explicit types in local declarations
* Convert overly-explicit local declaration types to implicit variables
* Add or remove explicit default access modifiers
* Add or remove a header comment
* Remove duplicate new lines
* Apply VS-based space formatting (equivalent of Edit->Advanced->Format Document)
* Simplify unnecessarily qualified types (by adding imports or using existing imports/aliases)
* Remove unnecessary imports (usings)
* Sort imports (usings)

## Future Work
* MSBuild integration to run Stylize as part of the build process
* Support for other source repositories
