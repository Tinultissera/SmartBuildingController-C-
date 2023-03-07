using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;

namespace smartBuildingController
{

    class BuildingControllerTests
    {
        private readonly ILightManager lightManager;
        private readonly IFireAlarmManager fireAlarmManager;
        private readonly IDoorManager doorManger;
        private readonly IEmailService emailService;
        private readonly IWebService webService;
        public BuildingControllerTests()
        { 
            lightManager = Substitute.For<ILightManager>();
            fireAlarmManager = Substitute.For<IFireAlarmManager>();
            doorManger = Substitute.For<IDoorManager>();
            emailService = Substitute.For<IEmailService>();
            webService = Substitute.For<IWebService>();
        }

        [TestCase("b-10")]
        [TestCase("b-20")]
        public void BuildingControllerShould_ReturnsSameBuildingID(string buildingId) // test Get building ID
        {
            BuildingController buildingController = new BuildingController(buildingId);
            var result = buildingController.GetBuildingID();
            Assert.That(result, Is.EqualTo(buildingId));
        }

        [Test]
        public void BuildingControllerShould_RetrunStatusisOutOfHours_whenCurrentStatusIsOutOfHours() // Test Status when current status is out of hours
        {
            BuildingController buildingController = new BuildingController("B-10");
            var result = buildingController.GetCurrentState();
            Assert.That(result, Is.EqualTo("out of hours"));
        }

        [TestCase("b-10", "open")]
        [TestCase("b-10", "closed")]
        [TestCase("b-10", "out of hours")]
        public void BuildingControllerShould_RetrunStatusAndBuildingID_whenUserInput(string bID, string bState) //Test status when user input status
        {
            BuildingController buildingController = new BuildingController(bID, bState);
            var buildingstate = buildingController.GetCurrentState();
            string buildingId = buildingController.GetBuildingID();
            Assert.That(buildingId, Is.EqualTo(bID));
            Assert.That(buildingstate, Is.EqualTo(bState));
        }

        [TestCase("B-10", "fire drill")]
        public void BuildingConstructer_whenConstructor_exceptiontest(string bID, string bState) // Test exceptionMessage
        {
            string exceptionMessage = "Argument Exception: BuildingController can only be initialised to the following states 'open', 'closed', 'out of hours'";
            try
            {
                BuildingController buildingController = new BuildingController(bID, bState);
                var buildingstatus = buildingController.GetCurrentState();
                string buildingId = buildingController.GetBuildingID();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
            }
        }

        [Test]
        public void BuildingControllerShould_SetBuildingID()  // test set building ID
        {
            BuildingController buildingController = new BuildingController("B-10");
            buildingController.SetBuildingID("B-20");
            string buildingId = buildingController.GetBuildingID();

            Assert.That(buildingId, Is.EqualTo("b-20"));
        }

