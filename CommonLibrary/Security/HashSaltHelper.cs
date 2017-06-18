using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web.Security;

namespace CommonLibrary.Security
{
    public static class HashSaltHelper
    {
        //default size
        public  const int SALT_SIZE = 10;

        public static string CreateSalt()
        {
            return CreateSalt(SALT_SIZE);
        }

        public static string CreateSalt(int size)
        {
            //create a cryptographic random number
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            //return a base64 string representation of the random number
            //does not work because if we do this the string returned is longer than the size intended/passed in
            //return Convert.ToBase64String(buff);
            //to fix this size issue we just return from the beginning of the base64string to the size that was passed in
            //basically using size of 10 right now.
            return Convert.ToBase64String(buff,0,size).Substring(0,size);
        }

        public static string CreatePasswordHash(string password, string salt)
        {
            string saltAndPwd = String.Concat(password, salt);
            string hashedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "SHA1");
            hashedPwd = String.Concat(hashedPwd, salt);
            return hashedPwd;
        }
    }
}
