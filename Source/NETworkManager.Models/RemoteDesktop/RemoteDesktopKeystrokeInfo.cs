namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopKeystrokeInfo
    {
        public bool[] ArrayKeyUp;
        public int[] KeyData;

        public RemoteDesktopKeystrokeInfo()
        {

        }

        public RemoteDesktopKeystrokeInfo(bool[] arrayKeyUp, int[] keyData)
        {
            ArrayKeyUp = arrayKeyUp;
            KeyData = keyData;
        }
    }
}