        [Test]
        public void BuildingControllerShould_GetStatusReport() // Test get status report
        {
            string expectStatus = "Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,FireAlarm,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,";

            lightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            doorManger.GetStatus().Returns("Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,");
            fireAlarmManager.GetStatus().Returns("FireAlarm,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            BuildingController buildingController = new BuildingController("B-10", lightManager, fireAlarmManager, doorManger, webService, emailService);
            string status = buildingController.GetStatusReport();

            Assert.That(status, Is.EqualTo(expectStatus));
        }

        [TestCase("Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", false)]
        [TestCase("Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", false)]
        [TestCase("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK", true)]
        [TestCase("FireAlarm,OK,OK,OK,FAULT,OK,OK,OK,OK,OK,FAULT,", true)]
        public void BuildingController_CheckFaults(string status, bool exceptedoutput) // Test check faults
        {
            BuildingController buildingController = new BuildingController("B0010");
            bool result = buildingController.CheckFaults(status);

            Assert.That(result, Is.EqualTo(exceptedoutput));
        }

        [TestCase("open", true)]
        [TestCase("closed", true)]
        [TestCase("out of hours", true)]
        [TestCase("fire drill", true)]
        [TestCase("fire alarm", true)]
        [TestCase("close", false)]
        public void BuildingControllerShould_SetCurrentState_WhenNoFault(string bState, bool exceptedoutput) // no fault in open and close state
        {
            doorManger.OpenAllDoors().Returns(true);
            doorManger.LockAllDoors().Returns(true);
            BuildingController buildingController = new BuildingController("B-10", lightManager, fireAlarmManager, doorManger, webService, emailService);
            buildingController.SetCurrentState(bState);
            bool result = buildingController.SetCurrentState(bState);

            Assert.That(result, Is.EqualTo(exceptedoutput));
        }

        [TestCase("open", false)]
        [TestCase("closed", false)]
        [TestCase("out of hours", true)]
        [TestCase("fire drill", true)]
        [TestCase("fire alarm", true)]
        [TestCase("close", false)]
        public void BuildingControllerShould_SetCurrentState_WhenFault(string bState, bool exceptedoutput) // fault in open and close state
        {
            doorManger.OpenAllDoors().Returns(false);
            doorManger.LockAllDoors().Returns(false);
            BuildingController buildingController = new BuildingController("B-10", lightManager, fireAlarmManager, doorManger, webService, emailService);
            buildingController.SetCurrentState(bState);
            bool result = buildingController.SetCurrentState(bState);

            Assert.That(result, Is.EqualTo(exceptedoutput));
        }

        [TestCase("open", "open", true)]
        [TestCase("open", "closed", false)]
        [TestCase("open", "out of hours", true)]
        [TestCase("open", "fire drill", true)]
        [TestCase("open", "fire alarm", true)]
        [TestCase("closed", "closed", true)]
        [TestCase("closed", "open", false)]
        [TestCase("closed", "out of hours", true)]
        [TestCase("closed", "fire drill", true)]
        [TestCase("closed", "fire alarm", true)]
        [TestCase("out of hours", "closed", true)]
        [TestCase("out of hours", "open", true)]
        [TestCase("out of hours", "out of hours", true)]
        [TestCase("out of hours", "fire drill", true)]
        [TestCase("out of hours", "fire alarm", true)]

        public void BuildingControllerShould_SetCurrentState(string bState, string nextState, bool exceptedoutput) // text with next status
        {
            doorManger.OpenAllDoors().Returns(true);
            doorManger.LockAllDoors().Returns(true);
            BuildingController buildingController = new BuildingController("B-10", lightManager, fireAlarmManager, doorManger, webService, emailService);
            buildingController.SetCurrentState(bState);
            bool result = buildingController.SetCurrentState(nextState);

            Assert.That(result, Is.EqualTo(exceptedoutput));
        }

        [TestCase("fire drills", "open", true)]
        [TestCase("fire drills", "closed", true)]
        [TestCase("fire drills", "out of hours", true)]
        [TestCase("fire drills", "fire drills", false)]
        [TestCase("fire drills", "fire alarm", true)]
        [TestCase("fire alarm", "open", true)]
        [TestCase("fire alarm", "closed", true)]
        [TestCase("fire alarm", "out of hours", true)]
        [TestCase("fire alarm", "fire drills", false)]
        [TestCase("fire alarm", "fire alarm", true)]

        public void BuildingControllerShould_SetCurrentState_WhenfiredrillOrfirealarm(string bState, string historyState, bool exceptedoutput) // check history status 
        {
            doorManger.OpenAllDoors().Returns(true);
            doorManger.LockAllDoors().Returns(true);
            BuildingController buildingController = new BuildingController("B-10", lightManager, fireAlarmManager, doorManger, webService, emailService);
            buildingController.SetCurrentState(bState);
            bool result = buildingController.SetCurrentState(historyState);

            Assert.That(result, Is.EqualTo(exceptedoutput));
        }
    }

}
