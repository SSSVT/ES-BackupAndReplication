using ESBackupAndReplication.Access;
using ESBackupAndReplication.Backup;
using ESBackupAndReplication.ESBackupServerService;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESBackupAndReplication.AppData.CRON.Jobs
{
    public class TemplateBackupJob : IJob
    {
        #region Local propeties
        private BackupManager _Manager { get; set; }
        private Guid _SessionID { get; set; }
        #endregion
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;
            BackupTemplate template = (BackupTemplate)dataMap["template"];
            this._SessionID = (Guid)dataMap["sessionID"];                       
            switch (template.BackupType)
            {
                case 0:
                    this.StoreInfo(this.DoFullBackup(template));
                    break;
                case 1:
                    this.StoreInfo(this.DoDifferencialBackup(template));
                    break;
                case 2:
                    this.StoreInfo(this.DoIncrementalBackup(template));
                    break;
                default:
                    break;
            }
        }
        #region Backup methods        
        private List<BackupInfo> DoFullBackup(BackupTemplate template)
        {
            //TODO: Create backup infos
            List<BackupInfo> data = new List<BackupInfo>();
            ushort i = 1;
            foreach (BackupTemplatePath path in template.Paths)
            {
                DateTime Begin = DateTime.UtcNow;
                this._Manager = new BackupManager(new LocalAccess());
                this._Manager.FullBackup(path.Source, path.Destination, template.SearchPattern, ".*", template.Compression);
                DateTime End = DateTime.UtcNow;
                data.Add(new BackupInfo()
                {
                    IDClient = template.IDClient,
                    IDBackupTemplate = template.ID,

                    Name = template.Name,
                    Description = "Backup created by template " + template.Name + "at time " + DateTime.UtcNow.ToShortDateString(),

                    BackupType = 0,

                    Source = path.Source,
                    Destination = path.Destination,

                    Compressed = template.Compression,

                    UTCStart = Begin,
                    UTCEnd = End,
                    Status = 0,

                    PathOrder = i,
                    EmailSent = template.IsEmailNotificationEnabled
                }
                );
                i++;
            }

            return data;
        }
        private List<BackupInfo> DoDifferencialBackup(BackupTemplate template)
        {
            //TODO: Create backup infos and pass parameters into DiffBackup
            List<BackupInfo> data = new List<BackupInfo>();
            foreach (BackupTemplatePath path in template.Paths)
            {
                this._Manager = new BackupManager(new LocalAccess());
                //this.Manager.DifferentialBackup();
            }

            return data;
        }
        private List<BackupInfo> DoIncrementalBackup(BackupTemplate template)
        {
            //TODO: Do incremental and pass parameters into IncrBackup
            List<BackupInfo> data = new List<BackupInfo>();
            foreach (BackupTemplatePath path in template.Paths)
            {
                this._Manager = new BackupManager(new LocalAccess());
                //this.Manager.IncrementalBackup();
            }
            return data;
        }
        private void StoreInfo(List<BackupInfo> data)
        {
            ESBackupServerServiceClient client = new ESBackupServerServiceClient();

            foreach (BackupInfo info in data)
            {
                client.CreateBackup(info, this._SessionID);
            }

            client.Close();
        }
        #endregion
    }
}
