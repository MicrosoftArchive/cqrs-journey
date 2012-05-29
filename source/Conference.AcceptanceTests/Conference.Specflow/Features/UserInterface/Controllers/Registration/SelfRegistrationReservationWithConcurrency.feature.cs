﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Conference.Specflow.Features.UserInterface.Controllers.Registration
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature : Xunit.IUseFixture<Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SelfRegistrationReservationWithConcurrency.feature"
#line hidden
        
        public Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Self-Registrant scenarios for making a Reservation for a Conference site where mu" +
                    "ltiple Registrants make simultaneous reservations", "In order to reserve Seats for a Conference\r\nAs an Attendee\r\nI want to be able to " +
                    "select an Order Item from one or many of the available where other Registrants m" +
                    "ay also be interested", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 20
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "rate",
                        "quota"});
            table1.AddRow(new string[] {
                        "CQRS Workshop",
                        "$500",
                        "10"});
#line 21
 testRunner.Given("the list of the available Order Items for the CQRS summit 2012 conference", ((string)(null)), table1);
#line hidden
        }
        
        public virtual void SetFixture(Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute()]
        [Xunit.TraitAttribute("FeatureTitle", "Self-Registrant scenarios for making a Reservation for a Conference site where mu" +
            "ltiple Registrants make simultaneous reservations")]
        [Xunit.TraitAttribute("Description", "Many Registrants try to reserve the same Order Item and not all of them get the r" +
            "eservation")]
        public virtual void ManyRegistrantsTryToReserveTheSameOrderItemAndNotAllOfThemGetTheReservation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Many Registrants try to reserve the same Order Item and not all of them get the r" +
                    "eservation", new string[] {
                        "NoWatiN"});
#line 27
this.ScenarioSetup(scenarioInfo);
#line 20
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "quantity"});
            table2.AddRow(new string[] {
                        "CQRS Workshop",
                        "1"});
#line 28
 testRunner.When("20 Registrants selects these Order Items", ((string)(null)), table2);
#line 31
 testRunner.Then("only 10 Registrants get confirmed reservations for the selected Order Items");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                Self_RegistrantScenariosForMakingAReservationForAConferenceSiteWhereMultipleRegistrantsMakeSimultaneousReservationsFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
