using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Utilities
{
    public class ValidEmailDomainAttribute(string allowedDomain) : ValidationAttribute
    {
        private readonly string _allowedDomain = allowedDomain;

        public override bool IsValid(object? value)
        {
           string[] strings = value?.ToString()?.Split('@') ?? [];
           return strings[1].Equals(_allowedDomain, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
