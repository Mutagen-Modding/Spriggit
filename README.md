# Spriggit
A tool to facilitate converting Bethesda plugin files to a text based format that can be stored in Git.   Large scale projects can then live in Github, and accept Pull Requests from many developers.

The goal is to help modders store their files in a versioning system that allows them to easily iterate in the same way that programmers do with their code.   

# Reasons to Use Git for Mods
Git is an extremely powerful versioning and iteration tool that almost all programmers use when working.  It's what powers the world of coding to be able to iterate new code quickly and collaborate easily.  

Some things Git can help you do when developing your mod:
- Keep track of the many versions of your mod, without resorting to Dropbox folder hell.
- Create a living "changelog" as you work
- Be able to go back in time and view your mod exactly as it was at any point in history
- Stamp your mod with version tags, letting you see how it looked at any one from the past
- Easily experiment on side branches without worrying your stable setup
- Share your work on Github, allowing people to see your mod's development progress
- Collaborate easily, by allowing others to contribute to your mod via Pull Requests
- More easily merge the work of multiple developers with Git Merge technology

# The Workflow:
## An Individual Modder
- Create a Git Repository to hold your mod
- Create a Bethesda plugin with existing normal tools of choice
- Use Spriggit to convert the `.esp/m/l` files from your Bethesda workspace, to `.yaml` or `.json` files inside your Git Repository
- Make commits in Git.  
  "Added all the bandit Npc definitions"
  "Fixed the Powerblade damage to be more reasonable"
- Upload your mod, in its text format, up to Github (or your host of preference)

## Many Collaborators
Other modders, whether on your team or just helpful people out in the world can help collaborate and participate in your mod's development.
- They can clone the mod via Git to their computers
- Use Spriggit to convert from the `.yaml` or `.json` files to a Bethesda plugin
- Open the Bethesda plugin with the game, or other tools
- Modify the mod and help work on something
- Use Spriggit to convert back to text format
- Make commits in Git
- Upload their improvements to Github
- Initiate a Pull Request to ask that you consider their changes
- You can discuss with them about further changes, or merge their improvements into your mod

# Example Output
## Record Data is Stored as Plaintext
Json or Yaml formatting is currently supported.

Here is a snippet of what a record file might look like if Yaml output is used:
```yaml
FormKey: 087835:Skyrim.esm
EditorID: JewelryNecklaceGoldGems
ObjectBounds:
  First: -3, -9, 0
  Second: 3, 9, 1
Name: Gold Jeweled Necklace
WorldModel:
  Male:
    Model:
      File: Armor\AmuletsandRings\GoldAmuletGemsGO.nif
      Data: 0x020000000300000000000000A4E51E5364647300D8C674AFC031228D64647300D8C674AFB8EC307B64647300262C333B
PickUpSound: 08AB15:Immersive Sounds - Compendium.esp
PutDownSound: 08AB16:Immersive Sounds - Compendium.esp
Race: 013749:Skyrim.esm
Keywords:
- 06BBE9:Skyrim.esm
- 08F95A:Skyrim.esm
- 0A8664:Skyrim.esm
- 10CD0A:Skyrim.esm
Description: ''
Armature:
- 09171F:Skyrim.esm
Value: 485
Weight: 0.5
```

This file is more palatable to Git and can support diff tools and similar functionality.

## Mods are Split into Folders
Rather than having one large file of all of a mod's data, Spriggit splits a mod into a folder of Yaml/Json files.

A typical mod folder structure might look like:
```
Some/Folder/
   RecordData.yaml          -  The mod header
   Weapons/                 -  Folder for all the weapons
      GlassDagger.yaml      -  File dedicated to the record Glass Dagger
      IronLongsword.yaml    -  Seperate file for the Iron Longsword
   Npcs/                    -  Folder for all the weapons
      Goblin.yaml           -  File dedicated to the Goblin's data
```

This folder structure helps organize git diffs to be more meaningful.  If a new record is added, then this will be seen as a new file.   If a record is modified, it will be a modified file.   Similar to wanting to avoid having a program's code be in one large monolith file, having smaller bite sized files helps navigate and digest changes being made.

# Spriggit CLI
Spriggit comes with a Command Line Interface that can be used to convert from Betheseda Plugins to Git Repositories, and back.

This would use Spriggit to convert from a SkyrimSE mod to Yaml, and put it in your Git Repository.

`.\Path\To\Spriggit.CLI.exe serialize --InputPath "C:\Games\steamapps\common\Skyrim Special Edition\Data\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp" --GameRelease SkyrimSE --PackageName Spriggit.Yaml`

And this would convert it back and overwrite the original mod file.

`.\Path\To\Spriggit.CLI.exe deserialize --InputPath "C:\Users\Levia\Downloads\SpriggitOutput\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp"`

# Spriggit UI
In development, and should be released soon.   Will be a convenient UI to help convert mods back and forth without using the command line.

# Spriggit Translation Packages
Spriggit uses [Mutagen](https://github.com/Mutagen-Modding/Mutagen) systems under the hood, and leans on the [Mutagen.Bethesda.Serialization](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Serialization) library to convert to/from Yaml and Json.

One of the features of [Mutagen.Bethesda.Serialization](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Serialization) is that it allows for customization of naming, file structure, and other similar things.  

Spriggit allows users to translate with these customizations, allowing the files as they exist in Git to look differently.   However, it restricts that these customizations must live as a Nuget package on Nuget.org, so that future users looking to convert from Git back to Bethesda Plugins will always be able to locate the customizations that were used, and so be able to convert back.

When you use Spriggit, you will have to specify with Translation Package you want to use, and that will be marked in the files so users can locate it later.

More documentation will follow on how to upload your own customization package to Nuget so that it can be used by Spriggit.    For now, two packages exist "built in":
- `Spriggit.Yaml`
- `Spriggit.Json`

