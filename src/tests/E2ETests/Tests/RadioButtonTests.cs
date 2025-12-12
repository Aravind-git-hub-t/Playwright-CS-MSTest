using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using E2ETests.Pages;
using E2ETests.Helpers;


namespace E2ETests.Tests;

[TestClass]
public class RadioButtonTests : PlaywrightBase
    {
        [TestMethod]
        public async Task YesRadio_ShowsCorrectMessage()
        {
            var pageObj = new RadioButtonPage(Page);
            await pageObj.NavigateAsync();
            await pageObj.Button("yes");

            string result = await pageObj.GetResultValueAsync();
            Assert.AreEqual("Yes", result);
        }

        [TestMethod]
        public async Task ImpressiveRadio_ShowsCorrectMessage()
        {
            var pageObj = new RadioButtonPage(Page);
            await pageObj.NavigateAsync();
            await pageObj.Button("impressive");

            string result = await pageObj.GetResultValueAsync();
            Assert.AreEqual("Impressive", result);
        }

        [TestMethod]
        public async Task NoRadio_ShouldBeDisabled()
        {
            var pageObj = new RadioButtonPage(Page);
            await pageObj.NavigateAsync();
            bool state = await pageObj.checkIfDisabled();

            Assert.IsFalse(state, "'No' radio button must be disabled.");
        }
    }
