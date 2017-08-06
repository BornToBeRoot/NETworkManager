using System;
/* 
 * http://tools.ietf.org/rfc/rfc2163.txt
 * 
4. The new DNS resource record for MIXER mapping rules: PX

   The specification of the Internet DNS (RFC1035) provides a number of
   specific resource records (RRs) to contain specific pieces of
   information. In particular they contain the Mail eXchanger (MX) RR
   and the host Address (A) records which are used by the Internet SMTP
   mailers. As we will store the RFC822 to X.400 mapping information in
   the already existing DNS name tree, we need to define a new DNS RR in
   order to avoid any possible clash or misuse of already existing data
   structures. The same new RR will also be used to store the mappings
   from X.400 to RFC822. More over the mapping information, i.e., the
   MCGAMs, has a specific format and syntax which require an appropriate
   data structure and processing. A further advantage of defining a new
   RR is the ability to include flexibility for some eventual future
   development.

   The definition of the new 'PX' DNS resource record is:

      class:        IN   (Internet)

      name:         PX   (pointer to X.400/RFC822 mapping information)

      value:        26

   The PX RDATA format is:

          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          |                  PREFERENCE                   |
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                    MAP822                     /
          /                                               /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                    MAPX400                    /
          /                                               /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

   where:

   PREFERENCE   A 16 bit integer which specifies the preference given to
                this RR among others at the same owner.  Lower values
                are preferred;

   MAP822       A <domain-name> element containing <rfc822-domain>, the
                RFC822 part of the MCGAM;

   MAPX400      A <domain-name> element containing the value of
                <x400-in-domain-syntax> derived from the X.400 part of
                the MCGAM (see sect. 4.2);

   PX records cause no additional section processing. The PX RR format
   is the usual one:

             <name> [<class>] [<TTL>] <type> <RDATA>

   When we store in DNS a 'table1' or a 'gate1' entry, then <name> will
   be an X.400 mail domain name in DNS syntax (see sect. 4.2). When we
   store a 'table2' or a 'gate2' table entry, <name> will be an RFC822
   mail domain name, including both fully qualified DNS domains and mail
   only domains (MX-only domains). All normal DNS conventions, like
   default values, wildcards, abbreviations and message compression,
   apply also for all the components of the PX RR. In particular <name>,
   MAP822 and MAPX400, as <domain-name> elements, must have the final
   "." (root) when they are fully qualified.



 */

namespace Heijden.DNS
{
	public class RecordPX : Record
	{
		public UInt16 PREFERENCE;
		public string MAP822;
		public string MAPX400;

		public RecordPX(RecordReader rr)
		{
			PREFERENCE = rr.ReadUInt16();
			MAP822 = rr.ReadDomainName();
			MAPX400 = rr.ReadDomainName();
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}",
				PREFERENCE,
				MAP822,
				MAPX400);
		}

	}
}
