using Lextm.SharpSnmpLib;
using NETworkManager.Models.Settings;
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
                return new ValidationResult(false, LocalizationManager.GetStringByKey("String_ValidationError_EnterValidOID"));
            }

            return ValidationResult.ValidResult;
        }
    }
}
