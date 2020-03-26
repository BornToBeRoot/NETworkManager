using Lextm.SharpSnmpLib;
using System.Globalization;
using System.Windows.Controls;

namespace NETworkManager.Validators
{
    public class OIDValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Use SharpSNMP new ObjectIdentifiert to validate oid
            try
            {
                // ReSharper disable once UnusedVariable
                var oid = new ObjectIdentifier(value as string);
            }
            catch (System.ArgumentException)
            {
                return new ValidationResult(false, Localization.Resources.Strings.EnterValidOID);
            }

            return ValidationResult.ValidResult;
        }
    }
}
