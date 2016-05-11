using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ImmutableArchitecture
{
    public class AuditManager
    {
        private readonly int _maxEntriesPerFile;


        public AuditManager(int maxEntriesPerFile)
        {
            _maxEntriesPerFile = maxEntriesPerFile;
        }


        public FileAction AddRecord(FileContent currentFile, string visitorName, DateTime timeOfVisit)
        {
            List<AuditEntry> entries = Parse(currentFile.Content);

            if (entries.Count < _maxEntriesPerFile)
            {
                entries.Add(new AuditEntry(entries.Count + 1, visitorName, timeOfVisit));
                string[] newContent = Serialize(entries);

                return new FileAction(currentFile.FileName, ActionType.Update, newContent);
            }
            else
            {
                var entry = new AuditEntry(1, visitorName, timeOfVisit);
                string[] newContent = Serialize(new List<AuditEntry> { entry });
                string newFileName = GetNewFileName(currentFile.FileName);

                return new FileAction(newFileName, ActionType.Create, newContent);
            }
        }


        private string[] Serialize(List<AuditEntry> entries)
        {
            return entries
                .Select(entry => entry.Number + ";" + entry.Visitor + ";" + entry.TimeOfVisit.ToString("s"))
                .ToArray();
        }


        private List<AuditEntry> Parse(string[] content)
        {
            var result = new List<AuditEntry>();

            foreach (string line in content)
            {
                string[] data = line.Split(';');
                result.Add(new AuditEntry(int.Parse(data[0]), data[1], DateTime.Parse(data[2])));
            }

            return result;
        }


        private string GetNewFileName(string existingFileName)
        {
            string fileName = Path.GetFileNameWithoutExtension(existingFileName);
            int index = int.Parse(fileName.Split('_')[1]);
            return "Audit_" + (index + 1) + ".txt";
        }


        public IReadOnlyList<FileAction> RemoveMentionsAbout(string visitorName, FileContent[] directoryFiles)
        {
            return directoryFiles
                .Select(file => RemoveMentionsIn(file, visitorName))
                .Where(action => action != null)
                .Select(action => action.Value)
                .ToList();
        }


        private FileAction? RemoveMentionsIn(FileContent file, string visitorName)
        {
            List<AuditEntry> entries = Parse(file.Content);

            List<AuditEntry> newContent = entries
                .Where(x => x.Visitor != visitorName)
                .Select((entry, index) => new AuditEntry(index + 1, entry.Visitor, entry.TimeOfVisit))
                .ToList();

            if (newContent.Count == entries.Count)
                return null;

            if (newContent.Count == 0)
                return new FileAction(file.FileName, ActionType.Delete, new string[0]);

            return new FileAction(file.FileName, ActionType.Update, Serialize(newContent));
        }
    }


    public struct AuditEntry
    {
        public readonly int Number;
        public readonly string Visitor;
        public readonly DateTime TimeOfVisit;


        public AuditEntry(int number, string visitor, DateTime timeOfVisit)
        {
            Number = number;
            Visitor = visitor;
            TimeOfVisit = timeOfVisit;
        }
    }


    public struct FileAction
    {
        public readonly string FileName;
        public readonly string[] Content;
        public readonly ActionType Type;


        public FileAction(string fileName, ActionType type, string[] content)
        {
            FileName = fileName;
            Type = type;
            Content = content;
        }
    }


    public enum ActionType
    {
        Create,
        Update,
        Delete
    }


    public struct FileContent
    {
        public readonly string FileName;
        public readonly string[] Content;


        public FileContent(string fileName, string[] content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}
