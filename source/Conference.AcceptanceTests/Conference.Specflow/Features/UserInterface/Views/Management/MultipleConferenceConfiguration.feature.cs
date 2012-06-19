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
namespace Conference.Specflow.Features.UserInterface.Views.Management
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature : Xunit.IUseFixture<MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "MultipleConferenceConfiguration.feature"
#line hidden
        
        public MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Multiple Conference configuration scenarios for creating and editing many Confere" +
                    "nces", "In order to create multiple Conferences\r\nAs a Business Customer\r\nI want to be abl" +
                    "e to create multiple Conferences and set its properties", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        public virtual void SetFixture(MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute()]
        [Xunit.TraitAttribute("FeatureTitle", "Multiple Conference configuration scenarios for creating and editing many Confere" +
            "nces")]
        [Xunit.TraitAttribute("Description", "Multiple Seat Types are created and assigned to a new existing Conference")]
        public virtual void MultipleSeatTypesAreCreatedAndAssignedToANewExistingConference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Multiple Seat Types are created and assigned to a new existing Conference", ((string[])(null)));
#line 22
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Owner",
                        "Email",
                        "Name",
                        "Description",
                        "Slug",
                        "Start",
                        "End"});
            table1.AddRow(new string[] {
                        "Neuro%1",
                        "neuro@neuro.com",
                        "NEURO%1",
                        "Neuro Test conference %1",
                        "neuro%1",
                        "05/02/2012",
                        "07/12/2012"});
#line 23
testRunner.Given("this base conference information", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "Description",
                        "Quantity",
                        "Price"});
            table2.AddRow(new string[] {
                        "TEST1",
                        "Test seat type 1",
                        "100000",
                        "0"});
            table2.AddRow(new string[] {
                        "TEST2",
                        "Test seat type 2",
                        "100000",
                        "1"});
#line 26
testRunner.And("these Seat Types", ((string)(null)), table2);
#line 30
testRunner.When("the Business Customer proceed to create 10 \'random\' conferences");
#line 31
testRunner.Then("all the conferences are created");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                MultipleConferenceConfigurationScenariosForCreatingAndEditingManyConferencesFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
