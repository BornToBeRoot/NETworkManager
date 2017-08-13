/* Modified Version by BornToBeRoot to support IPv6 dns server */

using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;


/*
 * Network Working Group                                     P. Mockapetris
 * Request for Comments: 1035                                           ISI
 *                                                            November 1987
 *
 *           DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
 *
 */
namespace Heijden.DNS
{
    /// <summary>
    /// Resolver is the main class to do DNS query lookups
    /// </summary>
    public class Resolver
    {
        /// <summary>
        /// Version of this set of routines, when not in a library
        /// </summary>
        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Default DNS port
        /// </summary>
        public const int DefaultPort = 53;

        /// <summary>
        /// Gets list of OPENDNS servers
        /// </summary>
        public static readonly IPEndPoint[] DefaultDnsServers =
            {
                new IPEndPoint(IPAddress.Parse("208.67.222.222"), DefaultPort),
                new IPEndPoint(IPAddress.Parse("208.67.220.220"), DefaultPort)
            };

        private ushort m_Unique;
        private bool m_UseCache;
        private bool m_Recursion;
        private int m_Retries;
        private int m_Timeout;
        private TransportType m_TransportType;

        private List<IPEndPoint> m_DnsServers;

        private Dictionary<string, Response> m_ResponseCache;

        /// <summary>
        /// Constructor of Resolver using DNS servers specified.
        /// </summary>
        /// <param name="DnsServers">Set of DNS servers</param>
        public Resolver(IPEndPoint[] DnsServers)
        {
            m_ResponseCache = new Dictionary<string, Response>();
            m_DnsServers = new List<IPEndPoint>();
            m_DnsServers.AddRange(DnsServers);

            m_Unique = (ushort)(new Random()).Next();
            m_Retries = 3;
            m_Timeout = 1;
            m_Recursion = true;
            m_UseCache = true;
            m_TransportType = TransportType.Udp;
        }

        /// <summary>
        /// Constructor of Resolver using DNS server specified.
        /// </summary>
        /// <param name="DnsServer">DNS server to use</param>
        public Resolver(IPEndPoint DnsServer)
            : this(new IPEndPoint[] { DnsServer })
        {
        }

        /// <summary>
        /// Constructor of Resolver using DNS server and port specified.
        /// </summary>
        /// <param name="ServerIpAddress">DNS server to use</param>
        /// <param name="ServerPortNumber">DNS port to use</param>
        public Resolver(IPAddress ServerIpAddress, int ServerPortNumber)
            : this(new IPEndPoint(ServerIpAddress, ServerPortNumber))
        {
        }

        /// <summary>
        /// Constructor of Resolver using DNS address and port specified.
        /// </summary>
        /// <param name="ServerIpAddress">DNS server address to use</param>
        /// <param name="ServerPortNumber">DNS port to use</param>
        public Resolver(string ServerIpAddress, int ServerPortNumber)
            : this(IPAddress.Parse(ServerIpAddress), ServerPortNumber)
        {
        }

        /// <summary>
        /// Constructor of Resolver using DNS address.
        /// </summary>
        /// <param name="ServerIpAddress">DNS server address to use</param>
        public Resolver(string ServerIpAddress)
            : this(IPAddress.Parse(ServerIpAddress), DefaultPort)
        {
        }

        /// <summary>
        /// Resolver constructor, using DNS servers specified by Windows
        /// </summary>
        public Resolver()
            : this(GetDnsServers())
        {
        }

        public class VerboseOutputEventArgs : EventArgs
        {
            public string Message;
            public VerboseOutputEventArgs(string Message)
            {
                this.Message = Message;
            }
        }

        private void Verbose(string format, params object[] args)
        {
            if (OnVerbose != null)
                OnVerbose(this, new VerboseEventArgs(string.Format(format, args)));
        }

        /// <summary>
        /// Verbose messages from internal operations
        /// </summary>
        public event VerboseEventHandler OnVerbose;
        public delegate void VerboseEventHandler(object sender, VerboseEventArgs e);

        public class VerboseEventArgs : EventArgs
        {
            public string Message;
            public VerboseEventArgs(string Message)
            {
                this.Message = Message;
            }
        }


