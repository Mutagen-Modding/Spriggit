# Spriggit
A tool to facilitate converting Bethesda plugin files to a text based format that can be stored in Git.  
The goal is to help modders store their files in a versioning system that allows them to easily iterate in the same way that programmers do with their code.   Large scale projects can then live in Github, and accept Pull Requests from many developers. 

# Reasons to Use Git for Mods
Git is an extremely powerful versioning and iteration tool that almost all programmers use when working.  It's what powers the world of coding to be able to iterate new code quickly and collaborate easily.  

Some things Git can help you do when developing your mod:
- Keep track of the many versions of your mod, without resorting to Dropbox folder hell.
- Create a living "changelog" as you work
- Be able to go back in time and view your mod exactly as it was at any point in history
- Stamp your mod with version tags, letting you see how it looked at any one from the past
- Easily experiment on side branches without worrying your stable setup
- Share your work on Github, allowing people to see your mods development progress
- Collaborate easily, by allowing others to contribute to your mod via Pull Requests (just like coders do)
- More easily merge the work of multiple developers 

# The Workflow:
Spriggit helps facilitate a workflow that coders will be very familiar with

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

One of the features of [Mutagen.Bethesda.Serialization](https://github.com/Mutagen-Modding/Mutagen.Bethesda.Serialization) is that it allows for customization, whether that's changing names, or file structures.  Spriggit allows users to translate with these customizations,
allowing the files as they exist in Git to look differently.   However, these customizations must live as a Nuget package on Nuget.org, so that future users looking to convert from Git back to Bethesda Plugins will always be able to locate the customizations that were used, 
and so be able to convert back.  When you use Spriggit, you will have to specify with Translation Package you want to use, and that will be marked in the files so users can locate it later.

More documentation will follow on how to upload your own customization package to Nuget so that it can be used by Spriggit.    For now, two packages exist "built in":
- `Spriggit.Yaml`
- `Spriggit.Json`

