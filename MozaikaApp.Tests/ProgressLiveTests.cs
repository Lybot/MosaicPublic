// <copyright file="ProgressLiveTests.cs" company="FreezeMyTime">Copyright ©  2019</copyright>
using System;
using MozaikaApp.Models;
using NUnit.Framework;

namespace MozaikaApp.Models.Tests
{
    /// <summary>Этот класс содержит параметризованные модульные тесты для Functions</summary>
    [TestFixture]
    public partial class ProgressLiveTests
    {
        [Test]
        public void ConstructorTest()
        {
            var model = new ProgressLiveModel();
            Assert.IsTrue(model.LiveCanvas.Count!=0);
        }
    }
}
