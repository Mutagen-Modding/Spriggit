# Omissions

## Overview
The Creation Kit sometimes writes junk or unused data into certain fields when saving plugin files. These fields contain no meaningful information and can vary between saves even when you haven't made any changes. Spriggit automatically omits these known problematic fields to prevent unnecessary differences in your Git history.

## Why Omissions Matter
When working with Git and tracking changes to your mod:

- **Cleaner Diffs**: Omitting junk data ensures your diffs only show actual changes you made, not random bytes the Creation Kit wrote
- **Meaningful History**: Your Git history reflects intentional modifications, not Creation Kit artifacts
- **Smaller Files**: Excluding unused fields reduces the size of your serialized mod files
- **Reduced Conflicts**: Preventing junk data changes minimizes false merge conflicts that would otherwise occur from random byte differences

## What Gets Omitted
Spriggit automatically omits several categories of fields that are known to contain junk or unnecessary data:

### Unused Fields
Many record types contain fields explicitly marked as "Unused" in the game's data structures. These fields serve no purpose and the Creation Kit may write arbitrary data into them. Examples include:

- **Unused condition parameters**: Condition records have unused data fields that vary randomly
- **PlayerSkills.Unused**: Padding bytes in Skyrim NPC player skill data
- Other record-specific unused fields

### Unknown/Internal Data
Some fields contain internal Creation Kit metadata that isn't relevant for version control:

- **Unknown group header data**: Binary metadata in group headers that changes between saves
- **Timestamp data**: When records were last modified in the Creation Kit (not your actual mod changes)
- **Last modified data**: Internal tracking information

### Condition Data Fields
Conditions have several data fields that are only used for specific function types. When a condition uses a function that doesn't need certain parameters, those parameter fields may contain leftover junk data. Spriggit omits unused condition data fields to prevent this junk from appearing in diffs.

## How Omissions Work
Omitted fields are:

- **Not written** during serialization (Spriggit → YAML/JSON)
- **Set to default values** during deserialization (YAML/JSON → Spriggit)

This means when you serialize a mod, omitted fields won't appear in your YAML/JSON files. When you deserialize back to a plugin, these fields will be set to safe default values (typically zeros or empty arrays).

## Reporting Issues
If you encounter problems with omissions:

### Fields That Should Be Omitted
If you notice fields that contain junk data and cause unnecessary diffs between saves (when you haven't actually changed anything), please report them to the [Spriggit GitHub repository](https://github.com/Mutagen-Modding/Spriggit/issues).

When reporting:

1. Specify which game (Skyrim, Fallout 4, etc.)
2. Identify the record type (Quest, NPC, Condition, etc.)
3. Note which specific field contains junk data
4. Describe how you verified it's junk (e.g., "appears as changed in Git after re-saving without modifications")

### Fields Being Omitted That Shouldn't Be
If you find that Spriggit is omitting a field that actually contains important data you need to preserve, please report this as well:

1. Specify which game and record type
2. Identify which field is being omitted
3. Explain what data it should contain and why it's important
4. Provide an example of how the omission causes problems

This helps improve Spriggit for everyone by ensuring the right balance between omitting junk and preserving meaningful data.
