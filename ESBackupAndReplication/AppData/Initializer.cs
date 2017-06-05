using ESBackupAndReplication.AppData.Authentication;
using ESBackupAndReplication.AppData.CRON;
using ESBackupAndReplication.AppData.Interfaces;
using ESBackupAndReplication.ESBackupServerService;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESBackupAndReplication.AppData
{
    public class Initializer: IStartable, IJob
    {
        #region Local properties
        private CronScheduler _Scheduler { get; set; }
        private User _User { get; set; }
        private LoginResponse _LoginResponse { get; set; }
        private Configuration _Config { get; set; }
        #endregion       
        public Initializer()
        {
            this.Start();
        }

        public void Start()
        {
            this._Scheduler = CronScheduler.GetInstance();
            this._Scheduler.ScheduleClient(1800); //Default interval - 1800
        }
        public void Stop()
        {
            if (this._LoginResponse.UTCExpiration > DateTime.UtcNow)
            {
                ESBackupServerServiceClient client = new ESBackupServerServiceClient();
                client.Logout(this._LoginResponse.SessionID);
                client.Close();
            }
        }

        public bool EstabilishConnection()
        {
            bool estabilished;
            ESBackupServerServiceClient client = new ESBackupServerServiceClient();
            //TODO: Correct parameters in Registration
            RegistrationResponse response = client.RequestRegistration("PC-Name", "hwid");
            if (response.Status == ClientStatus.Verified)
            {
                this._User = new User()
                {
                    Username = response.Username,
                    Password = response.Password
                };
                estabilished = true;
            }
            else
                estabilished = false;

            client.Close();
            return estabilished;
        }
        public void RefreshConnection()
        {
            ESBackupServerServiceClient client = new ESBackupServerServiceClient();
            this._LoginResponse = client.Login(this._User.Username, this._User.Password);
            client.ClientReportUpdated(this._LoginResponse.SessionID);

            //TODO: Implement - If config has changed then download new one               
            if (client.HasConfigUpdate(this._LoginResponse.SessionID,this._Config.Generated))
            {
                this._Scheduler.ClearJobs();
                this._Config = client.GetConfiguration(this._LoginResponse.SessionID);

                if (this._Config.Templates.Length != 0)
                {
                    foreach (BackupTemplate template in this._Config.Templates)
                    {
                        if (template.Enabled)
                        {                            
                            this._Scheduler.ScheduleTemplate(template, this._LoginResponse.SessionID);
                        }                        
                    }
                }
            }
            client.Close();
        }

        public void Execute(IJobExecutionContext context)
        {
            if (this._User == null)
                if (this.EstabilishConnection())
                    this.RefreshConnection();
            else
                this.RefreshConnection();
        }
    }
}
