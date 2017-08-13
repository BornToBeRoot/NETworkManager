using System;
/*
 * http://tools.ietf.org/rfc/rfc2930.txt
 * 
2. The TKEY Resource Record

   The TKEY resource record (RR) has the structure given below.  Its RR
   type code is 249.

      Field       Type         Comment
      -----       ----         -------
       Algorithm:   domain
       Inception:   u_int32_t
       Expiration:  u_int32_t
       Mode:        u_int16_t
       Error:       u_int16_t
       Key Size:    u_int16_t
       Key Data:    octet-stream
       Other Size:  u_int16_t
       Other Data:  octet-stream  undefined by this specification

 */

namespace Heijden.DNS
{
	public class RecordTKEY : Record
	{
		public string ALGORITHM;
		public UInt32 INCEPTION;
		public UInt32 EXPIRATION;
		public UInt16 MODE;
		public UInt16 ERROR;
		public UInt16 KEYSIZE;
		public byte[] KEYDATA;
		public UInt16 OTHERSIZE;
		public byte[] OTHERDATA;

		public RecordTKEY(RecordReader rr)
		{
			ALGORITHM = rr.ReadDomainName();
			INCEPTION = rr.ReadUInt32();
			EXPIRATION = rr.ReadUInt32();
			MODE = rr.ReadUInt16();
			ERROR = rr.ReadUInt16();
			KEYSIZE = rr.ReadUInt16();
			KEYDATA = rr.ReadBytes(KEYSIZE);
			OTHERSIZE = rr.ReadUInt16();
			OTHERDATA = rr.ReadBytes(OTHERSIZE);
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4}",
				ALGORITHM,
				INCEPTION,
				EXPIRATION,
				MODE,
				ERROR);
		}

	}
}
