using System.Windows.Input;

namespace NETworkManager.Utilities
{
    public static class HotKeys
    {
        public static int GetModifierKeysSum(ModifierKeys modifierKeys)
        {
            var sum = 0x0000;

            if (modifierKeys.HasFlag(ModifierKeys.Alt))
                sum += 0x0001;

            if (modifierKeys.HasFlag(ModifierKeys.Control))
                sum += 0x0002;

            if (modifierKeys.HasFlag(ModifierKeys.Shift))
                sum += 0x0004;

            if (modifierKeys.HasFlag(ModifierKeys.Windows))
                sum += 0x0008;

            return sum;
        }

        public static System.Windows.Forms.Keys WpfKeyToFormsKeys(Key key)
        {
            return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
        }

        public static Key FormsKeysToWpfKey(System.Windows.Forms.Keys keys)
        {
            return KeyInterop.KeyFromVirtualKey((int)keys);
        }

        public static Key FormsKeysToWpfKey(int keys)
        {
            return KeyInterop.KeyFromVirtualKey(keys);
        }

        public static ModifierKeys GetModifierKeysFromInt(int modifierKeys)
        {
            var modKeys = ModifierKeys.None;

            if (modifierKeys - 0x0008 >= 0)
            {
                modKeys |= ModifierKeys.Windows;
                modifierKeys -= 0x0008;
            }

            if (modifierKeys - 0x0004 >= 0)
            {
                modKeys |= ModifierKeys.Shift;
                modifierKeys -= 0x0004;
            }

            if (modifierKeys - 0x0002 >= 0)
            {
                modKeys |= ModifierKeys.Control;
                modifierKeys -= 0x0002;
            }

            if (modifierKeys - 0x0001 >= 0)
                modKeys |= ModifierKeys.Alt;

            return modKeys;
        }
    }
}
