using Microsoft.AspNetCore.DataProtection;

namespace EmployeeManagement.Services
{
    public class DataProtectionService(IDataProtectionProvider provider) : IDataProtectionService
    {
        // The IDataProtector instance is used to perform the actual data protection operations
        private readonly IDataProtector _protector = provider.CreateProtector(Constants.DataProtectionPurposes.EmployeeId);

        // Protects the given plain text and returns the protected string
        public string Protect(string plainText)
        {
            return _protector.Protect(plainText);
        }

        // Unprotects the given protected text and returns the original plain text
        public string Unprotect(string protectedText)
        {
            return _protector.Unprotect(protectedText);
        }
    }
}
