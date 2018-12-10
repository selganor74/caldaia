using NUnit.Framework;

using CaldaiaBackend;
using System.Linq;

namespace CaldaiaBackendTests
{
    [TestFixture]
    public class SerialPortEnumerator_Tests
    {
        [Test]
        public void CheckForExceptions()
        {
            var sut = new SerialEnumerator();
            Assert.DoesNotThrow(() => sut.GetSerialPorts() );
        }

        [Test]
        public void MakeSureItReturnsAnIEnumerable()
        {
            var sut = new SerialEnumerator();
            var returned = sut.GetSerialPorts();
            Assert.IsNotNull(returned);
            Assert.IsTrue(returned.Count() >= 0);
        }

    }
}
