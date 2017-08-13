using System;

#region Rfc info
/*
 * http://www.ietf.org/rfc/rfc2535.txt
 * 4.1 SIG RDATA Format

   The RDATA portion of a SIG RR is as shown below.  The integrity of
   the RDATA information is protected by the signature field.

                           1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
       0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |        type covered           |  algorithm    |     labels    |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                         original TTL                          |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                      signature expiration                     |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                      signature inception                      |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |            key  tag           |                               |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+         signer's name         +
      |                                                               /
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-/
      /                                                               /
      /                            signature                          /
      /                                                               /
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+


*/
#endregion

namespace Heijden.DNS
{
	public class RecordSIG : Record
	{
		public UInt16 TYPECOVERED;
		public byte ALGORITHM;
		public byte LABELS;
		public UInt32 ORIGINALTTL;
		public UInt32 SIGNATUREEXPIRATION;
		public UInt32 SIGNATUREINCEPTION;
		public UInt16 KEYTAG;
		public string SIGNERSNAME;
		public string SIGNATURE;

		public RecordSIG(RecordReader rr)
		{
			TYPECOVERED = rr.ReadUInt16();
			ALGORITHM = rr.ReadByte();
			LABELS = rr.ReadByte();
			ORIGINALTTL = rr.ReadUInt32();
			SIGNATUREEXPIRATION = rr.ReadUInt32();
			SIGNATUREINCEPTION = rr.ReadUInt32();
			KEYTAG = rr.ReadUInt16();
			SIGNERSNAME = rr.ReadDomainName();
			SIGNATURE = rr.ReadString();
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} \"{8}\"",
				TYPECOVERED,
				ALGORITHM,
				LABELS,
				ORIGINALTTL,
				SIGNATUREEXPIRATION,
				SIGNATUREINCEPTION,
				KEYTAG,
				SIGNERSNAME,
				SIGNATURE);
		}

	}
}
