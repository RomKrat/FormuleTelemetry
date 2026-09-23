namespace FormulaTelemetry.Web.Helpers;

/// <summary>Click-to-sort state for Blazor tables (toggle asc/desc per column).</summary>
public sealed class TableSortState
{
    public string Column { get; private set; }
    public bool Ascending { get; private set; }

    public TableSortState(string defaultColumn, bool ascending = true)
    {
        Column = defaultColumn;
        Ascending = ascending;
    }

    public void Reset(string defaultColumn, bool ascending = true)
    {
        Column = defaultColumn;
        Ascending = ascending;
    }

    public void Toggle(string column)
    {
        if (string.Equals(Column, column, StringComparison.Ordinal))
        {
            Ascending = !Ascending;
        }
        else
        {
            Column = column;
            Ascending = true;
        }
    }

    public string HeaderClass(string column)
    {
        if (!string.Equals(Column, column, StringComparison.Ordinal))
        {
            return "th-sort";
        }

        return Ascending ? "th-sort is-asc" : "th-sort is-desc";
    }

    public IOrderedEnumerable<T> Apply<T>(IEnumerable<T> source, Func<T, IComparable?> keySelector)
    {
        // Always OrderBy so nulls stay last; flip comparison when descending.
        return source.OrderBy(x => keySelector(x), Comparer<IComparable?>.Create((a, b) =>
        {
            if (a is null && b is null) return 0;
            if (a is null) return 1;
            if (b is null) return -1;
            var c = a.CompareTo(b);
            return Ascending ? c : -c;
        }));
    }
}
