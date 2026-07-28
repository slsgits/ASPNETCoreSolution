namespace EmployeeManagement.Services
{
    public interface IDataProtectionService
    {
        string Protect(string plainText);
        string Unprotect(string protectedText);
    }
}