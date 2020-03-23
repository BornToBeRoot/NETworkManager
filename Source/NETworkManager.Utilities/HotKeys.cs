using System.Windows.Input;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class to provide static methods for hotkeys.
    /// </summary>
    public static class HotKeys
    {
        /// <summary>
        /// Method to calculate the sum of the modifier keys.
        /// </summary>
        /// <param name="modifierKeys"><see cref="ModifierKeys"/>.</param>
        /// <returns>Sum of the calculated modifier keys.</returns>
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

        /// <summary>
        /// Method to convert WPF key to WinForms key.
        /// </summary>
        /// <param name="key">WPF key.</param>
        /// <returns>WinForms key.</returns>
        public static System.Windows.Forms.Keys WpfKeyToFormsKeys(Key key)
        {
            return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
        }

        /// <summary>
        /// Method to convert WinForms key to WPF key.
        /// </summary>
        /// <param name="keys">WinForms key.</param>
        /// <returns>WPF key.</returns>
        public static Key FormsKeysToWpfKey(System.Windows.Forms.Keys keys)
        {
            return FormsKeysToWpfKey((int)keys);
        }

        /// <summary>
        /// Method to convert WinForms key to WPF key.
        /// </summary>
        /// <param name="keys">WinForms key as  <see cref="int"/>.</param>
        /// <returns>WPF key.</returns>

        public static Key FormsKeysToWpfKey(int keys)
        {
            return KeyInterop.KeyFromVirtualKey(keys);
        }

        /// <summary>
        /// Method to get modifier keys from an <see cref="int"/> value.
        /// </summary>
        /// <param name="modifierKeys">Modifier key as <see cref="int"/>.</param>
        /// <returns>Return <see cref="ModifierKeys"/>.</returns>
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
