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
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |
| `-u` | `--ErrorOnUnknown` | Optional | (default True).  If on, will error out if any unknown records that are encountered |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |

!!! bug "Must Have Dedicated Folder"
    Make sure the output path is pointed to a folder which is -wholly- dedicated to containing Spriggit content.   [More Info](backups.md)

!!! bug "Starfield"
    Starfield must supply [Master Style Input](#master-style-input)
	
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
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |
| `-b` | `--BackupDays` | Optional | Days to keep backup plugins in the temp folder (default 30) |

!!! tip "Omit Package Details"
    Spriggit stores the package details it was created with, so in most circumstances, you want to let it automatically detect the package information.
	
!!! bug "Starfield"
    Starfield must supply [Master Style Input](#master-style-input)
	
[:octicons-arrow-right-24: Backups](backups.md)

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


## Pipeline commands

These are a collection of commands with the goal of helping transform mods in a way that help reduce "noise" or otherwise customize things during serialization.

They typically work in a "pipeline" where the first should be run with the original mod as input, and then output to a temporary location for the next one to read in as input.

The final capstone should then be the `serialize` command once all the other customization are done.

### Script Property Sorting
`sort-script-properties`

This command helps derandomize script properties, which often change order randomly after edits in the CK

#### Typical
`.\Path\To\Spriggit.CLI.exe sort-script-properties -i "C:\MyGitRepository\SomeMod.esp" -o "Some\Temp\Path\SomeMod.esp"`

#### Parameters
| Short | Long | Required | Description |
| ---- | ---- | ---- | ---- |
| `-i` | `--InputPath` | Required | Path to the Bethesda plugin folder as its Spriggit text representation |
| `-o` | `--OutputPath` | Required | Path to export the mod with the properties sorted |
| `-g` | `--GameRelease` | Semi-Optional | Game release that the plugin is related to.  Required if no `.spriggit` file is found. |
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference.  [Read More](#master-style-input)  |

## Master Style Input
Newer games, like Starfield, require extra inputs in order to translate.  These games need information from the source files of every master they list in a way that older games do not.  As such, you either need to provide:

- `-d` `--DataFolder` parameter pointing to a folder containing all of the master files, for reference.
- `Known Masters` within a [`.spriggit` file](spriggit-file.md#known-masters)

For command lines running as part of CI processes on servers without game information, the Known Master system can often be preferable to actually having the master files on hand.
