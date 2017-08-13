using System;
using System.Text;
/*
 * http://tools.ietf.org/rfc/rfc1348.txt  
 * http://tools.ietf.org/html/rfc1706
 * 
 *	          |--------------|
              | <-- IDP -->  |
              |--------------|-------------------------------------|
              | AFI |  IDI   |            <-- DSP -->              |
              |-----|--------|-------------------------------------|
              | 47  |  0005  | DFI | AA |Rsvd | RD |Area | ID |Sel |
              |-----|--------|-----|----|-----|----|-----|----|----|
       octets |  1  |   2    |  1  | 3  |  2  | 2  |  2  | 6  | 1  |
              |-----|--------|-----|----|-----|----|-----|----|----|

                    IDP    Initial Domain Part
                    AFI    Authority and Format Identifier
                    IDI    Initial Domain Identifier
                    DSP    Domain Specific Part
                    DFI    DSP Format Identifier
                    AA     Administrative Authority
                    Rsvd   Reserved
                    RD     Routing Domain Identifier
                    Area   Area Identifier
                    ID     System Identifier
                    SEL    NSAP Selector

                  Figure 1: GOSIP Version 2 NSAP structure.


 */

namespace Heijden.DNS
{
	public class RecordNSAP : Record
	{
		public ushort LENGTH;
		public byte[] NSAPADDRESS;

		public RecordNSAP(RecordReader rr)
		{
			LENGTH = rr.ReadUInt16();
			NSAPADDRESS = rr.ReadBytes(LENGTH);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0} ", LENGTH);
			for (int intI = 0; intI < NSAPADDRESS.Length; intI++)
				sb.AppendFormat("{0:X00}", NSAPADDRESS[intI]);
			return sb.ToString();
		}

		public string ToGOSIPV2()
		{
			return string.Format("{0:X}.{1:X}.{2:X}.{3:X}.{4:X}.{5:X}.{6:X}{7:X}.{8:X}",
				NSAPADDRESS[0],							// AFI
				NSAPADDRESS[1]  << 8  | NSAPADDRESS[2],	// IDI
				NSAPADDRESS[3],							// DFI
				NSAPADDRESS[4]  << 16 | NSAPADDRESS[5] << 8 | NSAPADDRESS[6], // AA
				NSAPADDRESS[7]  << 8  | NSAPADDRESS[8],	// Rsvd
				NSAPADDRESS[9]  << 8  | NSAPADDRESS[10],// RD
				NSAPADDRESS[11] << 8  | NSAPADDRESS[12],// Area
				NSAPADDRESS[13] << 16 | NSAPADDRESS[14] << 8 | NSAPADDRESS[15], // ID-High
				NSAPADDRESS[16] << 16 | NSAPADDRESS[17] << 8 | NSAPADDRESS[18], // ID-Low
				NSAPADDRESS[19]);
		}

	}
}
