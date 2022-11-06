using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class BitCalculatorSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        public List<BitCaluclatorNotation> Notations { get; set; }

        private BitCaluclatorNotation _notation;
        public BitCaluclatorNotation Notation
        {
            get => _notation;
            set
            {
                if (value == _notation)
                    return;


                if (!_isLoading)
                    SettingsManager.Current.BitCalculator_Notation = value;


                _notation = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public BitCalculatorSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Notations = System.Enum.GetValues(typeof(BitCaluclatorNotation)).Cast<BitCaluclatorNotation>().OrderBy(x => x.ToString()).ToList();
            Notation = Notations.First(x => x == SettingsManager.Current.BitCalculator_Notation);
        }
        #endregion
    }
}