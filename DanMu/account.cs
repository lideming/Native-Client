using System;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DanMu
{
    class account
    {
        public static string name;
        public static string password;

        public static string nameMD5;
        public static string passwordMD5;

        public static void calcAccountMD5()
        {
            if (name != null && password != null)
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    nameMD5 = GetMd5Hash(md5Hash, name);
                    passwordMD5 = GetMd5Hash(md5Hash, password);
                    Debug.WriteLine("Account: " + nameMD5);
                    Debug.WriteLine("Password: " + passwordMD5);
                }
            }
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
