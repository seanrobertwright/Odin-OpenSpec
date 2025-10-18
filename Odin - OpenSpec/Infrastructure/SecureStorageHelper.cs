using System.Security.Cryptography;
using System.Text;

namespace Odin_OpenSpec.Infrastructure
{
    public static class SecureStorageHelper
    {
        // Encrypts data using Windows DPAPI
        public static byte[] Protect(string plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        }

        // Decrypts data using Windows DPAPI
        public static string Unprotect(byte[] protectedData)
        {
            var data = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
    }
}
