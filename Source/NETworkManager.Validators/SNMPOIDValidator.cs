using Lextm.SharpSnmpLib;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators;

public class SNMPOIDValidator : ValidationRule
{
    public SNMPOIDDependencyObjectWrapper Wrapper { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // Use SharpSNMP new ObjectIdentifiert to validate oid
        try
        {
            var oidValue = (value as string).Replace(" ", "");

            if (Wrapper.Mode == Models.Network.SNMPMode.Get && oidValue.Contains(';'))
            {
                foreach (var oid in oidValue.Split(';'))
                    _ = new ObjectIdentifier(oid);

                return ValidationResult.ValidResult;
            }
            
            _ = new ObjectIdentifier(oidValue);                     
        }
        catch (System.ArgumentException)
        {
            return new ValidationResult(false, Localization.Resources.Strings.EnterValidOID);
        }

        return ValidationResult.ValidResult;
    }
}
