using System;

/*
3.3.13. SOA RDATA format

	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	/                     MNAME                     /
	/                                               /
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	/                     RNAME                     /
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	|                    SERIAL                     |
	|                                               |
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	|                    REFRESH                    |
	|                                               |
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	|                     RETRY                     |
	|                                               |
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	|                    EXPIRE                     |
	|                                               |
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
	|                    MINIMUM                    |
	|                                               |
	+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

MNAME           The <domain-name> of the name server that was the
				original or primary source of data for this zone.

RNAME           A <domain-name> which specifies the mailbox of the
				person responsible for this zone.

SERIAL          The unsigned 32 bit version number of the original copy
				of the zone.  Zone transfers preserve this value.  This
				value wraps and should be compared using sequence space
				arithmetic.

REFRESH         A 32 bit time interval before the zone should be
				refreshed.

RETRY           A 32 bit time interval that should elapse before a
				failed refresh should be retried.

EXPIRE          A 32 bit time value that specifies the upper limit on
				the time interval that can elapse before the zone is no
				longer authoritative.

MINIMUM         The unsigned 32 bit minimum TTL field that should be
				exported with any RR from this zone.

SOA records cause no additional section processing.

All times are in units of seconds.

Most of these fields are pertinent only for name server maintenance
operations.  However, MINIMUM is used in all query operations that
retrieve RRs from a zone.  Whenever a RR is sent in a response to a
query, the TTL field is set to the maximum of the TTL field from the RR
and the MINIMUM field in the appropriate SOA.  Thus MINIMUM is a lower
bound on the TTL field for all RRs in a zone.  Note that this use of
MINIMUM should occur when the RRs are copied into the response and not
when the zone is loaded from a master file or via a zone transfer.  The
reason for this provison is to allow future dynamic update facilities to
change the SOA RR with known semantics.
*/

namespace Heijden.DNS
{
	public class RecordSOA : Record
	{
		public string MNAME;
		public string RNAME;
		public uint SERIAL;
		public uint REFRESH;
		public uint RETRY;
		public uint EXPIRE;
		public uint MINIMUM;

		public RecordSOA(RecordReader rr)
		{
			MNAME = rr.ReadDomainName();
			RNAME = rr.ReadDomainName();
			SERIAL = rr.ReadUInt32();
			REFRESH = rr.ReadUInt32();
			RETRY = rr.ReadUInt32();
			EXPIRE = rr.ReadUInt32();
			MINIMUM = rr.ReadUInt32();
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6}",
				MNAME,
				RNAME,
				SERIAL,
				REFRESH,
				RETRY,
				EXPIRE,
				MINIMUM);
		}
	}
}
