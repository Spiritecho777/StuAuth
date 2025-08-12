using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;

namespace StuAuth.Classe
{
    public class EncryptionManager ()
    {
        private string Password = Properties.Settings.Default.KeyPass; // Changez ceci par une clé secrète sécurisée
        private const int KeySize = 256; // Taille de la clé en bits
        private const int Iterations = 100_000; // Nombre d'itérations pour dériver la clé
        
        // Méthode pour dériver une clé AES à partir d'un mot de passe
        private byte[] DeriveKey(string password, byte[] salt)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return rfc2898DeriveBytes.GetBytes(KeySize / 8);
            }
        }

        public void EncryptionToFile(string inputFilePath, string outputFilePath)
        {
            using (Aes aes = Aes.Create())
            {
                // Génère un IV unique pour ce chiffrement
                aes.GenerateIV();
                byte[] salt = aes.IV;  // Utiliser l'IV comme "salt" pour dériver la clé

                aes.Key = DeriveKey(Password, salt);  // Dériver la clé à partir du mot de passe et du salt

                // Lire tout le contenu du fichier déchiffré en une seule chaîne
                string inputText = File.ReadAllText(inputFilePath);

                using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    // Stocke le salt (IV) au début du fichier chiffré
                    fileStream.Write(salt, 0, salt.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cryptoStream))
                    {
                        // Chiffre l'intégralité du contenu et l'écrit dans le fichier chiffré
                        writer.Write(inputText);
                    }
                }
            }
        }

        public void DecryptFromFile(string inputFilePath, string outputFilePath)
        {
            try
            {
                Debug.WriteLine(Password);
                using (FileStream fileStream = new FileStream(inputFilePath, FileMode.Open))
                {
                    using (Aes aes = Aes.Create())
                    {
                        // Lire le sel (IV) depuis le fichier
                        byte[] salt = new byte[aes.BlockSize / 8];
                        int bytesRead = fileStream.Read(salt, 0, salt.Length);
                        if (bytesRead != salt.Length)
                        {
                            throw new CryptographicException("Impossible de lire le sel (IV) du fichier.");
                        }

                        aes.Key = DeriveKey(Password, salt);
                        aes.IV = salt;

                        // Déchiffrer les données restantes
                        using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            string decryptedData = reader.ReadToEnd();

                            // Supprimer le point-virgule (;) s'il est au début de la chaîne déchiffrée
                            if (decryptedData.StartsWith(";"))
                            {
                                decryptedData = decryptedData.Substring(1);
                            }

                            // Ajouter les données déchiffrées au fichier
                            using (StreamWriter writer = new StreamWriter(outputFilePath, false)) // false pour écraser, pas append
                            {
                                writer.WriteLine(decryptedData); // Écrit le contenu déchiffré sans le ';'
                            }
                        }
                    }
                }
            }
            catch (CryptographicException ex)
            {
                var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
                System.Windows.MessageBox.Show(
                    loc["Encryption1"],
                    loc["Encryption2"],
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                System.Windows.Application.Current.Shutdown();

                Debug.WriteLine("Erreur de décryption : " + ex.Message);
            }
        }
    }
}
