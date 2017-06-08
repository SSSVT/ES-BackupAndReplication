using ESBackupAndReplication.AppData.CRON.Jobs;
using ESBackupAndReplication.AppData.Interfaces;
using ESBackupAndReplication.ESBackupServerService;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESBackupAndReplication.AppData.CRON
{
    public class CronScheduler: IStartable
    {
        private IScheduler _scheduler;
        #region Singleton
        private static CronScheduler _Instance { get; set; }
        private CronScheduler()
        {
            this._scheduler = StdSchedulerFactory.GetDefaultScheduler();            
        }
        public static CronScheduler GetInstance()
        {
            if (CronScheduler._Instance == null)
                CronScheduler._Instance = new CronScheduler();

            return CronScheduler._Instance;
        }
        #endregion
        #region Schedule Jobs
        public void ScheduleTemplate(BackupTemplate template, Guid sessionID)
        {
            IJobDetail job = JobBuilder.Create<TemplateBackupJob>()
            .WithIdentity("job_" + template.ID.ToString())
            .Build();

            job.JobDataMap.Add("template", template);
            job.JobDataMap.Add("sessionID", sessionID);

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger_" + template.ID.ToString())
                .StartNow()
                .WithCronSchedule(template.CRONRepeatInterval)
                .Build();

            this._scheduler.ScheduleJob(job, trigger);
        }
        public void ScheduleClient(int interval)
        {
            IJobDetail job = JobBuilder.Create<Initializer>()
                .WithIdentity("job_client")
                .Build();                   

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger_client")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever())
                .Build();

            this._scheduler.ScheduleJob(job, trigger);
        }
        #endregion
        public void Start()
        {
            this._scheduler.Start();
        }

        public void Stop()
        {
            this._scheduler.Shutdown();
        }
        public void ClearJobs()
        {
            this._scheduler.Clear();
        }
    }
}
