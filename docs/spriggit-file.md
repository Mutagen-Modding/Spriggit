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

## Known Masters
`.spriggit` files can also supply extra information to help with Starfield translations in the form of Known Masters.

Since Starfield requires extra information about all listed masters, typically it needs those master files present so they can be read in and referenced.   However, when running Spriggit translations on a server, those master files might not be available.

In situations like these, you can list the master files and their "Master Style" within a `.spriggit` file to provide the necessary information, without the actual master mod files needing to be present.

```
{
   "KnownMasters": 
   [
       {
	       "ModKey": "Starfield.esm",
		   "Style": "Full"
	   }
   ]
}
```

Where the options are:
- Full
- Medium
- Small

These correspond to the type of master the file it is, and the FormID master index patterns it was saved with.