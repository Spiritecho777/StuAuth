using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace StuAuth.Classe
{
    public class EncryptionManager
    {
        public void EncryptionToFile(string inputText, string outputFilePath)
        {
            using (Aes aes = Aes.Create())
            {
                // Génère une clé et un IV aléatoires
                aes.GenerateKey();
                aes.GenerateIV();

                using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    // Écrire l'IV dans le fichier pour pouvoir le réutiliser lors du déchiffrement
                    fileStream.Write(aes.IV, 0, aes.IV.Length);

                    // Crée un flux de chiffrement et chiffre les données
                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(inputText);
                    }
                }
            }
        }

        // Méthode de déchiffrement
        public void DecryptFromFile(string inputFilePath, string outputFilePath)
        {
            using (FileStream fileStream = new FileStream(inputFilePath, FileMode.Open))
            {
                using (Aes aes = Aes.Create())
                {
                    // Lire l'IV du fichier (pour pouvoir déchiffrer correctement)
                    byte[] iv = new byte[aes.BlockSize / 8]; // BlockSize divisé par 8 pour obtenir la taille en octets
                    fileStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    // Génère une clé aléatoire pour déchiffrer
                    aes.GenerateKey();

                    // Crée un flux de déchiffrement et lit les données
                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream))
                    {
                        string decryptedData = reader.ReadToEnd();
                        File.WriteAllText(outputFilePath, decryptedData);
                    }
                }
            }
        }
    }
}
