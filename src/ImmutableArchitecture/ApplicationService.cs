using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ImmutableArchitecture
{
    public class ApplicationService
    {
        private readonly string _directoryName;
        private readonly AuditManager _auditManager;
        private readonly Persister _persister;


        public ApplicationService(string directoryName)
        {
            _directoryName = directoryName;
            _auditManager = new AuditManager(10);
            _persister = new Persister();
        }


        public void RemoveMentionsAbout(string visitorName)
        {
            FileContent[] files = _persister.ReadDirectory(_directoryName);
            IReadOnlyList<FileAction> actions = _auditManager.RemoveMentionsAbout(visitorName, files);
            _persister.ApplyChanges(actions);
        }


        public void AddRecord(string visitorName, DateTime timeOfVisit)
        {
            FileInfo fileInfo = new DirectoryInfo(_directoryName)
                .GetFiles()
                .OrderByDescending(x => x.LastWriteTime)
                .First();

            FileContent file = _persister.ReadFile(fileInfo.Name);
            FileAction action = _auditManager.AddRecord(file, visitorName, timeOfVisit);
            _persister.ApplyChange(action);
        }
    }
}
