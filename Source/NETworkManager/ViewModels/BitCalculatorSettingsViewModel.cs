using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the bit calculator settings.
/// </summary>
public class BitCalculatorSettingsViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Gets the list of available notations.
    /// </summary>
    public List<BitCaluclatorNotation> Notations { get; private set; }

    /// <summary>
    /// Gets or sets the selected notation.
    /// </summary>
    public BitCaluclatorNotation Notation
    {
        get;
        set
        {
            if (value == field)
                return;


            if (!_isLoading)
                SettingsManager.Current.BitCalculator_Notation = value;


            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="BitCalculatorSettingsViewModel"/> class.
    /// </summary>
    public BitCalculatorSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        Notations = Enum.GetValues(typeof(BitCaluclatorNotation)).Cast<BitCaluclatorNotation>()
            .OrderBy(x => x.ToString()).ToList();
        Notation = Notations.First(x => x == SettingsManager.Current.BitCalculator_Notation);
    }

    #endregion
}