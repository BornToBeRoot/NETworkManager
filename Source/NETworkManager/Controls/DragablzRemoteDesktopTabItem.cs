using System;

namespace NETworkManager.Controls
{
    public class DragablzRemoteDesktopTabItem : IDisposable
    {
        public string Header { get; set; }
        public RemoteDesktopControl Control { get; set; }

        public DragablzRemoteDesktopTabItem(string header, RemoteDesktopControl control)
        {
            Header = header;
            Control = control;
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                Control.OnClose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
