using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;

namespace JSErrorCollector.Tests
{
    /// <summary>
    /// Tests with Firebug ensuring that we get JS errors & console content.
    /// 
    /// @author Marc Guillemot, ported by Joel Sanderson
    /// </summary>
    [TestClass]
    public class FirebugConsoleTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FirebugConsole_Simple()
        {
            string url = GetResource("withConsoleOutput.html");
            JavaScriptError errorSimpleHtml = new JavaScriptError("TypeError: null has no properties", url, 8, "before JS error");
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorSimpleHtml };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(url);
                IList<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        private void AssertErrorsEqual(IEnumerable<JavaScriptError> expectedErrors, IEnumerable<JavaScriptError> actualErrors)
        {
            string expected = "";
            foreach (JavaScriptError err in expectedErrors)
            {
                expected += err.ErrorMessage + " (line " + err.LineNumber + ")";
            }

            string actual = "";
            foreach (JavaScriptError err in actualErrors)
            {
                actual += err.ErrorMessage + " (line " + err.LineNumber + ")";
            }

            Assert.AreEqual(expected, actual);
        }

        private IWebDriver BuildFFDriver()
        {
            string env = "remote";
            IWebDriver driver;
            if (env == "local-install")
            {
                FirefoxProfile ffProfile = new FirefoxProfile();
                JavaScriptError.AddExtension(ffProfile);
                driver = new FirefoxDriver(ffProfile);
            }
            else if (env == "local-profile")
            {
                var profileManager = new FirefoxProfileManager();
                FirefoxProfile ffProfile = profileManager.GetProfile("SELENIUM");
                driver = new FirefoxDriver(ffProfile);
            }
            else // if (env == "remote")
            {
                DesiredCapabilities capability = DesiredCapabilities.Firefox();
                driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), capability);
            }
            return driver;
        }

        private String GetResource(String fileName)
        {
            string resourceContent = TestResources.ResourceManager.GetString(Path.GetFileNameWithoutExtension(fileName));

            string tempResourcePath = Path.Combine(TestContext.DeploymentDirectory, fileName);
            using (StreamWriter sw = new StreamWriter(tempResourcePath))
            {
                sw.Write(resourceContent);
            }

            string resourceUrl = tempResourcePath.Replace(Path.DirectorySeparatorChar, '/');
            resourceUrl = Uri.EscapeDataString(resourceUrl)
                .Replace("%2F", "/")
                .Replace("%3A", ":");
            resourceUrl = "file://" + resourceUrl;
            return resourceUrl;
        }

        private string SaveBinaryResource(String fileName, byte[] data)
        {
            string tempResourcePath = Path.Combine(TestContext.DeploymentDirectory, fileName);
            using (FileStream sw = new FileStream(tempResourcePath, FileMode.Create, FileAccess.Write))
            {
                sw.Write(data, 0, data.Length);
            }

            return tempResourcePath;
        }
    }
}
