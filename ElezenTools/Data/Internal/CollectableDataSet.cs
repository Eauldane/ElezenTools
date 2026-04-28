using System.Collections.Generic;

namespace ElezenTools.Data.Internal;

internal sealed class CollectableDataSet<TData, TRow>
{
    public CollectableDataSet(
        TData[] items,
        IReadOnlyDictionary<uint, TData> itemsById,
        IReadOnlyDictionary<uint, TRow> rowsById)
    {
        Items = items;
        ItemsById = itemsById;
        RowsById = rowsById;
    }

    public TData[] Items { get; }
    public IReadOnlyDictionary<uint, TData> ItemsById { get; }
    public IReadOnlyDictionary<uint, TRow> RowsById { get; }
}
