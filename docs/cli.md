# Command Line Interface
Spriggit comes with a Command Line Interface that can be used to convert from Betheseda Plugins to Git Repositories, and back. 

These commands have parameters that are optional only if a .spriggit file is found.

[:octicons-arrow-right-24: .spriggit File](spriggit-file.md)

## Serialize | Convert From Plugin
`serialize`, or `convert-from-plugin`

This converts from a Bethesda Plugin mod to Yaml, and puts it in your Git Repository.

### Typical
`.\Path\To\Spriggit.CLI.exe convert-from-plugin --InputPath "C:\Games\steamapps\common\Skyrim Special Edition\Data\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp" --GameRelease SkyrimSE --PackageName Spriggit.Yaml`

### Parameters

| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-i` | `--InputPath` | Required | Path to the Bethesda plugin (esp/esm) |
| `-o` | `--OutputPath` | Required | Dedicated folder to export mod as its text representation |
| `-g` | `--GameRelease` | Semi-Optional | Game release that the plugin is related to.  Required if no `.spriggit` file is found. |
| `-p` | `--PackageName` | Semi-Optional | Spriggit serialization nuget package name to use for conversion.  Required if no `.spriggit` file is found. |
| `-v` | `--PackageVersion` | Optional | Spriggit serialization nuget package version to use for conversion |
| `-t` | `--Threads` | Optional | Maximum number of threads to use |
| `-m` | `--ModKey` | Optional | ModKey override.  If left blank, the input file name is used |
| `-c` | `--Check` | Optional | (default True).  Checks some basic correctness after serialization.  Not extensive. |
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |
| `-u` | `--ErrorOnUnknown` | Optional | (default True).  If on, will error out if any unknown records that are encountered |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |

!!! bug "Must Have Dedicated Folder"
    Make sure the output path is pointed to a folder which is -wholly- dedicated to containing Spriggit content.   [More Info](backups.md)

!!! bug "Starfield"
    Starfield must supply [Master Style Input](#master-style-input)
	
The valid list of GameReleases are listed [here](https://github.com/Mutagen-Modding/Mutagen/blob/dev/Mutagen.Bethesda.Kernel/GameRelease.cs) but are generally as follows:

- Oblivion
- OblivionRE
- Fallout3
- SkyrimLE
- SkyrimSE
- SkyrimVR
- Fallout4
- Fallout4VR
- SkyrimSEGog
- Starfield

`PackageName` and `PackageVersion` are both driven by what [Translation Package](translation-packages.md) you want to use to do the translation.  Each translation is a NuGet package with a name and a version, which you specify in the appropriate fields.  For the built in Spriggit Translation Packages `Spriggit.Yaml` and `Spriggit.Json`, the `.[GameName]` suffix can be omitted, as it's implied by the GameRelease parameter.   For non-standard 3rd party packages, the full NuGet package name is required.

## Deserialize | Convert To Plugin
`deserialize`, `convert-to-plugin`, `create-plugin`

This converts from a folder in your Git Repository to a Bethesda Plugin.

### Typical
`.\Path\To\Spriggit.CLI.exe convert-to-plugin --InputPath "C:\Users\Levia\Downloads\SpriggitOutput\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp"`

### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-i` | `--InputPath` | Required | Path to the Bethesda plugin folder as its Spriggit text representation |
| `-o` | `--OutputPath` | Required | Path to export the mod as its Bethesda plugin representation |
| `-p` | `--PackageName` | Optional | Spriggit serialization nuget package name to use for conversion.  Leave blank to auto detect |
| `-v` | `--PackageVersion` | Optional | Spriggit serialization nuget package version to use for conversion.  Leave blank to auto detect |
| `-t` | `--Threads` | Optional | Maximum number of threads to use |
| `-l` | `--Localized` | Optional | Forces the build to be localized if true, or unlocalized if false.  If missing, the mod's flags determine localization. |
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |
| `-b` | `--BackupDays` | Optional | Days to keep backup plugins in the temp folder (default 30) |

