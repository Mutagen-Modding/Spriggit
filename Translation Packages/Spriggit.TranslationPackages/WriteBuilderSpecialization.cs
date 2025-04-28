using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Records;

namespace Spriggit.TranslationPackages;

public static class WriteBuilderSpecialization
{
    public static BinaryModdedWriteBuilder<TModGetter> AddNonOpinionatedWriteOptions<TModGetter>(this BinaryModdedWriteBuilder<TModGetter> builder)
        where TModGetter : class, IModGetter
    {
        return builder
            .WithRecordCount(RecordCountOption.Iterate)
            .WithModKeySync(ModKeyOption.CorrectToPath)
            .WithMastersListContent(MastersListContentOption.NoCheck)
            .WithMastersListOrdering(MastersListOrderingOption.NoCheck)
            .NoFormIDUniquenessCheck()
            .NoFormIDCompactnessCheck()
            .NoCheckIfLowerRangeDisallowed()
            .NoNullFormIDStandardization();
    }
}