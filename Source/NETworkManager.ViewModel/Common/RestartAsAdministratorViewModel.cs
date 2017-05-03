using System.Windows.Input;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;

namespace NETworkManager.ViewModel.Common
{
    public class RestartAsAdministratorViewModel : ViewModelBase
    {
        #region ICommands
        public ICommand RestartAsAdminCommand
        {
            get { return new RelayCommand(p => RestartApplicationAsAdminAction()); }
        }
        #endregion

        #region Methods
        public static void RestartApplicationAsAdminAction()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
                info.Verb = "runas";

                Process.Start(info);

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode != 1223) // User has canceled
                    throw;
            }
        }
        #endregion
    }
}
