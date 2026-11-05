namespace NETworkManager.ViewModels;

/// <summary>
///     View model of a control whose text boxes validate their input (see
///     <see cref="Controls.InputErrorTracker" />): WPF doesn't pass invalid input to the view model, which keeps the
///     last valid value (e.g. the first character of a cleared username) - the control tells it instead.
/// </summary>
public interface IHasInputError
{
    /// <summary>
    ///     A text box of the control has invalid input (set by the control).
    /// </summary>
    bool HasInputError { get; set; }
}
