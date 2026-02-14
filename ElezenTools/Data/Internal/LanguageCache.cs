using System.Collections.Concurrent;
using System.Threading;
using Dalamud.Game;

namespace ElezenTools.Data.Internal;

internal sealed class LanguageCache<T>
{
    private readonly ConcurrentDictionary<ClientLanguage, Lazy<T>> cache = new();
    private readonly Func<ClientLanguage, T> factory;

    public LanguageCache(Func<ClientLanguage, T> factory)
    {
        this.factory = factory;
    }

    public T Get(ClientLanguage language)
    {
        var lazy = this.cache.GetOrAdd(language,
            key => new Lazy<T>(() => this.factory(key), LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value;
    }

    public void Clear()
    {
        this.cache.Clear();
    }
}
