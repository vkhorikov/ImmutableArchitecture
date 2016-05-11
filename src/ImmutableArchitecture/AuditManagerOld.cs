using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ImmutableArchitecture
{
    public class AuditManagerOld
    {
        private readonly int _maxEntriesPerFile;


        public AuditManagerOld(int maxEntriesPerFile)
        {
            _maxEntriesPerFile = maxEntriesPerFile;
        }


        public void AddRecord(string currentFile, string visitorName, DateTime timeOfVisit)
        {
            string[] lines = File.ReadAllLines(currentFile);

            if (lines.Length < _maxEntriesPerFile)
            {
                int lastIndex = int.Parse(lines.Last().Split(';')[0]);
                string newLine = (lastIndex + 1) + ";" + visitorName + ';' + timeOfVisit.ToString("s");
                File.AppendAllLines(currentFile, new[] { newLine });
            }
            else
            {
                string newLine = "1;" + visitorName + ';' + timeOfVisit.ToString("s");
                string newFileName = GetNewFileName(currentFile);
                File.WriteAllLines(newFileName, new[] { newLine });
            }
        }


        private string GetNewFileName(string existingFileName)
        {
            string fileName = Path.GetFileNameWithoutExtension(existingFileName);
            int index = int.Parse(fileName.Split('_')[1]);
            return "Audit_" + (index + 1) + ".txt";
        }


        public void RemoveMentionsAbout(string visitorName, string directoryName)
        {
            foreach (string fileName in Directory.GetFiles(directoryName))
            {
                string tempFile = Path.GetTempFileName();
                List<string> linesToKeep = File
                    .ReadLines(fileName)
                    .Where(line => !line.Contains(visitorName))
                    .ToList();

                if (linesToKeep.Count == 0)
                {
                    File.Delete(fileName);
                }
                else
                {
                    File.WriteAllLines(tempFile, linesToKeep);
                    File.Delete(fileName);
                    File.Move(tempFile, fileName);
                }
            }
        }
    }
}
