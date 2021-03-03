using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace SpecFlowComputerVision
{
    class PaintPomBase
    {
        // Note: append /wd/hub to the URL if you're directing the test at Appium
        protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string Paint3DAppId = @"Microsoft.MSPaint_8wekyb3d8bbwe!Microsoft.MSPaint";

        protected static WindowsDriver<WindowsElement> session;
        private WindowsElement inkCanvas;
        private WindowsElement undoButton;
        private WindowsElement brushesPane;
        private const string eraserWidth = "8";

        private static Point A = new Point(-298, -214);
        private static Point B = new Point(298, -298);
        private static Point C = new Point(298, 298);
        private static Point D = new Point(-298, 214);
        private static Point E = new Point(-38, 0);

        public PaintPomBase()
        {
            // Launch Paint 3D application if it is not yet launched
            if (session == null)
            {
                // Create a new session to launch Paint 3D application

                AppiumOptions opt = new AppiumOptions();
                opt.AddAdditionalCapability("app", Paint3DAppId);
                opt.AddAdditionalCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), opt);
                //Assert.IsNotNull(session);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);

                // Maximize Paint 3D window to ensure all controls being displayed
                session.Manage().Window.Maximize();
            }
        }

        public void TearDown()
        {
            // Close the application and delete the session
            if (session != null)
            {
                ClosePaint3D();
                session.Quit();
                session = null;
            }
        }

        public void CreateNewPaint3DProject()
        {
            try
            {
                session.FindElementByAccessibilityId("WelcomeScreenNewButton").Click();
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            catch
            {
                // Create a new Paint 3D project by pressing Ctrl + N
                session.SwitchTo().Window(session.CurrentWindowHandle);
                session.Keyboard.SendKeys(Keys.Control + "n" + Keys.Control);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                DismissSaveConfirmDialog();
            }
        }

        private static void DismissSaveConfirmDialog()
        {
            try
            {
                WindowsElement closeSaveConfirmDialog = session.FindElementByAccessibilityId("CloseSaveConfirmDialog");
                closeSaveConfirmDialog.FindElementByAccessibilityId("SecondaryBtnG3").Click();
            }
            catch { }
        }

        private static void ClosePaint3D()
        {
            try
            {
                session.Close();
                string currentHandle = session.CurrentWindowHandle; // This should throw if the window is closed successfully

                // When the Paint 3D window remains open because of save confirmation dialog, attempt to close modal dialog
                DismissSaveConfirmDialog();
            }
            catch { }
        }

        public void SetupBrushesPane()
        {
            // Select the Brushes toolbox to have the Brushes Pane sidebar displayed
            session.FindElementByAccessibilityId("Toolbox").FindElementByAccessibilityId("TopBar_ArtTools").Click();
            brushesPane = session.FindElementByAccessibilityId("SidebarWrapper");

            // Set eraser thickness to eraser width in pixel
            brushesPane.FindElementByAccessibilityId("Eraser3d").Click();
            if (brushesPane.FindElementByAccessibilityId("Thickness").Text != eraserWidth)
            {
                brushesPane.FindElementByAccessibilityId("Thickness").SendKeys(Keys.Control + "a" + Keys.Control);
                brushesPane.FindElementByAccessibilityId("Thickness").SendKeys(eraserWidth + Keys.Enter);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            // Ensure that the Pixel Pen is selected
            brushesPane.FindElementByAccessibilityId("PixelPencil3d").Click();

            // Locate the drawing surface
            inkCanvas = session.FindElementByAccessibilityId("InteractorFocusWrapper");

            // Locate the Undo button
            undoButton = session.FindElementByAccessibilityId("UndoIcon");
            //Assert.IsTrue(undoButton.Displayed);
            //Assert.IsFalse(undoButton.Enabled);
        }

        public void DrawCircle()
        {
            // Draw a circle with radius 300 and 40 (x, y) points
            const int radius = 300;
            const int points = 40;

            // Select the Brushes toolbox to have the Brushes Pane sidebar displayed and ensure that Marker is selected
            session.FindElementByAccessibilityId("Toolbox").FindElementByAccessibilityId("TopBar_ArtTools").Click();
            session.FindElementByAccessibilityId("SidebarWrapper").FindElementByAccessibilityId("Marker3d").Click();

            // Locate the drawing surface
            WindowsElement inkCanvas = session.FindElementByAccessibilityId("InteractorFocusWrapper");

            // Draw the circle with a single touch actions
            OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchContact = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
            ActionSequence touchSequence = new ActionSequence(touchContact, 0);
            touchSequence.AddAction(touchContact.CreatePointerMove(inkCanvas, 0, -radius, TimeSpan.Zero));
            touchSequence.AddAction(touchContact.CreatePointerDown(PointerButton.TouchContact));
            for (double angle = 0; angle <= 2 * Math.PI; angle += 2 * Math.PI / points)
            {
                touchSequence.AddAction(touchContact.CreatePointerMove(inkCanvas, (int)(Math.Sin(angle) * radius), -(int)(Math.Cos(angle) * radius), TimeSpan.Zero));
            }
            touchSequence.AddAction(touchContact.CreatePointerUp(PointerButton.TouchContact));
            session.PerformActions(new List<ActionSequence> { touchSequence });

            // Verify that the drawing operations took place
            WindowsElement undoButton = session.FindElementByAccessibilityId("UndoIcon");
            //Assert.IsTrue(undoButton.Displayed);
            //Assert.IsTrue(undoButton.Enabled);
        }

        public void DrawRectangle()
        {
            OpenQA.Selenium.Appium.Interactions.PointerInputDevice penDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Pen);

            // Draw rectangle ABCD (consisting of AB, BC, CD, and DA lines)
            ActionSequence sequence = new ActionSequence(penDevice, 0);
            sequence.AddAction(penDevice.CreatePointerMove(inkCanvas, A.X, A.Y, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
            sequence.AddAction(penDevice.CreatePointerMove(inkCanvas, B.X, B.Y, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerMove(inkCanvas, C.X, C.Y, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerMove(inkCanvas, D.X, D.Y, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerMove(inkCanvas, A.X, A.Y, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
            session.PerformActions(new List<ActionSequence> { sequence });

            // Fill the rectangle ABCD at the middle of the crosshair position (Point E)
            brushesPane.FindElementByAccessibilityId("FillBucket").Click();

            ActionSequence fillSequence = new ActionSequence(penDevice, 0);
            fillSequence.AddAction(penDevice.CreatePointerMove(inkCanvas, E.X, E.Y, TimeSpan.Zero));
            fillSequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
            fillSequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
            session.PerformActions(new List<ActionSequence> { fillSequence });

            // Erase by pressing PenEraser button along Point E X-Axis and Y-Axis to make the crosshair
            ActionSequence eraseSequence = new ActionSequence(penDevice, 0);
            eraseSequence.AddAction(penDevice.CreatePointerMove(inkCanvas, A.X - 5, E.Y, TimeSpan.Zero));
            eraseSequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenEraser));
            eraseSequence.AddAction(penDevice.CreatePointerMove(inkCanvas, B.X + 5, E.Y, TimeSpan.FromSeconds(.5)));
            eraseSequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenEraser));
            eraseSequence.AddAction(penDevice.CreatePointerMove(inkCanvas, E.X, C.Y, TimeSpan.Zero));
            eraseSequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenEraser));
            eraseSequence.AddAction(penDevice.CreatePointerMove(inkCanvas, E.X, B.Y, TimeSpan.FromSeconds(.5)));
            eraseSequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenEraser));
            session.PerformActions(new List<ActionSequence> { eraseSequence });

            // Verify that the drawing operations took place
            Assert.IsTrue(undoButton.Displayed);
            Assert.IsTrue(undoButton.Enabled);
        }

        public void DrawTriangle()
        {

            // Select the Brushes toolbox to have the Brushes Pane sidebar displayed and ensure that Marker is selected
            session.FindElementByAccessibilityId("Toolbox").FindElementByAccessibilityId("TopBar_ArtTools").Click();
            session.FindElementByAccessibilityId("SidebarWrapper").FindElementByAccessibilityId("Marker3d").Click();

            // Locate the drawing surface
            WindowsElement inkCanvas = session.FindElementByAccessibilityId("InteractorFocusWrapper");

            TimeSpan howFast = TimeSpan.FromMilliseconds(300.0);
            OpenQA.Selenium.Appium.Interactions.PointerInputDevice penDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Pen);
            ActionSequence sequence = new ActionSequence(penDevice, 0);

            var halfWidth = inkCanvas.Size.Width / 2;
            var halfHeight = inkCanvas.Size.Height / 2;

            // left base 
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth, halfHeight, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.TouchContact));
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth - 500, halfHeight, howFast));
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));

            // right base 
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth, halfHeight, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.TouchContact));
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth + 500, halfHeight, howFast));
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));

            // left top 
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth - 500, halfHeight, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.TouchContact));
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth, halfHeight - 500, howFast));
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));

            // right top 
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth + 500, halfHeight, TimeSpan.Zero));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.TouchContact));
            sequence.AddAction(penDevice.CreatePointerMove(CoordinateOrigin.Viewport, halfWidth, halfHeight - 500, howFast));
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));

            session.PerformActions(new List<ActionSequence> { sequence });
        }
    }
}
