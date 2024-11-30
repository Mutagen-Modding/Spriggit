﻿using Spriggit.TranslationPackages;

namespace Spriggit.Json.Oblivion;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await TranslationPackageRunner.Run(args, new EntryPoint());
    }
}