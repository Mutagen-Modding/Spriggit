# Sorting

## Overview
The Creation Kit automatically shuffles certain properties when saving plugin files. This behavior can cause instability in version control systems, as the same data may be stored in different orders across different save operations, leading to unnecessary differences in your Git history.

Spriggit automatically sorts these known shuffled categories to ensure they remain stable from translation to translation.

## Why Sorting Matters
When working with Git and tracking changes to your mod:

- **Cleaner Diffs**: Without sorting, the Creation Kit's shuffling would show every affected property as "changed" even when the actual data is identical
- **Meaningful History**: Your Git history will only show actual changes you made, not random reorderings from the Creation Kit
- **Better Merges**: Consistent ordering makes Git's merge algorithms more effective when combining changes from multiple contributors
- **Reduced Conflicts**: Stable ordering minimizes false merge conflicts that would otherwise occur from random shuffling

## What Gets Sorted
Spriggit tracks and sorts various record properties that the Creation Kit is known to shuffle. This includes many list-type fields and collections within records.

The sorting logic is continuously maintained and updated as new shuffling behaviors are discovered.

## Reporting New Shuffle Cases
If you notice fields in your Spriggit output that are still being shuffled between translations (appearing as changes in Git when you haven't actually modified them), please report them to the [Spriggit GitHub repository](https://github.com/Mutagen-Modding/Spriggit/issues) as issues.

When reporting:

1. Specify which game (Skyrim, Fallout 4, etc.)
2. Identify the record type (Quest, NPC, etc.)
3. Note which specific field or property is being shuffled

This helps improve Spriggit for everyone by ensuring all known shuffle cases are handled.