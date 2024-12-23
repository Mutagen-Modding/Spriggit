﻿using Spriggit.TranslationPackages;

namespace Spriggit.Yaml.Fallout4;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await TranslationPackageRunner.Run(args, new EntryPoint());
    }
}