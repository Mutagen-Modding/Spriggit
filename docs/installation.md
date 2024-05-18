# Installation
## Spriggit UI
This is a self-contained WPF application, which can only run on Windows.  You can add links between a mod file and where it should be translated to, and then sync back and forth with one click.

![Spriggit UI](images/Spiggit-ui-1.png)
![Spriggit UI](images/Spiggit-ui-2.png)

## Spriggit CLI
Spriggit comes with a Command Line Interface that can be used to convert from Betheseda Plugins to Git Repositories, and back.   Note that the UI can also accept these CLI commands, and so this variant is meant for linux machines that cannot handle the UI.   It is not self contained, so it might complain about needing a specific .NET runtime downloaded.

This would use Spriggit to convert from a SkyrimSE mod to Yaml, and put it in your Git Repository.

`.\Path\To\Spriggit.CLI.exe serialize --InputPath "C:\Games\steamapps\common\Skyrim Special Edition\Data\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp" --GameRelease SkyrimSE --PackageName Spriggit.Yaml`

And this would convert it back and overwrite the original mod file.

`.\Path\To\Spriggit.CLI.exe deserialize --InputPath "C:\Users\Levia\Downloads\SpriggitOutput\SomeMod.esp" --OutputPath "C:\MyGitRepository\SomeMod.esp"`
