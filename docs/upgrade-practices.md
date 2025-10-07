# Upgrade Spriggit Version in Dedicated Commits

When upgrading your Spriggit translation package to a newer version, it's strongly recommended to do this in a dedicated commit that only contains the upgrade changes.

## Why This Matters

- **Isolates upgrade diffs**: Version upgrades can cause formatting changes, improved serialization, or other structural modifications to your text files that are unrelated to your actual mod changes
- **Prevents ambush diffs**: By upgrading and committing immediately, you avoid unexpected and unrelated file changes appearing in future commits when you're working on actual mod content
- **Cleaner git history**: Makes it easier to review what changes are due to the upgrade versus actual mod modifications
- **Easier troubleshooting**: If issues arise, you can easily identify whether they're related to the upgrade or your mod changes

## Recommended Workflows

There are two approaches to upgrading Spriggit versions:

### CLI Workflow

1. **Upgrade the version** using the [`upgrade` command](cli.md#upgrade-spriggit-version):
   ```
   .\Path\To\Spriggit.CLI.exe upgrade -p "C:\MyGitRepository\SomeMod.esp\" -v "1.2.3"
   ```

2. **Review the changes** to understand what the upgrade modified:
   ```
   git diff
   ```

3. **Commit immediately** with a clear message:
   ```
   git add -A
   git commit -m "Upgrade Spriggit to version 1.2.3"
   ```

4. **Continue with your mod work** in subsequent commits, knowing that any future diffs will be related to your actual changes

### Manual Workflow

1. **Update the spriggit-meta.json file** to specify the new version:
   ```json
   {
     "Source": {
       "PackageName": "Spriggit.Yaml.Skyrim",
       "Version": "1.2.3"
     }
   }
   ```

2. **Re-serialize with new version** back to the repository:
   ```
   .\Path\To\Spriggit.CLI.exe serialize -i "C:\Temp\SomeMod.esp" -o "C:\MyGitRepository\SomeMod.esp\"
   ```
   Or use the UI, if that is what you're using.

3. **Review and commit** the changes:
   ```
   git diff
   git add -A
   git commit -m "Upgrade Spriggit to version 1.2.3"
   ```

!!! warning "Don't Mix Changes"
    Avoid making mod changes in the same commit as a Spriggit version upgrade, as this makes it difficult to distinguish between upgrade-related changes and your actual modifications.