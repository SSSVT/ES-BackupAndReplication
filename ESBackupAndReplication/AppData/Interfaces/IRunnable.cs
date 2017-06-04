using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESBackupAndReplication.AppData.Interfaces
{
    interface IStartable
    {
        void Start();
        void Stop();
    }
}
