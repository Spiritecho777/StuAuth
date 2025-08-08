using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace StuAuth.Classe
{
    public class AccountManager
    {
        private string appDirectory;
        private string filePath;
        private string filePathE;

        public AccountManager()
        {
            appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");

            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            filePathE = System.IO.Path.Combine(appDirectory, "Account.dat");
            filePath = System.IO.Path.Combine(appDirectory, "Account_decrypted.dat");

            if (File.Exists(filePathE))
            {
                EncryptionManager encryptionManager = new EncryptionManager();
                encryptionManager.DecryptFromFile(filePathE, filePath);
            }
            else
            {
                CreateFile();
            }
        }

        private void CreateFile()
        {
            File.WriteAllText(filePathE, string.Empty);
        }

        private void Chiffrement()
        {
            EncryptionManager encryptionManager = new EncryptionManager();
            encryptionManager.EncryptionToFile(filePath, filePathE);
        }


        #region Traitements de données
        public bool FileExists()
        {
            return File.Exists(filePath);
        }

        public List<string> ReadLines()
        {
            if (FileExists())
            {
                return File.ReadAllLines(filePath).ToList();
            }
            else
            {
                // Le fichier n'existe pas, retour d'une liste vide ou gestion d'erreur
                CreateFile();
                return File.ReadAllLines(filePath).ToList();
            }
        }

        public void WriteLines(List<string> lines)
        {
            File.WriteAllLines(filePath, lines);
        }

        public Dictionary<string, int> CountFolderOccurrences()
        {
            var occurrences = new Dictionary<string, int>();
            var lines = ReadLines();

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2)
                {
                    string[] folderParts = parts[0].Split('\\');
                    string folderName = folderParts[0];

                    if (occurrences.ContainsKey(folderName))
                    {
                        occurrences[folderName]++;
                    }
                    else
                    {
                        occurrences[folderName] = 1;
                    }
                }
            }

            return occurrences;
        }

        public List<string> GetValidLines(Dictionary<string, int> occurrences)
        {
            var validLines = new List<string>();
            var lines = ReadLines();

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2)
                {
                    string[] folderParts = parts[0].Split('\\');
                    string folderName = folderParts[0];

                    if (!string.IsNullOrEmpty(parts[1]) || occurrences[folderName] == 1)
                    {
                        validLines.Add(line);
                    }
                }
            }

            return validLines;
        }

        public List<string> GetAccountsByFolder(string folderName)
        {
            var accounts = new List<string>();

            if (FileExists())
            {
                var lines = ReadLines();

                foreach (var line in lines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == 2)
                    {
                        string[] folderParts = parts[0].Split('\\');
                        if (folderParts[0] == folderName)
                        {
                            accounts.Add(line);
                        }
                    }
                }
            }
            return accounts;
        }

        public List<string> GetAllOtpUri()
        {
            var otps = new List<string>();

            if (FileExists())
            {
                var lines = ReadLines();

                foreach (var line in lines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == 2)
                    {
                        string otpUri = parts[1];

                        var match = Regex.Match(otpUri, @"secret=([^&]+)");
                        if (match.Success)
                        {
                            otps.Add(match.Groups[1].Value);
                        }
                    }
                }
            }
            return otps;
        }
        #endregion

        #region Manipulations de données
        public void Add(string folderName)
        {
            if (!string.IsNullOrEmpty(folderName))
            {
                if (Directory.Exists(appDirectory))
                {
                    if (File.Exists(filePath))
                    {
                        List<string> line = File.ReadAllLines(filePath).ToList();

                        line.Add($"{folderName}\\;");
                        File.WriteAllLines(filePath, line);
                    }
                    else
                    {
                        List<string> line = new List<string>();

                        line.Add($"{folderName}\\;");
                        File.WriteAllLines(filePath, line);
                    }
                }
                else
                {
                    Directory.CreateDirectory(appDirectory);
                    if (File.Exists(filePath))
                    {
                        List<string> line = File.ReadAllLines(filePath).ToList();

                        line.Add($"{folderName}\\;");
                        File.WriteAllLines(filePath, line);
                    }
                    else
                    {
                        List<string> line = new List<string>();

                        line.Add($"{folderName}\\;");
                        File.WriteAllLines(filePath, line);
                    }
                }
            }
            Chiffrement();
        }

        public bool DeleteFolderOrAccount(string name, bool isFolder, bool force = false)
        {
            if (!FileExists()) return false;

            var lines = ReadLines();
            bool itemDeleted = false;

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                string[] parts = lines[i].Split(';');
                if (parts.Length < 2) continue;

                if (isFolder)
                {
                    string[] folderParts = parts[0].Split('\\');
                    if (folderParts[0] == name)
                    {
                        bool hasAccount = !string.IsNullOrEmpty(parts[1]);

                        if (hasAccount && !force)
                        {
                            return false;
                        }

                        lines.RemoveAt(i);
                        itemDeleted = true;
                    }
                }
                else
                {
                    string[] accountParts = parts[0].Split('\\');
                    if (accountParts.Length > 1 && accountParts[1] == name)
                    {
                        lines.RemoveAt(i);
                        itemDeleted = true;
                    }
                }
            }

            if (itemDeleted)
            {
                WriteLines(lines);
            }

            Chiffrement();
            return itemDeleted;
        }

        public void RenameFolderOrAccount(string oldName, string newName, bool isFolder)
        {
            if (!FileExists()) return;

            var lines = ReadLines();
            for (int i = 0; i < lines.Count; i++)
            {
                string[] parts = lines[i].Split(';');
                if (parts.Length < 2) continue;

                if (isFolder)
                {
                    string[] folderParts = parts[0].Split('\\');
                    if (folderParts[0] == oldName)
                    {
                        folderParts[0] = newName;
                        parts[0] = string.Join("\\", folderParts);
                    }
                }
                else
                {
                    string[] accountParts = parts[0].Split('\\');
                    if (accountParts.Length > 1 && accountParts[1] == oldName)
                    {
                        accountParts[1] = newName;
                        parts[0] = string.Join("\\", accountParts);
                        parts[1] = UpdateUri(parts[1], newName);
                    }
                }

                lines[i] = string.Join(";", parts);
            }

            WriteLines(lines);
            Chiffrement();
        }

        private string UpdateUri(string uri, string newName)
        {
            if (string.IsNullOrEmpty(uri)) return uri;

            string[] uriParts = uri.Split('/');
            if (uriParts.Length < 4) return uri;

            string namePart = uriParts[^1];
            string[] nameDetails = namePart.Split('?');
            if (nameDetails.Length < 2) return uri;

            nameDetails[0] = Uri.EscapeDataString(newName);
            uriParts[^1] = string.Join("?", nameDetails);

            return string.Join("/", uriParts);
        }

        public void AddAccount(string account)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(account);
            }
            Chiffrement();
        }
        #endregion
    }
}
