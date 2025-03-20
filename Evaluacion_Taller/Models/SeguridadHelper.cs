using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TallerMecanicoMVC.Models
{
    public static class SeguridadHelper
    {
        private static string clave = "MiClaveSegura123"; 

        public static string EncriptarAES(string texto)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(clave);
                Array.Resize(ref keyBytes, 16); 
                aesAlg.Key = keyBytes;
                aesAlg.IV = keyBytes;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(texto);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string DesencriptarAES(string textoEncriptado)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(clave);
                Array.Resize(ref keyBytes, 16);
                aesAlg.Key = keyBytes;
                aesAlg.IV = keyBytes;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(textoEncriptado)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