!!! tip "Omit Package Details"
    Spriggit stores the package details it was created with, so in most circumstances, you want to let it automatically detect the package information.
	
!!! bug "Starfield"
    Starfield must supply [Master Style Input](#master-style-input)
	
[:octicons-arrow-right-24: Backups](backups.md)

## Upgrade Spriggit Version
`upgrade`

This command upgrades existing Spriggit files to a newer package version. It deserializes the mod files using the current version, updates the spriggit-meta.json to the specified version, and re-serializes the files with the new translation package.

### Typical
`.\Path\To\Spriggit.CLI.exe upgrade -p "C:\MyGitRepository\SomeMod.esp\" -v "1.2.3"`

### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-p` | `--SpriggitPath` | Required | Path to the Bethesda plugin folder as its Spriggit text representation |
| `-v` | `--PackageVersion` | Required | Spriggit serialization nuget package version to upgrade to |
| `-d` | `--DataFolder` | Semi-Optional | Path to the data folder for reference. [Read More](#master-style-input) |
| `-s` | `--SkipGitOperations` | Optional | Skip git operations (don't check for uncommitted changes or auto-commit) |

!!! warning "Backup Recommended"
    It's recommended to backup your Spriggit files before upgrading, as the process involves deserializing and re-serializing your mod data.

!!! tip "Git Integration"
    By default, the upgrade command checks for uncommitted changes before starting and automatically commits the upgrade changes when complete. Use `--SkipGitOperations` to disable this behavior if you want to manage git operations manually.

!!! bug "Starfield"
    Starfield must supply [Master Style Input](#master-style-input)

## FormID Collision Fixing
`formid-collision`

This command helps detangle colliding FormIDs that result after a Git Merge.

[:octicons-arrow-right-24: FormID Collision](merge-conflicts.md#formid-collision)

!!! bug "Two Collisions Maximum"
    The logic that Spriggit contains to handle FormID conflicts can only handle two records with a single FormID.  As such, collisions need to be handled immediately after each Git merge.

### Typical
`.\Path\To\Spriggit.CLI.exe formid-collision -p "C:\MyGitRepository\SomeMod.esp\"`

### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-p` | `--SpriggitPath` | Required | Path to the Bethesda plugin folder as its Spriggit text representation |
| `-d` | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |

## Merge Version Syncer
`merge-version-syncer`

This command is run after a Git merge to reconcile differing Spriggit versions between the two merge parents, re-serializing the files to a single consistent version.

[:octicons-arrow-right-24: Merge Conflicts](merge-conflicts.md)

### Typical
`.\Path\To\Spriggit.CLI.exe merge-version-syncer -p "C:\MyGitRepository\SomeMod.esp\"`

### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-p` | `--SpriggitPath` | Required | Path to the Bethesda plugin folder as its Spriggit text representation |
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |

## Standardize
`standardize`

!!! warning "Advanced / Testing Command"
    This is a testing utility used to standardize a plugin for binary comparison.  It is not part of the typical serialize/deserialize workflow, and most users will not need it.

This reads a Bethesda plugin and writes out a standardized (record-sorted) copy, which is useful when diffing two plugins at the binary level.

### Typical
`.\Path\To\Spriggit.CLI.exe standardize -i "C:\Games\...\SomeMod.esp" -o "C:\SomeMod.standardized.esp" -g SkyrimSE`

### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-i` | `--InputPath` | Required | Path to the Bethesda plugin (esp/esm) |
| `-o` | `--OutputPath` | Required | Path to output the standardized Bethesda plugin |
| `-g` | `--GameRelease` | Required | Game release that the plugin is related to |

## Master Style Input
Newer games, like Starfield, require extra inputs in order to translate.  These games need information from the source files of every master they list in a way that older games do not.  As such, you either need to provide:

- `-d` `--DataFolder` parameter pointing to a folder containing all of the master files, for reference.
- `Known Masters` within a [`.spriggit` file](spriggit-file.md#known-masters)

For command lines running as part of CI processes on servers without game information, the Known Master system can often be preferable to actually having the master files on hand.
