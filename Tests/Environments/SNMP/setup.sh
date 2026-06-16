#!/usr/bin/env bash
# Sets up a minimal, insecure SNMP test environment on Debian/Ubuntu.
# Idempotent: safe to re-run; resets configuration and SNMPv3 credentials each time.
set -euo pipefail

# ------------------------------------------------------------------------------
# Configuration — adjust to your needs
# ------------------------------------------------------------------------------
SYS_LOCATION="Testserver"
SYS_CONTACT="Test <test@borntoberoot.net>"

V2C_COMMUNITY="public"

V3_NOAUTH_USER="noAuthUser"

V3_AUTH_USER="authNoPrivUser"
V3_AUTH_PROTOCOL="SHA-512"      # MD5 | SHA | SHA-224 | SHA-256 | SHA-384 | SHA-512
V3_AUTH_PASSWORD="auth123456789"

V3_AUTHPRIV_USER="authPrivUser"
V3_PRIV_PROTOCOL="AES"          # DES | AES
V3_PRIV_PASSWORD="priv987654321"
# ------------------------------------------------------------------------------

SNMPD_CONF="/etc/snmp/snmpd.conf"
SNMPD_PERSIST="/var/lib/snmp/snmpd.conf"
SNMPD_CONF_D="/etc/snmp/snmpd.conf.d"

if [[ $EUID -ne 0 ]]; then
    echo "ERROR: Run as root or with sudo." >&2
    exit 1
fi

echo "==> Installing packages..."
apt-get update -y -qq
apt-get install -y -qq snmp snmpd

echo "==> Stopping snmpd..."
systemctl stop snmpd 2>/dev/null || true

# snmpd moves 'createUser' entries into /var/lib/snmp/snmpd.conf on first start
# and removes them from the main config. Remove the persisted user so snmpd
# re-creates it from our config on the next start (idempotency).
echo "==> Resetting SNMPv3 credentials from persistent store..."
if [[ -f "$SNMPD_PERSIST" ]]; then
    sed -i "/usmUser.*\"$V3_NOAUTH_USER\"/d" "$SNMPD_PERSIST"
    sed -i "/usmUser.*\"$V3_AUTH_USER\"/d"   "$SNMPD_PERSIST"
    sed -i "/usmUser.*\"$V3_AUTHPRIV_USER\"/d" "$SNMPD_PERSIST"
fi

echo "==> Writing $SNMPD_CONF..."
cat > "$SNMPD_CONF" << EOF
sysLocation    $SYS_LOCATION
sysContact     $SYS_CONTACT
sysServices    72

master  agentx

agentaddress  udp:161,udp6:161

view   systemonly  included   .1.3.6.1.2.1.1
view   systemonly  included   .1.3.6.1.2.1.25.1

# v2c
rocommunity  $V2C_COMMUNITY default -V systemonly
rocommunity6 $V2C_COMMUNITY default -V systemonly

# v3 – noAuthNoPriv (no authentication, no encryption)
createUser $V3_NOAUTH_USER
rouser $V3_NOAUTH_USER noauth -V systemonly

# v3 – authNoPriv (authentication, no encryption)
createUser $V3_AUTH_USER $V3_AUTH_PROTOCOL $V3_AUTH_PASSWORD
rouser $V3_AUTH_USER auth -V systemonly

# v3 – authPriv (authentication and encryption)
createUser $V3_AUTHPRIV_USER $V3_AUTH_PROTOCOL $V3_AUTH_PASSWORD $V3_PRIV_PROTOCOL $V3_PRIV_PASSWORD
rouser $V3_AUTHPRIV_USER authpriv -V systemonly

# include all *.conf files in a directory
includeDir $SNMPD_CONF_D
EOF
chmod 600 "$SNMPD_CONF"

echo "==> Creating $SNMPD_CONF_D (for optional extra config files)..."
mkdir -p "$SNMPD_CONF_D"

echo "==> Enabling and starting snmpd..."
systemctl enable --quiet snmpd
systemctl start snmpd

echo ""
if systemctl is-active --quiet snmpd; then
    echo "snmpd is running."
else
    echo "WARNING: snmpd did not start. Check 'journalctl -u snmpd' for details." >&2
    exit 1
fi

echo ""
echo "Setup complete. Credentials:"
echo ""
echo "  SNMPv2c"
echo "    Community string : $V2C_COMMUNITY"
echo ""
echo "  SNMPv3 – noAuthNoPriv"
echo "    Username         : $V3_NOAUTH_USER"
echo ""
echo "  SNMPv3 – authNoPriv"
echo "    Username         : $V3_AUTH_USER"
echo "    Auth protocol    : $V3_AUTH_PROTOCOL"
echo "    Auth password    : $V3_AUTH_PASSWORD"
echo ""
echo "  SNMPv3 – authPriv"
echo "    Username         : $V3_AUTHPRIV_USER"
echo "    Auth protocol    : $V3_AUTH_PROTOCOL"
echo "    Auth password    : $V3_AUTH_PASSWORD"
echo "    Priv protocol    : $V3_PRIV_PROTOCOL"
echo "    Priv password    : $V3_PRIV_PASSWORD"
echo ""
echo "Verify with:"
echo "  snmpwalk -v2c -c $V2C_COMMUNITY localhost .1.3.6.1.2.1.1"
echo "  snmpwalk -v3 -l noAuthNoPriv -u $V3_NOAUTH_USER localhost .1.3.6.1.2.1.1"
echo "  snmpwalk -v3 -l authNoPriv   -u $V3_AUTH_USER    -a $V3_AUTH_PROTOCOL -A $V3_AUTH_PASSWORD localhost .1.3.6.1.2.1.1"
echo "  snmpwalk -v3 -l authPriv     -u $V3_AUTHPRIV_USER -a $V3_AUTH_PROTOCOL -A $V3_AUTH_PASSWORD -x $V3_PRIV_PROTOCOL -X $V3_PRIV_PASSWORD localhost .1.3.6.1.2.1.1"
