using System;
/* http://tools.ietf.org/rfc/rfc1183.txt

3.1. The X25 RR

   The X25 RR is defined with mnemonic X25 and type code 19 (decimal).

   X25 has the following format:

   <owner> <ttl> <class> X25 <PSDN-address>

   <PSDN-address> is required in all X25 RRs.

   <PSDN-address> identifies the PSDN (Public Switched Data Network)
   address in the X.121 [10] numbering plan associated with <owner>.
   Its format in master files is a <character-string> syntactically
   identical to that used in TXT and HINFO.

   The format of X25 is class insensitive.  X25 RRs cause no additional
   section processing.

   The <PSDN-address> is a string of decimal digits, beginning with the
   4 digit DNIC (Data Network Identification Code), as specified in
   X.121. National prefixes (such as a 0) MUST NOT be used.

   For example:

   Relay.Prime.COM.  X25       311061700956


 */

namespace Heijden.DNS
{
	public class RecordX25 : Record
	{
		public string PSDNADDRESS;

		public RecordX25(RecordReader rr)
		{
			PSDNADDRESS = rr.ReadString();
		}

		public override string ToString()
		{
			return string.Format("{0}",
				PSDNADDRESS);
		}

	}
}
