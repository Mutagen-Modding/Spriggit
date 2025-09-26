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

## Accessing Source Generator Generated Files

The translation packages use Mutagen.Bethesda.Serialization.SourceGenerator to generate serialization code for all Bethesda game records. By default, these generated files exist only during compilation and are not accessible on disk.

### Enabling Generated File Access

All translation package projects have been configured with MSBuild properties to write generated files to disk:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### Generated File Structure

After building a translation package, generated files are available in the `GeneratedFiles/` folder:

```
Translation Packages/Spriggit.Yaml.Starfield/GeneratedFiles/
├── Mutagen.Bethesda.Serialization.SourceGenerator/
│   └── Mutagen.Bethesda.Serialization.SourceGenerator.Serialization.SerializationSourceGenerator/
│       ├── Perk_Serializations.g.cs
│       ├── MutagenYamlConverter_StarfieldMod_MixIns.g.cs
│       ├── Armor_Serializations.g.cs
│       ├── Weapon_Serializations.g.cs
│       └── [2000+ other record type serialization files]
└── Noggog.SourceGenerators/
    └── [Assembly version files]
```

### Key Generated Files

- **`[RecordType]_Serializations.g.cs`**: Complete serialization/deserialization logic for each game record type
- **`MutagenYamlConverter_[Game]Mod_MixIns.g.cs`**: Extension methods for the main converter classes
- **Individual record serializers**: Each Bethesda record type (Perk, Armor, Weapon, Quest, etc.) gets its own serialization file

### Usage Notes

- Generated files are automatically excluded from source control via `.gitignore`
- Files are regenerated on each build to reflect any changes in the source generator
- Useful for debugging serialization issues or understanding how records are processed
- All generated code includes comprehensive error handling and cancellation support

### Example Generated Code Structure

Each record serialization file contains:
- `Serialize<TKernel, TWriteObject>()` - Writes record to YAML/JSON
- `Deserialize<TReadObject>()` - Reads record from YAML/JSON
- `HasSerializationItems()` - Determines if record needs serialization
- `DeserializeInto<TReadObject>()` - Deserializes into existing object

## Development Best Practices

**CRITICAL: Always build and run tests after implementing changes to confirm correctness:**

```bash
# After making any code changes, ALWAYS follow these steps in order:

# 1. Build to check for compilation errors
dotnet build

# 2. Run ALL relevant tests (not just one) to verify functionality
dotnet test --filter "YourTestClassName"

# 3. Verify that ALL tests pass - any failing tests indicate incomplete work
# 4. If tests fail, fix them before claiming the work is complete
# 5. Never consider work "successful" when tests are failing
```

**Test Verification Requirements:**
- **ALL tests in the affected area must pass** - partial success is not success
- **Run the full test suite for the component you're working on**, not just individual tests
- **Fix failing tests immediately** - do not ignore or postpone test failures
- **Verify tests both compile AND pass execution** - compilation success alone is not sufficient
- **Test failures indicate incomplete or incorrect implementation** - address the root cause

**Common Mistakes to Avoid:**
- ❌ Only running one test and assuming others work
- ❌ Ignoring test failures and claiming success
- ❌ Only checking that code compiles without verifying runtime behavior
- ❌ Making assumptions about test state without verifying

**Correct Approach:**
- ✅ Run complete test suite for the area being modified
- ✅ Ensure 100% of relevant tests pass before completing work
- ✅ Fix any failing tests as part of the implementation task
- ✅ Verify both compilation and runtime test execution success

This is critical to ensure:
- Code compiles without errors
- New functionality works as expected
- Existing functionality hasn't been broken by changes
- Tests themselves are correctly written and can execute
- **All functionality is actually working, not just appearing to work**