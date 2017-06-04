using ESBackupAndReplication.AppData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ESBackupAndReplication
{
    public partial class ESBackupAndReplicationService : ServiceBase
    {
        private Initializer _Initializer { get; set; }
        public ESBackupAndReplicationService()
        {
            InitializeComponent();            
        }

        protected override void OnStart(string[] args)
        {
            this._Initializer = new Initializer();
        }

        protected override void OnStop()
        {
            this._Initializer.Stop();
        }
    }
}
