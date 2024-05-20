# .spriggit File

By default, the Translation Package and Version to use is set by the user when doing the translation call (either by UI or CMD).   In environments with multiple developers, though, this might be undesirable as each developer might choose different settings when working.

A dedicated `.spriggit` file can be defined at or above the spriggit folder in the repository, which provides a centralized location that the package details can be set.  These specifications are automatically picked up and used by Spriggit, keeping the settings coordinated even with many developers.

## Creating
Right now the only way to make this file is by hand, but it is fairly trivial.

Create a file named `.spriggit` with the desired content in this format:
```
{
  "PackageName": "Spriggit.Yaml",
  "Release": "Starfield",
  "Version": "0.18"
}
```

[:octicons-arrow-right-24: Game Releases](https://github.com/Mutagen-Modding/Mutagen/blob/dev/Mutagen.Bethesda.Kernel/GameRelease.cs)

This file should be next to, or above the Spriggit mod folder(s) you want it to affect.

## Updating
To update to a different version, simply modify the file to the desired version, and then run any spriggit commands.