# Bad Target Folder

"Cannot export next to a .git folder"

Spriggit's patterns require that you serialize into a folder that is wholly dedicated to containing Spriggit content.   As part of the serialization process, all files within that folder that were not just exported get deleted.   As such, if you're exporting spriggit content into a folder containing other files, they will be deleted as part of the serialization process.

## Solution

Simply make a new dedicated subfolder named after the mod and target that instead.

```
/Some Folder
   Other Content.txt
   MyMod.esp/
```

In this case, you would not want to target `/Some Folder`, as that has `Other Content.txt` within it.   Rather, targeting a subfolder with nothing else is recommended.