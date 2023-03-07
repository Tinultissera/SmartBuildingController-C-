using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smartBuildingController
{
    public interface ILightManager
    {
        void SetLight(bool isOn, int lightID);

        void SetAllLights(bool isOn);

        string GetStatus();
    }
}
