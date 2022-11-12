using System;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{    public static class BitCaluclator
    {        public static Task<BitCaluclatorInfo> CalculateAsync(double input, BitCaluclatorUnit unit, BitCaluclatorNotation notation)
        {
            return Task.Run(() => Calculate(input, unit, notation));
        }

        public static BitCaluclatorInfo Calculate(double input, BitCaluclatorUnit unit, BitCaluclatorNotation notation)
        {
            // Get bits from input
            int u = GetUnitBase(unit);
            int n = GetNotationBase(notation);

            double bits;

            if (unit.ToString().EndsWith("Bits", StringComparison.OrdinalIgnoreCase))
                bits = input * Math.Pow(n, u);
            else
                bits = input * 8 * Math.Pow(n, u);

            // Return caculation
            return new BitCaluclatorInfo
            {
                Bits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Bits)),
                Bytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Bytes)),
                Kilobits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Kilobits)),
                Kilobytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Kilobytes)),
                Megabits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Megabits)),
                Megabytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Megabytes)),
                Gigabits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Gigabits)),
                Gigabytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Gigabytes)),
                Terabits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Terabits)),
                Terabytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Terabytes)),
                Petabits = bits / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Petabits)),
                Petabytes = bits / 8 / Math.Pow(n, GetUnitBase(BitCaluclatorUnit.Petabytes)),
            };
        }

        private static int GetNotationBase(BitCaluclatorNotation notation)
        {
            return notation == BitCaluclatorNotation.Binary ? 1024 : 1000;
        }

        private static int GetUnitBase(BitCaluclatorUnit unit)
        {
            return unit switch
            {
                BitCaluclatorUnit.Bits => 0,
                BitCaluclatorUnit.Bytes => 0,
                BitCaluclatorUnit.Kilobits => 1,
                BitCaluclatorUnit.Kilobytes => 1,
                BitCaluclatorUnit.Megabits => 2,
                BitCaluclatorUnit.Megabytes => 2,
                BitCaluclatorUnit.Gigabits => 3,
                BitCaluclatorUnit.Gigabytes => 3,
                BitCaluclatorUnit.Terabits => 4,
                BitCaluclatorUnit.Terabytes => 4,
                BitCaluclatorUnit.Petabits => 5,
                BitCaluclatorUnit.Petabytes => 5,
                _ => -1,
            };
        }
    }
}
