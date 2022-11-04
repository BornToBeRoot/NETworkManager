using NETworkManager.Settings;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Threading.Tasks;
using System.Linq;
using MahApps.Metro.Controls;
using NETworkManager.Profiles;
using System.Windows.Threading;
using System.Collections.Generic;
using NETworkManager.Models;

namespace NETworkManager.ViewModels
{
    public class BitCalculatorViewModel : ViewModelBase
    {
        #region  Variables 
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private readonly bool _isLoading = true;
        private bool _isViewActive = true;

      
        #endregion

        #region Constructor, load settings
        public BitCalculatorViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;
                        
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            

        }
        #endregion

        #region ICommands & Actions
     
        #endregion

        #region Methods
        

        public void OnViewVisible()
        {
            _isViewActive = true;                    
        }

        public void OnViewHide()
        {
            _isViewActive = false;
        }

        
        #endregion

        #region Event
      
        #endregion
    }
}
