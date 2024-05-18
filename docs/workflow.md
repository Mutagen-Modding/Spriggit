# Workflow

This area discusses the typical patterns for using Spriggit in your workflow.

<iframe width="560" height="315" src="https://www.youtube.com/embed/VgJaCaZSh98?si=5n6UIDiYBGZ-ba_U" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>

## An Individual Modder

- Create a Git Repository to hold your mod
- Create a Bethesda plugin with existing normal tools of choice
- Use Spriggit to convert the `.esp/m/l` files from your Bethesda workspace, to `.yaml` or `.json` files inside your Git Repository
- Make commits in Git.  
  "Added all the bandit Npc definitions"
  "Fixed the Powerblade damage to be more reasonable"
- Upload your mod, in its text format, up to Github (or your host of preference)

## Many Collaborators

Other modders, whether on your team or just helpful people out in the world can help collaborate and participate in your mod's development.

- They can clone the mod via Git to their computers
- Use Spriggit to convert from the `.yaml` or `.json` files to a Bethesda plugin
- Open the Bethesda plugin with the game, or other tools
- Modify the mod and help work on something
- Use Spriggit to convert back to text format
- Make commits in Git
- Upload their improvements to Github
- Initiate a Pull Request to ask that you consider their changes
- You can discuss with them about further changes, or merge their improvements into your mod
