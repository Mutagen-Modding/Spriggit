﻿using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Spriggit.Core.Commands;

public static class SerializeCommandMetaExtractor
{
    public static SpriggitSource? ExtractSpriggitSource(DeserializeCommand deserializeCommand)
    {
        SpriggitSource? source = null;
        
        if (!deserializeCommand.PackageVersion.IsNullOrWhitespace() && deserializeCommand.PackageName.IsNullOrWhitespace())
        {
            throw new ArgumentException("Cannot specify PackageVersion without PackageName.");
        }

        if (!deserializeCommand.PackageName.IsNullOrWhitespace() ||
            !deserializeCommand.PackageVersion.IsNullOrWhitespace())
        {
            source = new SpriggitSource()
            {
                PackageName = deserializeCommand.PackageName,
                Version = deserializeCommand.PackageVersion,
            };
        }

        return source;
    }
    
    public static SpriggitMeta? ExtractSpriggitMeta(SerializeCommand serializeCommand)
    {
        SpriggitMeta? meta;
        if (serializeCommand.GameRelease == null || serializeCommand.PackageName.IsNullOrWhitespace())
        {
            if (!serializeCommand.PackageVersion.IsNullOrWhitespace())
            {
                throw new ArgumentException(
                    "Cannot specify version without also specifying GameRelease and PackageName");
            }
            meta = null;
        }
        else
        {
            if (serializeCommand.PackageVersion.IsNullOrWhitespace())
            {
                throw new ArgumentException("PackageVersion needs to be set if GameRelease or PackageName are.");
            }
            meta = new SpriggitMeta(
                new SpriggitSource()
                {
                    PackageName = serializeCommand.PackageName,
                    Version = serializeCommand.PackageVersion
                },
                Release: serializeCommand.GameRelease.Value);
        }

        return meta;
    }

    public static ModPath ExtractModPath(SerializeCommand serializeCommand)
    {
        ModPath modPath;
        if (serializeCommand.ModKey.IsNullOrWhitespace())
        {
            modPath = serializeCommand.InputPath;
        }
        else
        {
            modPath = new ModPath(
                ModKey.FromNameAndExtension(serializeCommand.ModKey),
                serializeCommand.InputPath);
        }

        return modPath;
    }
}