using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Serilog;

namespace Spriggit.Engine.Merge;

public class FormIDReassigner
{
    private readonly ILogger _logger;

    public FormIDReassigner(ILogger logger)
    {
        _logger = logger;
    }
    
    public void Reassign<TMod, TModGetter>(
        TMod mod,
        Func<FormKey> getNextFormKey,
        IReadOnlyCollection<IFormLinkIdentifier> keysToReassign)
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {
        _logger.Information("Reassigning:");
        var cache = mod.ToImmutableLinkCache<TMod, TModGetter>();
        foreach (var key in keysToReassign)
        {
            if (!cache.TryResolveContext(key, out var context))
            {
                _logger.Error($"  Could not look up {key.FormKey}");
                continue;
            }

            var nextId = getNextFormKey();
            _logger.Information($"  {key.FormKey} ({context.Record.EditorID}) -> {nextId}");
            context.DuplicateIntoAsNewRecord(mod, nextId);
            mod.Remove(key.FormKey, key.Type);
        }
    }
}