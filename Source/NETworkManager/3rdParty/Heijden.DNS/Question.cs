using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Heijden.DNS
{
	#region Rfc 1034/1035
	/*
	4.1.2. Question section format

	The question section is used to carry the "question" in most queries,
	i.e., the parameters that define what is being asked.  The section
	contains QDCOUNT (usually 1) entries, each of the following format:

										1  1  1  1  1  1
		  0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
		+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
		|                                               |
		/                     QNAME                     /
		/                                               /
		+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
		|                     QTYPE                     |
		+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
		|                     QCLASS                    |
		+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

	where:

	QNAME           a domain name represented as a sequence of labels, where
					each label consists of a length octet followed by that
					number of octets.  The domain name terminates with the
					zero length octet for the null label of the root.  Note
					that this field may be an odd number of octets; no
					padding is used.

	QTYPE           a two octet code which specifies the type of the query.
					The values for this field include all codes valid for a
					TYPE field, together with some more general codes which
					can match more than one type of RR.


	QCLASS          a two octet code that specifies the class of the query.
					For example, the QCLASS field is IN for the Internet.
	*/
	#endregion

	public class Question
	{
		private string m_QName;
		public string QName
		{
			get
			{
				return m_QName;
			}
			set
			{
				m_QName = value;
				if (!m_QName.EndsWith("."))
					m_QName += ".";
			}
		}
		public QType QType;
		public QClass QClass;

		public Question(string QName,QType QType,QClass QClass)
		{
			this.QName = QName;
			this.QType = QType;
			this.QClass = QClass;
		}

		public Question(RecordReader rr)
		{
			QName = rr.ReadDomainName();
			QType = (QType)rr.ReadUInt16();
			QClass = (QClass)rr.ReadUInt16();
		}

		private byte[] WriteName(string src)
		{
			if (!src.EndsWith("."))
				src += ".";

			if (src == ".")
				return new byte[1];

			StringBuilder sb = new StringBuilder();
			int intI, intJ, intLen = src.Length;
			sb.Append('\0');
			for (intI = 0, intJ = 0; intI < intLen; intI++, intJ++)
			{
				sb.Append(src[intI]);
				if (src[intI] == '.')
				{
					sb[intI - intJ] = (char)(intJ & 0xff);
					intJ = -1;
				}
			}
			sb[sb.Length - 1] = '\0';
			return System.Text.Encoding.ASCII.GetBytes(sb.ToString());
		}

		public byte[] Data
		{
			get
			{
				List<byte> data = new List<byte>();
				data.AddRange(WriteName(QName));
				data.AddRange(WriteShort((ushort)QType));
				data.AddRange(WriteShort((ushort)QClass));
				return data.ToArray();
			}
		}

		private byte[] WriteShort(ushort sValue)
		{
			return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)sValue));
		}


		public override string ToString()
		{
			return string.Format("{0,-32}\t{1}\t{2}", QName, QClass, QType);
		}
	}
}
