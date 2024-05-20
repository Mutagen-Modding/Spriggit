# Spriggit Translation Packages
The logic for actually doing the translation is not housed or packaged directly with the CLI or UI.   Rather, the logic exists in NuGet packages that are downloaded and then used to do the translation.

## Mechanics
When Spriggit is asked to do a serialization from a Bethesda plugin file to text format, it takes the supplied Nuget package name and version and downloads that package.  It then uses the libraries within that package to do the translations.

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