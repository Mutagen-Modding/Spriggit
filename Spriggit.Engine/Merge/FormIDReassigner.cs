using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Spriggit.Engine.Merge;

public class FormIDReassigner
{
    public void Reassign<TMod, TModGetter>(
        TMod mod,
        Func<FormKey> getNextFormKey,
        IReadOnlyCollection<IFormLinkIdentifier> keysToReassign)
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {
        var cache = mod.ToImmutableLinkCache<TMod, TModGetter>();
        foreach (var key in keysToReassign)
        {
            if (!cache.TryResolveContext(key, out var context)) continue;
            context.DuplicateIntoAsNewRecord(mod, getNextFormKey());
            mod.Remove(key.FormKey, key.Type);
        }
    }
}