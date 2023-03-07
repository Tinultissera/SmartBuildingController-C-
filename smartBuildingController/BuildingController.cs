using System;
using NSubstitute;

namespace smartBuildingController
{
    class BuildingController
    {
        // private variables
        private string buildingID;
        private string currentState;
        private string historyState;


        private ILightManager lightManager;
        private IFireAlarmManager fireAlarmManager;
        private IDoorManager doorManger;
        private IEmailService emailService;
        private IWebService webService;


        // overloaded constructor
        public BuildingController(string id)
        {
            buildingID = id.ToLower();
            currentState = "out of hours";
        }

        public BuildingController(string id, string startState)
        {
            if (startState.ToLower() == "open" || startState.ToLower() == "closed" || startState.ToLower() == "out of hours")
            {
                buildingID = id.ToLower();
                currentState = startState.ToLower();
            }
            else
            {
                // exception message if non of these states
                throw new ArgumentException("Argument Exception: BuildingController can only be initialised to the following states 'open', 'closed', 'out of hours'");
            }
        }

        public BuildingController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager, IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            buildingID = id.ToLower();
            currentState = "out of hours";
            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManger = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;
        }

        // setters

        //set building id
        public void SetBuildingID(string id)
        {
            buildingID = id.ToLower();
        }

        //set current state
        public bool SetCurrentState(string state)
        {
            switch (state.ToLower())
            {
                case "open":
                    // set the new state with current state 
                    if ((currentState == "out of hours" || currentState == "open") && doorManger.OpenAllDoors())
                    {
                        currentState = state.ToLower();
                    }
                    // if the current state is fire alarm or fire drill set current state as history state
                    else if (currentState == "fire alarm" || currentState == "fire drill")
                    {
                        currentState = historyState;
                    }
                    else
                    {
                        return false;
                    }
                    break;

                case "closed":
                    // set the new state with current state 
                    if ((currentState == "out of hours" || currentState == "closed") && doorManger.LockAllDoors())
                    {
                        lightManager.SetAllLights(false);
                        currentState = state.ToLower();
                    }
                    // if the current state is fire alarm or fire drill set current state as history state
                    else if (currentState == "fire alarm" || currentState == "fire drill")
                    {
                        currentState = historyState;
                    }
                    else
                    {
                        return false;
                    }
                    break;

                case "out of hours":
                    // set the new state with current state 
                    if (currentState == "open" || currentState == "closed" || currentState == "out of hours")
                    {
                        currentState = state.ToLower();
                    }
                    // if the current state is fire alarm or fire drill set current state as history state
                    else if (currentState == "fire alarm" || currentState == "fire drill")
                    {
                        currentState = historyState;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "fire drill":
                    if (currentState != "fire alarm")
                    {
                        historyState = currentState;
                        currentState = state.ToLower();
                    }
                    else
                    {
                        return false;
                    }
                    break;

                case "fire alarm":
                    if (currentState != "fire drill")
                    {
                        historyState = currentState;
                        currentState = state.ToLower();
                        fireAlarmManager.SetAlarm(true);
                        doorManger.OpenAllDoors();
                        lightManager.SetAllLights(true);
                        webService.LogFireAlarm("fire alarm");
                        try
                        {
                            webService.LogFireAlarm("fire alarm");
                        }
                        catch (Exception ex)
                        {
                            // send the email
                            emailService.SendMail("smartbuilding@uclan.ac.uk", "failed to log alarm", ex.ToString());
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                default:
                    return false;
            }
            return true;
        }

        //getters

        // return building id
        public string GetBuildingID() 
        {
            return buildingID;
        }

        // return current state
        public string GetCurrentState()
        {
            return currentState;
        }

        public string GetStatusReport()
        {
            string logFaults = "";
            if (CheckFaults(lightManager.GetStatus()))
            {
                logFaults += "Lights,";
            }

            if (CheckFaults(doorManger.GetStatus()))
            {
                logFaults += "Doors,";
            }

            if (CheckFaults(fireAlarmManager.GetStatus()))
            {
                logFaults += "FireAlarm,";
            }

            if (logFaults != "")
            {
                webService.LogEngineerRequired(logFaults);
            }
            string status = lightManager.GetStatus() + doorManger.GetStatus() + fireAlarmManager.GetStatus();
            return status;
        }

        // check whether the status contains fault
        public bool CheckFaults(string status)


        {
            string[] statusList = status.Split(',');

            for (int i = 1; i < statusList.Length; i++)
            {
                if (statusList[i].ToUpper() == "FAULT")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
