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
| `-g` | `--GameRelease` | Semi | Game release that the plugin is related to.  Required if no `.spriggit` file is found. |
| `-p` | `--PackageName` | Optional | Spriggit serialization nuget package name to use for conversion |
| `-v` | `--PackageVersion` | Optional | Spriggit serialization nuget package version to use for conversion |
| `-t` | `--Threads` | Optional | Maximum number of threads to use |
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference during Separated Master game mods (Starfield) |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |

!!! error "Must Have Dedicated Folder"
    Make sure the output path is pointed to a folder which is -wholly- dedicated to containing Spriggit content.   [More Info](backups.md)

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
| `-d` | `--DataFolder` | Semi-Optional | Provides a path to the data folder for reference during Separated Master game mods (Starfield) |
|      | `--Debug` | Optional | Set up for debug mode, including resetting nuget caches |
| `-b` | `--BackupDays` | Optional | Days to keep backup plugins in the temp folder (default 30) |

!!! tip "Omit Package Details"
    Spriggit stores the package details it was created with, so in most circumstances, you want to let it automatically detect the package information.
	
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



 