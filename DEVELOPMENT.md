# Development Notes

This file contains helpful information for developers and AI assistants working on this codebase.

## Project Overview

Spriggit is a tool that converts Bethesda plugin files (.esp/.esm) to text-based formats (YAML/JSON) for version control with Git. It enables modders to collaborate on Bethesda mods using standard Git workflows.

### Core Architecture

The solution follows a layered architecture:

- **Spriggit.Core**: Core models and interfaces (SpriggitSource, etc.)
- **Spriggit.Engine**: Main conversion engine that orchestrates NuGet package downloads and translation
- **Spriggit.CLI**: Command-line interface executable
- **Spriggit.CLI.Lib**: Shared CLI library logic
- **Spriggit.UI**: WPF-based Windows GUI application
- **Translation Packages**: Game-specific serialization packages for each Bethesda game (Skyrim, Oblivion, Fallout 4, Starfield) in both YAML and JSON formats

The system uses a plugin architecture where translation logic is distributed as NuGet packages rather than being embedded in the CLI/UI, allowing for versioned translations and backward compatibility.

## Build Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build Spriggit.CLI/Spriggit.CLI.csproj

# Build release version
dotnet build -c Release

# Pack NuGet packages
dotnet pack
```

## Test Commands

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test Spriggit.Tests/Spriggit.Tests.csproj

# Run with verbose output
dotnet test -v normal
```

## Project Structure

- `/Translation Packages/`: Contains game-specific serialization packages
  - `Spriggit.Yaml.[Game]`: YAML serialization for each supported game
  - `Spriggit.Json.[Game]`: JSON serialization for each supported game
- `/docs/`: MkDocs documentation site
- Solution uses Central Package Management with `Directory.Packages.props`
- All projects target .NET 9.0
- Uses Mutagen.Bethesda libraries for Bethesda game file handling

## Key Dependencies

- **Mutagen.Bethesda**: Core library for handling Bethesda game files
- **Mutagen.Bethesda.Serialization**: Handles YAML/JSON conversion
- **NuGet.Protocol**: For dynamic package downloading
- **CommandLineParser**: CLI argument parsing
- **ReactiveUI/WPF**: For the UI project
- **xUnit**: Testing framework

## Development Notes

- The engine dynamically downloads translation packages from NuGet at runtime
- Game releases supported: Oblivion, Skyrim (LE/SE/VR/GOG), Fallout 4, Starfield
- Each serialization creates a `.spriggit` metadata file tracking the package version used
- Starfield requires "Master Style Input" - either DataFolder parameter or Known Masters in .spriggit file
- Pre-build targets clear local NuGet cache for translation packages to ensure fresh downloads during development