        /// <summary>
        /// Gets or sets timeout in milliseconds
        /// </summary>
        public int TimeOut
        {
            get
            {
                return m_Timeout;
            }
            set
            {
                m_Timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets number of retries before giving up
        /// </summary>
        public int Retries
        {
            get
            {
                return m_Retries;
            }
            set
            {
                if (value >= 1)
                    m_Retries = value;
            }
        }

        /// <summary>
        /// Gets or set recursion for doing queries
        /// </summary>
        public bool Recursion
        {
            get
            {
                return m_Recursion;
            }
            set
            {
                m_Recursion = value;
            }
        }

        /// <summary>
        /// Gets or sets protocol to use
        /// </summary>
        public TransportType TransportType
        {
            get
            {
                return m_TransportType;
            }
            set
            {
                m_TransportType = value;
            }
        }

        /// <summary>
        /// Gets or sets list of DNS servers to use
        /// </summary>
        public IPEndPoint[] DnsServers
        {
            get
            {
                return m_DnsServers.ToArray();
            }
            set
            {
                m_DnsServers.Clear();
                m_DnsServers.AddRange(value);
            }
        }

        /// <summary>
        /// Gets first DNS server address or sets single DNS server to use
        /// </summary>
        public string DnsServer
        {
            get
            {
                return m_DnsServers[0].Address.ToString();
            }
            set
            {
                IPAddress ip;
                if (IPAddress.TryParse(value, out ip))
                {
                    m_DnsServers.Clear();
                    m_DnsServers.Add(new IPEndPoint(ip, DefaultPort));
                    return;
                }
                Response response = Query(value, QType.A);
                if (response.RecordsA.Length > 0)
                {
                    m_DnsServers.Clear();
                    m_DnsServers.Add(new IPEndPoint(response.RecordsA[0].Address, DefaultPort));
                }
            }
        }


        public bool UseCache
        {
            get
            {
                return m_UseCache;
            }
            set
            {
                m_UseCache = value;
                if (!m_UseCache)
                    m_ResponseCache.Clear();
            }
        }

        /// <summary>
        /// Clear the resolver cache
        /// </summary>
        public void ClearCache()
        {
            m_ResponseCache.Clear();
        }

        private Response SearchInCache(Question question)
        {
            if (!m_UseCache)
                return null;

            string strKey = question.QClass + "-" + question.QType + "-" + question.QName;

            Response response = null;

            lock (m_ResponseCache)
            {
                if (!m_ResponseCache.ContainsKey(strKey))
                    return null;

                response = m_ResponseCache[strKey];
            }

            int TimeLived = (int)((DateTime.Now.Ticks - response.TimeStamp.Ticks) / TimeSpan.TicksPerSecond);
            foreach (RR rr in response.RecordsRR)
            {
                rr.TimeLived = TimeLived;
                // The TTL property calculates its actual time to live
                if (rr.TTL == 0)
                    return null; // out of date
            }
            return response;
        }

        private void AddToCache(Response response)
        {
            if (!m_UseCache)
                return;

            // No question, no caching
            if (response.Questions.Count == 0)
                return;

            // Only cached non-error responses
            if (response.header.RCODE != RCode.NoError)
                return;

            Question question = response.Questions[0];

            string strKey = question.QClass + "-" + question.QType + "-" + question.QName;

            lock (m_ResponseCache)
            {
                if (m_ResponseCache.ContainsKey(strKey))
                    m_ResponseCache.Remove(strKey);

                m_ResponseCache.Add(strKey, response);
            }
        }

        private Response UdpRequest(Request request)
        {
            // RFC1035 max. size of a UDP datagram is 512 bytes
            byte[] responseMessage = new byte[512];

            for (int intAttempts = 0; intAttempts < m_Retries; intAttempts++)
            {
                for (int intDnsServer = 0; intDnsServer < m_DnsServers.Count; intDnsServer++)
                {
                    Socket socket;

                    if (m_DnsServers[intDnsServer].AddressFamily == AddressFamily.InterNetwork)
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    else
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);

                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, m_Timeout);

                    try
                    {
                        socket.SendTo(request.Data, m_DnsServers[intDnsServer]);
                        int intReceived = socket.Receive(responseMessage);
                        byte[] data = new byte[intReceived];
                        Array.Copy(responseMessage, data, intReceived);
                        Response response = new Response(m_DnsServers[intDnsServer], data);
                        AddToCache(response);
                        return response;
                    }
                    catch (SocketException)
                    {
                        Verbose(string.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)));
                        continue; // next try
                    }
                    finally
                    {
                        m_Unique++;

                        // close the socket
                        socket.Close();
                    }
                }
            }

