# Filename Too Long

"Filename too long" or similar path length errors when working with Spriggit output.

When Spriggit converts Bethesda plugins to text format, it creates detailed folder structures that can result in very long file paths. On Windows, the default path length limit is 260 characters, which can easily be exceeded when dealing with complex mod structures that include nested folders and descriptive filenames.

## Common Error Messages

- "The specified path, file name, or both are too long"
- "Filename too long"
- Git operations failing with path length errors
- Unable to clone or checkout repositories with Spriggit content

## Solution

Configure Git to handle long paths by running this command in your terminal:

```bash
git config --global core.longpaths true
```

This enables Git to work with paths longer than 260 characters on Windows.

### Alternative: Per-Repository Configuration

If you prefer to enable long paths only for specific repositories:

```bash
git config core.longpaths true
```

Run this command from within the specific Git repository where you're working with Spriggit content.

## Additional Windows Configuration

In some cases, you may also need to enable long path support at the Windows system level:

1. Open Group Policy Editor (`gpedit.msc`) as Administrator
2. Navigate to: `Computer Configuration > Administrative Templates > System > Filesystem`
3. Enable "Enable Win32 long paths"
4. Restart your computer

Alternatively, you can enable this via the Windows Registry by setting `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled` to `1`.

## Prevention

To minimize path length issues:

- Keep your Git repository closer to the root of your drive (e.g., `C:\Projects\MyMod` instead of `C:\Users\Username\Documents\Very\Deep\Folder\Structure\MyMod`)
- Use shorter mod names when possible
- Consider the total path length when organizing your project structure

## Why This Happens

Spriggit creates organized folder structures that mirror the hierarchical nature of Bethesda plugin data. Records are sorted into folders by type, and individual records get their own files with descriptive names. This organization makes the content more Git-friendly but can result in deeply nested paths that exceed Windows' default limitations.