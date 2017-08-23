using System.Windows.Input;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System;
using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels.Notes
{
    public class RestartAsAdministratorViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        
        #endregion

        #region Constructor
        public RestartAsAdministratorViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
        }
        #endregion

        #region ICommands
        public ICommand RestartAsAdminCommand
        {
            get { return new RelayCommand(p => RestartApplicationAsAdminAction()); }
        }
        #endregion

        #region Methods
        public async void RestartApplicationAsAdminAction()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
                {
                    Verb = "runas"
                };

                Process.Start(info);

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode != 1223) // User has canceled
                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }
        #endregion
    }
}
