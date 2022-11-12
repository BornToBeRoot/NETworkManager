namespace NETworkManager.Models.Network
{
    public class BitCaluclatorInfo
    {

        public double Bits { get; set; }

        public double Bytes { get; set; }

        public double Kilobits { get; set; }

        public double Kilobytes { get; set; }

        public double Megabits { get; set; }

        public double Megabytes { get; set; }

        public double Gigabits { get; set; }

        public double Gigabytes { get; set; }

        public double Terabits { get; set; }

        public double Terabytes { get; set; }

        public double Petabits { get; set; }

        public double Petabytes { get; set; }

        public BitCaluclatorInfo()
        {

        }

        public BitCaluclatorInfo(double bits, double bytes, double kilobits, double kilobytes, double megabits, double megabytes, double gigabits, double gigabytes, double terabits, double terabytes, double petabits, double petabytes)
        {
            Bits = bits;
            Bytes = bytes;
            Kilobits = kilobits;
            Kilobytes = kilobytes;
            Megabits = megabits;
            Megabytes = megabytes;
            Gigabits = gigabits;
            Gigabytes = gigabytes;
            Terabits = terabits;
            Terabytes = terabytes;
            Petabits = petabits;
            Petabytes = petabytes;
        }
    }
}