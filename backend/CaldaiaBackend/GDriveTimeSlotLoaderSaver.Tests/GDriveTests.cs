using System;
using CaldaiaBackend.Infrastructure;
using NUnit.Framework;
using Services.TimeSlotLoaderSaver.GDrive;

namespace GDriveTimeSlotLoaderSaver.Tests
{
    [TestFixture]
    public class GDriveTests
    {
        private static GDriveTimeSlotLoaderSaver<string> ABufferOfStrings => new GDriveTimeSlotLoaderSaver<string>("gdriveTests.json", null);

        [Test]
        public void Should_Be_Possible_To_Authenticate_The_Api()
        {
            var sut = ABufferOfStrings;
            Assert.IsInstanceOf<GDriveTimeSlotLoaderSaver<string>>(sut);
        }

        [Test]
        public void Should_Be_Possible_To_Load_From_Drive()
        {
            var sut = ABufferOfStrings;
            Assert.DoesNotThrow(() =>
            {
                var tsb = sut.Load();
            });
        }

        [Test]
        public void Should_Be_Possible_To_Save_To_Drive()
        {
            var sut = ABufferOfStrings;
            var tsb = new CircularTimeSlotBuffer<string>(12, TimeSpan.FromSeconds(900),
                new DateTime(2018, 12, 29, 18, 30, 00));
            Assert.DoesNotThrow(() =>
            {
                sut.Save(tsb);
            });
        }
    }
}
