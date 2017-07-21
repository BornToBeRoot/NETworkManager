using System.Windows.Input;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System;

namespace NETworkManager.ViewModels.Notes
{
    public class RestartAsAdministratorViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();
        #endregion

        #region Constructor
        public RestartAsAdministratorViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };
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
                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }
        }
        #endregion
    }
}
