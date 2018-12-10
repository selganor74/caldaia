using System.Linq;
using System.Threading;
using NUnit.Framework;

using CaldaiaBackend;

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
        public void MustBePossibleToSendACommand()
        {
            using (var sut = CreateSerial())
            {
                sut.Start();
                sut.SendGetCommand();
                Thread.Sleep(10000);
                sut.Dispose();
            }
        }

        private static CaldaiaControllerViaArduino CreateSerial()
        {
            var serial = new SerialEnumerator().GetSerialPorts().First();
            var sut = new CaldaiaControllerViaArduino(serial);
            return sut;
        }
    }
}
