using Lextm.SharpSnmpLib.Messaging;
using System.Security;
using System.Threading;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class representing the SNMP options for version 3.
    /// </summary>
    public class SNMPOptionsV3 : SNMPOptionsBase
    {
        /// <summary>
        /// SNMPv3 security (noAuthNoPriv, authNoPriv, authPriv).
        /// </summary>
        public SNMPV3Security Security { get; set; }

        /// <summary>
        /// Username for SNMPv3.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Authentication provider for SNMPv3.
        /// </summary>
        public SNMPV3AuthenticationProvider AuthProvider { get; set; }

        /// <summary>
        /// Authentication password for SNMPv3.
        /// </summary>
        public SecureString Auth { get; set; }

        /// <summary>
        /// Privacy provider for SNMPv3.
        /// </summary>
        public SNMPV3PrivacyProvider PrivProvider { get; set; }

        /// <summary>
        /// Privacy password for SNMPv3.
        /// </summary>
        public SecureString Priv { get; set; }

        /// <summary>
        /// Create an instance of <see cref="SNMPOptionsV3"/> with parameters.
        /// </summary>
        /// <param name="security">SNMPv3 security (noAuthNoPriv, authNoPriv, authPriv).</param>
        /// <param name="username">Username for SNMPv3.</param>
        /// <param name="authProvider">Authentication provider for SNMPv3.</param>
        /// <param name="auth">Authentication password for SNMPv3.</param>
        /// <param name="privProvider">Privacy provider for SNMPv3.</param>
        /// <param name="priv">Privacy password for SNMPv3.</param>
        /// <param name="port">Port to use for SNMP requests.</param>
        /// <param name="walkMode">Walk mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public SNMPOptionsV3(SNMPV3Security security, string username, SNMPV3AuthenticationProvider authProvider, SecureString auth, SNMPV3PrivacyProvider privProvider, SecureString priv, int port, WalkMode walkMode, CancellationToken cancellationToken) : base(SNMPVersion.V3, port, walkMode, cancellationToken)
        {            
            Security = security;
            Username = username;
            AuthProvider = authProvider;
            Auth = auth;
            PrivProvider = privProvider;
            Priv = priv;            
        }
    }
}
