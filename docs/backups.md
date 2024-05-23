# Backups

Spriggit does a lot of transformations to get a Bethesda Plugin to a Git compatible text based format.  In an ideal world, you no longer need to keep your older Bethesda Plugins, as you can always go back in history and reconstitute one from the Git contents.  However, in doing so, you're putting a lot of trust in Spriggit that it can do that successfully, otherwise you're left with a bunch of text files and no actual plugin your game can run.  

While this hopefully never happens, it's always good to have a backup plan (especially as Spriggit is still in beta)

## Timed Backups

Spriggit currently backs up Bethesda Plugins every time it does a deserialize call.  These are stored in
`%temp%/Spriggit/Backups/[Mod Name]/[Date of Backup]/

It will do some optimization, like not re-backing up the file if the contents are the same as the previous.  

Backups are stored for 30 days by default, at which point older ones are cleared out.

## Future Work

More work might be put into the backup system, including easier ways to restore via CLI/UI, or mechanisms for storing backups in the Git repo itself.