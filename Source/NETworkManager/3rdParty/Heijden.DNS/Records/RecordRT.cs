using System;
/* http://tools.ietf.org/rfc/rfc1183.txt

3.3. The Route Through RR

   The Route Through RR is defined with mnemonic RT and type code 21
   (decimal).

   The RT resource record provides a route-through binding for hosts
   that do not have their own direct wide area network addresses.  It is
   used in much the same way as the MX RR.

   RT has the following format:

   <owner> <ttl> <class> RT <preference> <intermediate-host>

   Both RDATA fields are required in all RT RRs.

   The first field, <preference>, is a 16 bit integer, representing the
   preference of the route.  Smaller numbers indicate more preferred
   routes.

   <intermediate-host> is the domain name of a host which will serve as
   an intermediate in reaching the host specified by <owner>.  The DNS
   RRs associated with <intermediate-host> are expected to include at
   least one A, X25, or ISDN record.

   The format of the RT RR is class insensitive.  RT records cause type
   X25, ISDN, and A additional section processing for <intermediate-
   host>.

   For example,

   sh.prime.com.      IN   RT   2    Relay.Prime.COM.
                      IN   RT   10   NET.Prime.COM.
   *.prime.com.       IN   RT   90   Relay.Prime.COM.

   When a host is looking up DNS records to attempt to route a datagram,
   it first looks for RT records for the destination host, which point
   to hosts with address records (A, X25, ISDN) compatible with the wide
   area networks available to the host.  If it is itself in the set of
   RT records, it discards any RTs with preferences higher or equal to
   its own.  If there are no (remaining) RTs, it can then use address
   records of the destination itself.

   Wild-card RTs are used exactly as are wild-card MXs.  RT's do not
   "chain"; that is, it is not valid to use the RT RRs found for a host
   referred to by an RT.

   The concrete encoding is identical to the MX RR.


 */

namespace Heijden.DNS
{
	public class RecordRT : Record
	{
		public ushort PREFERENCE;
		public string INTERMEDIATEHOST;

		public RecordRT(RecordReader rr)
		{
			PREFERENCE = rr.ReadUInt16();
			INTERMEDIATEHOST = rr.ReadDomainName();
		}

		public override string ToString()
		{
			return string.Format("{0} {1}",
				PREFERENCE,
				INTERMEDIATEHOST);
		}

	}
}
