using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smartBuildingController
{
    public interface IFireAlarmManager
    {
        void SetAlarm(bool isActive);

        string GetStatus();
    }
}
