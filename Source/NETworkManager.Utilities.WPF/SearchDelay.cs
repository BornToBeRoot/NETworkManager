using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Delays a search while the user is typing, so a large list is not filtered on every key press:
///     <see cref="Restart" /> on every change of the search text, the search is applied once the user stopped typing
///     for the interval - or right away with <see cref="Apply" /> (e.g. Enter in the search box, see
///     <see cref="SearchTextBoxHelper" />).
/// </summary>
public sealed class SearchDelay
{
    private readonly DispatcherTimer _timer;
    private readonly Action _applySearch;

    /// <summary>
    ///     Creates a new search delay. Must be created on the UI thread.
    /// </summary>
    /// <param name="interval">Time without a change after which the search is applied.</param>
    /// <param name="applySearch">Applies the search (e.g. refreshes the filtered view).</param>
    public SearchDelay(TimeSpan interval, Action applySearch)
    {
        _applySearch = applySearch;

        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += (_, _) => Apply();

        ApplyCommand = new RelayCommand(_ => Apply());
    }

    /// <summary>
    ///     Gets the command to apply the search right away (see <see cref="Apply" />), e.g. bound to Enter in the search
    ///     box.
    /// </summary>
    public ICommand ApplyCommand { get; }

    /// <summary>
    ///     Starts the delay again (the search text was changed).
    /// </summary>
    public void Restart()
    {
        _timer.Stop();
        _timer.Start();
    }

    /// <summary>
    ///     Applies the search right away, a pending delay is stopped.
    /// </summary>
    public void Apply()
    {
        _timer.Stop();
        _applySearch();
    }
}