            Response responseTimeout = new Response();
            responseTimeout.Error = "Timeout Error";
            return responseTimeout;
        }

        private Response TcpRequest(Request request)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            byte[] responseMessage = new byte[512];

            for (int intAttempts = 0; intAttempts < m_Retries; intAttempts++)
            {
                for (int intDnsServer = 0; intDnsServer < m_DnsServers.Count; intDnsServer++)
                {
                    TcpClient tcpClient;

                    if (m_DnsServers[intDnsServer].AddressFamily == AddressFamily.InterNetwork)
                        tcpClient = new TcpClient();
                    else
                        tcpClient = new TcpClient(AddressFamily.InterNetworkV6);

                    tcpClient.ReceiveTimeout = m_Timeout;

                    try
                    {
                        IAsyncResult result = tcpClient.BeginConnect(m_DnsServers[intDnsServer].Address, m_DnsServers[intDnsServer].Port, null, null);

                        bool success = result.AsyncWaitHandle.WaitOne(m_Timeout, true);

                        if (!success || !tcpClient.Connected)
                        {
                            tcpClient.Close();
                            Verbose(string.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)));
                            continue;
                        }

                        BufferedStream bs = new BufferedStream(tcpClient.GetStream());

                        byte[] data = request.Data;
                        bs.WriteByte((byte)((data.Length >> 8) & 0xff));
                        bs.WriteByte((byte)(data.Length & 0xff));
                        bs.Write(data, 0, data.Length);
                        bs.Flush();

                        Response TransferResponse = new Response();
                        int intSoa = 0;
                        int intMessageSize = 0;

                        //Debug.WriteLine("Sending "+ (request.Length+2) + " bytes in "+ sw.ElapsedMilliseconds+" mS");

                        while (true)
                        {
                            int intLength = bs.ReadByte() << 8 | bs.ReadByte();
                            if (intLength <= 0)
                            {
                                tcpClient.Close();
                                Verbose(string.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)));
                                throw new SocketException(); // next try
                            }

                            intMessageSize += intLength;

                            data = new byte[intLength];
                            bs.Read(data, 0, intLength);
                            Response response = new Response(m_DnsServers[intDnsServer], data);

                            //Debug.WriteLine("Received "+ (intLength+2)+" bytes in "+sw.ElapsedMilliseconds +" mS");

                            if (response.header.RCODE != RCode.NoError)
                                return response;

                            if (response.Questions[0].QType != QType.AXFR)
                            {
                                AddToCache(response);
                                return response;
                            }

                            // Zone transfer!!

                            if (TransferResponse.Questions.Count == 0)
                                TransferResponse.Questions.AddRange(response.Questions);
                            TransferResponse.Answers.AddRange(response.Answers);
                            TransferResponse.Authorities.AddRange(response.Authorities);
                            TransferResponse.Additionals.AddRange(response.Additionals);

                            if (response.Answers[0].Type == Type.SOA)
                                intSoa++;

                            if (intSoa == 2)
                            {
                                TransferResponse.header.QDCOUNT = (ushort)TransferResponse.Questions.Count;
                                TransferResponse.header.ANCOUNT = (ushort)TransferResponse.Answers.Count;
                                TransferResponse.header.NSCOUNT = (ushort)TransferResponse.Authorities.Count;
                                TransferResponse.header.ARCOUNT = (ushort)TransferResponse.Additionals.Count;
                                TransferResponse.MessageSize = intMessageSize;
                                return TransferResponse;
                            }
                        }
                    } // try
                    catch (SocketException)
                    {
                        continue; // next try
                    }
                    finally
                    {
                        m_Unique++;

                        // close the socket
                        tcpClient.Close();
                    }
                }
            }
            Response responseTimeout = new Response();
            responseTimeout.Error = "Timeout Error";
            return responseTimeout;
        }

        /// <summary>
        /// Do Query on specified DNS servers
        /// </summary>
        /// <param name="name">Name to query</param>
        /// <param name="qtype">Question type</param>
        /// <param name="qclass">Class type</param>
        /// <returns>Response of the query</returns>
        public Response Query(string name, QType qtype, QClass qclass)
        {
            Question question = new Question(name, qtype, qclass);
            Response response = SearchInCache(question);
            if (response != null)
                return response;

            Request request = new Request();
            request.AddQuestion(question);
            return GetResponse(request);
        }

        /// <summary>
        /// Do an QClass=IN Query on specified DNS servers
        /// </summary>
        /// <param name="name">Name to query</param>
        /// <param name="qtype">Question type</param>
        /// <returns>Response of the query</returns>
        public Response Query(string name, QType qtype)
        {
            Question question = new Question(name, qtype, QClass.IN);
            Response response = SearchInCache(question);
            if (response != null)
                return response;

            Request request = new Request();
            request.AddQuestion(question);
            return GetResponse(request);
        }

        private Response GetResponse(Request request)
        {
            request.header.ID = m_Unique;
            request.header.RD = m_Recursion;

            if (m_TransportType == TransportType.Udp)
                return UdpRequest(request);

            if (m_TransportType == TransportType.Tcp)
                return TcpRequest(request);

            Response response = new Response();
            response.Error = "Unknown TransportType";
            return response;
        }

        /// <summary>
        /// Gets a list of default DNS servers used on the Windows machine.
        /// </summary>
        /// <returns></returns>
        public static IPEndPoint[] GetDnsServers()
        {
            List<IPEndPoint> list = new List<IPEndPoint>();

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in adapters)
            {
                if (n.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProps = n.GetIPProperties();
                    // thanks to Jon Webster on May 20, 2008
                    foreach (IPAddress ipAddr in ipProps.DnsAddresses)
                    {
                        IPEndPoint entry = new IPEndPoint(ipAddr, DefaultPort);
                        if (!list.Contains(entry))
                            list.Add(entry);
                    }

                }
            }
            return list.ToArray();
        }


        //

        private IPHostEntry MakeEntry(string HostName)
        {
            IPHostEntry entry = new IPHostEntry();

            entry.HostName = HostName;

            Response response = Query(HostName, QType.A, QClass.IN);

            // fill AddressList and aliases
            List<IPAddress> AddressList = new List<IPAddress>();
            List<string> Aliases = new List<string>();
            foreach (AnswerRR answerRR in response.Answers)
            {
                if (answerRR.Type == Type.A)
                {
                    // answerRR.RECORD.ToString() == (answerRR.RECORD as RecordA).Address
                    AddressList.Add(IPAddress.Parse((answerRR.RECORD.ToString())));
                    entry.HostName = answerRR.NAME;
                }
                else
                {
                    if (answerRR.Type == Type.CNAME)
                        Aliases.Add(answerRR.NAME);
                }
            }
            entry.AddressList = AddressList.ToArray();
            entry.Aliases = Aliases.ToArray();

            return entry;
        }

        /// <summary>
        /// Translates the IPV4 or IPV6 address into an arpa address
        /// </summary>
        /// <param name="ip">IP address to get the arpa address form</param>
        /// <returns>The 'mirrored' IPV4 or IPV6 arpa address</returns>
        public static string GetArpaFromIp(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("in-addr.arpa.");
                foreach (byte b in ip.GetAddressBytes())
                {
                    sb.Insert(0, string.Format("{0}.", b));
                }
                return sb.ToString();
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ip6.arpa.");
                foreach (byte b in ip.GetAddressBytes())
                {
                    sb.Insert(0, string.Format("{0:x}.", (b >> 4) & 0xf));
                    sb.Insert(0, string.Format("{0:x}.", (b >> 0) & 0xf));
                }
                return sb.ToString();
            }
            return "?";
        }

        public static string GetArpaFromEnum(string strEnum)
        {
            StringBuilder sb = new StringBuilder();
            string Number = System.Text.RegularExpressions.Regex.Replace(strEnum, "[^0-9]", "");
            sb.Append("e164.arpa.");
            foreach (char c in Number)
            {
                sb.Insert(0, string.Format("{0}.", c));
            }
            return sb.ToString();
        }

        #region Deprecated methods in the original System.Net.DNS class

        /// <summary>
        ///		Returns the Internet Protocol (IP) addresses for the specified host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>
        ///		An array of type System.Net.IPAddress that holds the IP addresses for the
        ///		host that is specified by the hostNameOrAddress parameter. 
        ///</returns>
        public IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            IPHostEntry entry = GetHostEntry(hostNameOrAddress);
            return entry.AddressList;
        }

        private delegate IPAddress[] GetHostAddressesDelegate(string hostNameOrAddress);

        /// <summary>
        ///		Asynchronously returns the Internet Protocol (IP) addresses for the specified
        ///     host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <param name="requestCallback">
        ///		An System.AsyncCallback delegate that references the method to invoke when
        ///     the operation is complete.
        /// </param>
        /// <param name="stateObject">
        ///		A user-defined object that contains information about the operation. This
        ///     object is passed to the requestCallback delegate when the operation is complete.
        ///</param>
        /// <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        public IAsyncResult BeginGetHostAddresses(string hostNameOrAddress, AsyncCallback requestCallback, object stateObject)
        {
            GetHostAddressesDelegate g = new GetHostAddressesDelegate(GetHostAddresses);
            return g.BeginInvoke(hostNameOrAddress, requestCallback, stateObject);
        }

        /// <summary>
        ///		Ends an asynchronous request for DNS information.
        /// </summary>
        /// <param name="AsyncResult">
        ///		An System.IAsyncResult instance returned by a call to the Heijden.Dns.Resolver.BeginGetHostAddresses(System.String,System.AsyncCallback,System.Object)
        ///		method.
        /// </param>
        /// <returns></returns>
        public IPAddress[] EndGetHostAddresses(IAsyncResult AsyncResult)
        {
            AsyncResult aResult = (AsyncResult)AsyncResult;
            GetHostAddressesDelegate g = (GetHostAddressesDelegate)aResult.AsyncDelegate;
            return g.EndInvoke(AsyncResult);
        }

        /// <summary>
        ///		Creates an System.Net.IPHostEntry instance from the specified System.Net.IPAddress.
        /// </summary>
        /// <param name="ip">An System.Net.IPAddress.</param>
        /// <returns>An System.Net.IPHostEntry.</returns>
        public IPHostEntry GetHostByAddress(IPAddress ip)
        {
            return GetHostEntry(ip);
        }

        /// <summary>
        ///		Creates an System.Net.IPHostEntry instance from an IP address.
        /// </summary>
        /// <param name="address">An IP address.</param>
        /// <returns>An System.Net.IPHostEntry instance.</returns>
        public IPHostEntry GetHostByAddress(string address)
        {
            return GetHostEntry(address);
        }

        /// <summary>
        ///		Gets the DNS information for the specified DNS host name.
        /// </summary>
        /// <param name="hostName">The DNS name of the host</param>
        /// <returns>An System.Net.IPHostEntry object that contains host information for the address specified in hostName.</returns>
        public IPHostEntry GetHostByName(string hostName)
        {
            return MakeEntry(hostName);
        }

        private delegate IPHostEntry GetHostByNameDelegate(string hostName);

        /// <summary>
        ///		Asynchronously resolves an IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="hostName">The DNS name of the host</param>
        /// <param name="requestCallback">An System.AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="stateObject">
        ///		A user-defined object that contains information about the operation. This
        ///		object is passed to the requestCallback delegate when the operation is complete.
        /// </param>
        /// <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        public IAsyncResult BeginGetHostByName(string hostName, AsyncCallback requestCallback, object stateObject)
        {
            GetHostByNameDelegate g = new GetHostByNameDelegate(GetHostByName);
            return g.BeginInvoke(hostName, requestCallback, stateObject);
        }

        /// <summary>
        ///		Ends an asynchronous request for DNS information.
        /// </summary>
        /// <param name="AsyncResult">
        ///		An System.IAsyncResult instance returned by a call to an 
        ///		Heijden.Dns.Resolver.BeginGetHostByName method.
        /// </param>
        /// <returns></returns>
        public IPHostEntry EndGetHostByName(IAsyncResult AsyncResult)
        {
            AsyncResult aResult = (AsyncResult)AsyncResult;
            GetHostByNameDelegate g = (GetHostByNameDelegate)aResult.AsyncDelegate;
            return g.EndInvoke(AsyncResult);
        }

        /// <summary>
        ///		Resolves a host name or IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="hostName">A DNS-style host name or IP address.</param>
        /// <returns></returns>
        //[Obsolete("no problem",false)]
        public IPHostEntry Resolve(string hostName)
        {
            return MakeEntry(hostName);
        }

        private delegate IPHostEntry ResolveDelegate(string hostName);

        /// <summary>
        ///		Begins an asynchronous request to resolve a DNS host name or IP address to
        ///     an System.Net.IPAddress instance.
        /// </summary>
        /// <param name="hostName">The DNS name of the host.</param>
        /// <param name="requestCallback">
        ///		An System.AsyncCallback delegate that references the method to invoke when
        ///     the operation is complete.
        ///	</param>
        /// <param name="stateObject">
        ///		A user-defined object that contains information about the operation. This
        ///     object is passed to the requestCallback delegate when the operation is complete.
        /// </param>
        /// <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        public IAsyncResult BeginResolve(string hostName, AsyncCallback requestCallback, object stateObject)
        {
            ResolveDelegate g = new ResolveDelegate(Resolve);
            return g.BeginInvoke(hostName, requestCallback, stateObject);
        }

        /// <summary>
        ///		Ends an asynchronous request for DNS information.
        /// </summary>
        /// <param name="AsyncResult">
        ///		An System.IAsyncResult instance that is returned by a call to the System.Net.Dns.BeginResolve(System.String,System.AsyncCallback,System.Object)
        ///     method.
        /// </param>
        /// <returns>An System.Net.IPHostEntry object that contains DNS information about a host.</returns>
        public IPHostEntry EndResolve(IAsyncResult AsyncResult)
        {
            AsyncResult aResult = (AsyncResult)AsyncResult;
            ResolveDelegate g = (ResolveDelegate)aResult.AsyncDelegate;
            return g.EndInvoke(AsyncResult);
        }
        #endregion

        /// <summary>
        ///		Resolves an IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="ip">An IP address.</param>
        /// <returns>
        ///		An System.Net.IPHostEntry instance that contains address information about
        ///		the host specified in address.
        ///</returns>
        public IPHostEntry GetHostEntry(IPAddress ip)
        {
            Response response = Query(GetArpaFromIp(ip), QType.PTR, QClass.IN);
            if (response.RecordsPTR.Length > 0)
                return MakeEntry(response.RecordsPTR[0].PTRDNAME);
            else
                return new IPHostEntry();
        }

        /// <summary>
        ///		Resolves a host name or IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>
        ///		An System.Net.IPHostEntry instance that contains address information about
        ///		the host specified in hostNameOrAddress. 
        ///</returns>
        public IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            IPAddress iPAddress;
            if (IPAddress.TryParse(hostNameOrAddress, out iPAddress))
                return GetHostEntry(iPAddress);
            else
                return MakeEntry(hostNameOrAddress);
        }

        private delegate IPHostEntry GetHostEntryViaIPDelegate(IPAddress ip);
        private delegate IPHostEntry GetHostEntryDelegate(string hostNameOrAddress);

        /// <summary>
        /// Asynchronously resolves a host name or IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <param name="requestCallback">
        ///		An System.AsyncCallback delegate that references the method to invoke when
        ///		the operation is complete.
        ///</param>
        /// <param name="stateObject">
        ///		A user-defined object that contains information about the operation. This
        ///		object is passed to the requestCallback delegate when the operation is complete.
        /// </param>
        /// <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        public IAsyncResult BeginGetHostEntry(string hostNameOrAddress, AsyncCallback requestCallback, object stateObject)
        {
            GetHostEntryDelegate g = new GetHostEntryDelegate(GetHostEntry);
            return g.BeginInvoke(hostNameOrAddress, requestCallback, stateObject);
        }

        /// <summary>
        /// Asynchronously resolves an IP address to an System.Net.IPHostEntry instance.
        /// </summary>
        /// <param name="ip">The IP address to resolve.</param>
        /// <param name="requestCallback">
        ///		An System.AsyncCallback delegate that references the method to invoke when
        ///		the operation is complete.
        /// </param>
        /// <param name="stateObject">
        ///		A user-defined object that contains information about the operation. This
        ///     object is passed to the requestCallback delegate when the operation is complete.
        /// </param>
        /// <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        public IAsyncResult BeginGetHostEntry(IPAddress ip, AsyncCallback requestCallback, object stateObject)
        {
            GetHostEntryViaIPDelegate g = new GetHostEntryViaIPDelegate(GetHostEntry);
            return g.BeginInvoke(ip, requestCallback, stateObject);
        }

        /// <summary>
        /// Ends an asynchronous request for DNS information.
        /// </summary>
        /// <param name="AsyncResult">
        ///		An System.IAsyncResult instance returned by a call to an 
        ///		Overload:Heijden.Dns.Resolver.BeginGetHostEntry method.
        /// </param>
        /// <returns>
        ///		An System.Net.IPHostEntry instance that contains address information about
        ///		the host. 
        ///</returns>
        public IPHostEntry EndGetHostEntry(IAsyncResult AsyncResult)
        {
            AsyncResult aResult = (AsyncResult)AsyncResult;
            if (aResult.AsyncDelegate is GetHostEntryDelegate)
            {
                GetHostEntryDelegate g = (GetHostEntryDelegate)aResult.AsyncDelegate;
                return g.EndInvoke(AsyncResult);
            }
            if (aResult.AsyncDelegate is GetHostEntryViaIPDelegate)
            {
                GetHostEntryViaIPDelegate g = (GetHostEntryViaIPDelegate)aResult.AsyncDelegate;
                return g.EndInvoke(AsyncResult);
            }
            return null;
        }

        private enum RRRecordStatus
        {
            UNKNOWN,
            NAME,
            TTL,
            CLASS,
            TYPE,
            VALUE
        }

        /// <summary>
        /// Not implemented yet!!!!!
        /// </summary>
        /// <param name="strPath"></param>
        public void LoadRootFile(string strPath)
        {
            StreamReader sr = new StreamReader(strPath);
            while (!sr.EndOfStream)
            {
                string strLine = sr.ReadLine();
                if (strLine == null)
                    break;
                int intI = strLine.IndexOf(';');
                if (intI >= 0)
                    strLine = strLine.Substring(0, intI);
                strLine = strLine.Trim();
                if (strLine.Length == 0)
                    continue;
                RRRecordStatus status = RRRecordStatus.NAME;
                string Name = "";
                string Ttl = "";
                string Class = "";
                string Type = "";
                string Value = "";
                string strW = "";
                for (intI = 0; intI < strLine.Length; intI++)
                {
                    char C = strLine[intI];

                    if (C <= ' ' && strW != "")
                    {
                        switch (status)
                        {
                            case RRRecordStatus.NAME:
                                Name = strW;
                                status = RRRecordStatus.TTL;
                                break;
                            case RRRecordStatus.TTL:
                                Ttl = strW;
                                status = RRRecordStatus.CLASS;
                                break;
                            case RRRecordStatus.CLASS:
                                Class = strW;
                                status = RRRecordStatus.TYPE;
                                break;
                            case RRRecordStatus.TYPE:
                                Type = strW;
                                status = RRRecordStatus.VALUE;
                                break;
                            case RRRecordStatus.VALUE:
                                Value = strW;
                                status = RRRecordStatus.UNKNOWN;
                                break;
                            default:
                                break;
                        }
                        strW = "";
                    }
                    if (C > ' ')
                        strW += C;
                }

            }
            sr.Close();
        }
    } // class
}