using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ArduinoCommunication;
using NUnit.Framework;

using CaldaiaBackend;
using CaldaiaBackend.Application.DataModels;

namespace CaldaiaBackendTests
{
    [TestFixture]
    public class CaldaiaController_Tests
    {
        [Test]
        public void MustBePossibleToInstantiateOne()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var sut = CreateSerial())
                {
                    sut.Start();
                    sut.Dispose();
                }
            });
        }

        [Test]
        public void MustBePossibleToSendA_GET_Command()
        {
            using (var sut = CreateSerial())
            {
                var locker = true;
                var timeout = 10000;
                var sw = new Stopwatch();
                sw.Start();

                sut.RegisterObserver((data) =>
                {
                    Assert.IsInstanceOf<DataFromArduino>(data);
                    locker = false;
                });

                sut.Start();
                sut.PullOutData();
                while (locker && sw.ElapsedMilliseconds <= timeout)
                {
                    Thread.Sleep(100);
                }

                sw.Stop();
                Assert.IsFalse(locker);
            }
        }

        [Test]
        public void MustBePossibleToSendA_GET_RA_Command()
        {
            using (var sut = CreateSerial())
            {
                var locker = true;
                var timeout = 10000;
                var sw = new Stopwatch();
                sw.Start();

                sut.RegisterObserver((data) =>
                {
                    Assert.IsInstanceOf<DataFromArduino>(data);
                    locker = false;
                });

                sut.Start();
                sut.SendGetAndResetAccumulatorsCommand();
                while (locker && sw.ElapsedMilliseconds <= timeout)
                {
                    Thread.Sleep(100);
                }

                sw.Stop();
                Assert.IsFalse(locker);
            }
        }

        [Test]
        public void MustBePossibleToSendA_GET_RS_Command()
        {
            using (var sut = CreateSerial())
            {
                var locker = true;
                var timeout = 10000;
                var sw = new Stopwatch();
                sw.Start();

                sut.RegisterSettingsObserver((data) =>
                {
                    Assert.IsInstanceOf<SettingsFromArduino>(data);
                    locker = false;
                });

                sut.Start();
                sut.PullOutSettings();
                while (locker && sw.ElapsedMilliseconds <= timeout)
                {
                    Thread.Sleep(100);
                }

                sw.Stop();
                Assert.IsFalse(locker);
            }
        }

        private static CaldaiaControllerViaArduino CreateSerial()
        {
            var serial = new SerialEnumerator().GetSerialPorts().First();
            var sut = new CaldaiaControllerViaArduino(serial, null);
            return sut;
        }
    }
}
