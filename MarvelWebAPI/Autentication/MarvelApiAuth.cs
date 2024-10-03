using System;
using System.Security.Cryptography;
using System.Text;

namespace MarvelWebAPI.Autentication
{
    public class MarvelApiAuth
    {
        private readonly string _publicKey;
        private readonly string _privateKey;

        public MarvelApiAuth(string publicKey, string privateKey)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
        }
    
        public string GetAuthenticationString()
        {
            //criar o timestamp
            var timestamp = DateTime.Now.Ticks.ToString();
            //gerar o hash pelo md5
            var hash = GetMd5Hash(timestamp + _privateKey + _publicKey);
            var md5Hash = $"ts={timestamp}&apikey={_publicKey}&hash={hash}";
            return md5Hash;
        }

        private string GetMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    
    }

  

}