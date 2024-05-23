# Unexpected Records

Especially in the early days of a new game, you might see Spriggit refuse to serialize a mod with the verbage of "unexpected records".

This is intentional as a safety mechanism to help avoid data loss.   If Spriggit encounters a record that it is unsure of, it refuses to proceed, rather than skip the record.

## Solution

Running into this error usually means that Spriggit's definitions need to be updated.  You can report unexpected records to the [Spriggit](https://github.com/Mutagen-Modding/Spriggit/issues) or [Mutagen](https://github.com/Mutagen-Modding/Mutagen/issues) Github issues.

Please include as much detail as you can in the report, including:

- The subrecord in question that it complained about (usually printed in the logs or console)
- Information about what tools went into making the mod.  Did you just use the official CK?  Or did you use other 3rd party tools?
- The source file (if you're willing to give it)

With that info, Spriggit's definitions should hopefully get updated quickly in a new published version, and you'll be on your way.  The mechanisms are meant to be upgraded frequently in this way.

Feel free to stop by the Discord to chat more directly!

[![Discord](https://discordapp.com/api/guilds/759302581448474626/widget.png)](https://discord.gg/53KMEsW)
