using System;
/*

 */

namespace Heijden.DNS
{
	public class RecordAPL : Record
	{
		public byte[] RDATA;

		public RecordAPL(RecordReader rr)
		{
			// re-read length
			ushort RDLENGTH = rr.ReadUInt16(-2);
			RDATA = rr.ReadBytes(RDLENGTH);
		}

		public override string ToString()
		{
			return string.Format("not-used");
		}

	}
}
