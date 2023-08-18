# Spriggit
A tool to facilitate converting Bethesda plugin files to a text based format that can be stored in Git.   Large scale projects can then live in Github, and accept Pull Requests from many developers.

The goal is to help modders store their files in a versioning system that allows them to easily iterate in the same way that programmers do with their code.   

![https://youtu.be/VgJaCaZSh98](https://i.imgur.com/ATjXyFT.png)

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
[ToDo]

# Spriggit Translation Packages
The logic for actually doing the translation to/from an esp is not housed or packaged directly with the CLI or UI.   Rather, the logic exists in NuGet packages that are downloaded and then used to do the translation.

## Mechanics
When Spriggit is asked to do a serialization from a Bethesda plugin file to text format, you must provide it with the name of a `NuGet` package to use for the translation.   If a specific version is supplied, it will use that, otherwise it will grab the latest version of that package.
As it exports the text files, the NuGet package name and version are included.

When Spriggit is asked to do a deserialization from text to a Bethesda Plugin, no specification of a `NuGet` package is needed.  Rather, it looks in the text files for the listed package + version that was used to make the files in the first place, and downloads that package to use.

As such, a Spriggit CLI/UI can translate to/from many different translation formats/styles, rather than being bound to the translation logic that existed when the CLI/UI was built.

## Reasoning
This separation is an important aspect of keeping Spriggit flexible as record definitions evolve.   

Especially early on during a game's release, the record definitions are constantly being upgraded, adjusted, and fixed.  The separation allows Spriggit to always grab the older version of a translation, and use that to deserialize text that contains older definitions.

If very old versions get deserialized, the older nuget packages will be downloaded and used to read them.   And, if the user re-serializes them with the latest nuget packages, they will be "upgraded" to the latest text definitions.

### Example
Let's take a simple example of a typo.   Let's say `Haelth` was accidentally used as a field name in `v1.1` of Spriggit, and a plugin was serialized into text with that typo.
Eventually someone notices, and fixes it to `Health` in `v1.2`.  How does the old file that contained `Haelth` get properly read anymore?

It will be able to be read, because the original file will have been stamped with `v1.1`.  During deserialization of the file, Spriggit will see that and download that `v1.1` nuget package, and use that to read the file.   The `Haelth` field will be read in as expected by the `v1.1` package, yielding a proper esp output.

## Customization
Spriggit uses [Mutagen](https://github.com/Mutagen-Modding/Mutagen) systems under the hood, and leans on the [Mutagen.Bethesda.Serialization](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Serialization) library to convert to/from Yaml and Json.

One of the features of [Mutagen.Bethesda.Serialization](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Serialization) is that it allows for customization of naming, file structure, and other similar things.   If you utilize this to make your own customization, you will need to upload the results to nuget.org for people to grab and use.

More documentation will follow on how to upload your own customization package to Nuget so that it can be used by Spriggit.    For now, two packages exist "built in":
- `Spriggit.Yaml.[GameName]`
- `Spriggit.Json.[GameName]`

These are what will be used to do any translations, unless the user specifies the name of a customization package they wish to use instead.
