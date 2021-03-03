using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace SpecFlowComputerVision
{
    [Binding]
    public class PaintSteps
    {
        private PaintPomBase paintPomBase = null;

        [BeforeScenario()]
        public void  LaunchApplication(TestContext context)
        {
            paintPomBase = new PaintPomBase();
            paintPomBase.CreateNewPaint3DProject();
        }

        [AfterScenario()] 
        public void CleanUp()
        {
            paintPomBase.TearDown();
        }

        public PaintSteps()
        {
            paintPomBase = new PaintPomBase();
        }

        [Given(@"Draw Rectangle")]
        public void GivenDrawRectangle()
        {
            Assert.IsTrue(true);
        }

        [Given(@"I select brush")]
        public void GivenISelectBrush()
        {
            paintPomBase.SetupBrushesPane();
        }

        [Given(@"I draw rectangle")]
        [Then(@"I draw rectangle")]
        public void ThenIDrawRectangle()
        {
            paintPomBase.DrawRectangle();
        }

        [Given(@"I draw a circle")]
        [Then(@"I draw a circle")]
        public void ThenIDrawACircle()
        {
            paintPomBase.DrawCircle();
        }

        [Given(@"I draw a traingle")]
        public void GivenIDrawATraingle()
        {
            paintPomBase.DrawTriangle();
        }



    }
}
