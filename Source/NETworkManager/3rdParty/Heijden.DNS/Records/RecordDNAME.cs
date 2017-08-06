using System;
/*
 * http://tools.ietf.org/rfc/rfc2672.txt
 * 
3. The DNAME Resource Record

   The DNAME RR has mnemonic DNAME and type code 39 (decimal).
   DNAME has the following format:

      <owner> <ttl> <class> DNAME <target>

   The format is not class-sensitive.  All fields are required.  The
   RDATA field <target> is a <domain-name> [DNSIS].

 * 
 */
namespace Heijden.DNS
{
	public class RecordDNAME : Record
	{
		public string TARGET;

		public RecordDNAME(RecordReader rr)
		{
			TARGET = rr.ReadDomainName();
		}

		public override string ToString()
		{
			return TARGET;
		}

	}
}
