using System;
using System.Linq;
using CaldaiaBackend.Infrastructure;
using NUnit.Framework;

namespace CaldaiaBackendTests
{
    public class SomeComplexType
    {
        public string AStringProperty { get; set; }
        public int AnIntProperty { get; set; }
        public float AFloatProperty { get; set; }
        public double ADoubleProperty { get; set; }

        public static SomeComplexType ComplexTypeBuilder()
        {
            return new SomeComplexType
            {
                ADoubleProperty = 10.001d,
                AFloatProperty = 10.2F,
                AnIntProperty = 100,
                AStringProperty = "Ciao"
            };
        }
    }

    [TestFixture]
    public class CircularTimeBuffer_Tests
    {
        [Test]
        public void It_Must_Be_Possibile_To_Instantiate_A_CircularTimeSlot()
        {
            var ctb = new CircularTimeSlotBuffer<string>();
            Assert.IsInstanceOf<CircularTimeSlotBuffer<string>>(ctb);
        }

        [Test]
        public void It_Must_Be_Possible_To_Add_A_New_TimeSlot()
        {
            var numOfSlots = 12;
            var slotSize = TimeSpan.FromSeconds(10);
            var ctb = new CircularTimeSlotBuffer<string>(numOfSlots, slotSize, new DateTime(2018, 12, 25, 10, 00, 00));
            var removed = ctb.UpdateOrCreateContentAtReference(new DateTime(2018,12,25,10,00,07), "TEST");
            Assert.AreEqual(numOfSlots, ctb.GetBuffer().Count());
            Assert.AreEqual(1, removed.Count());
        }

        [Test]
        public void Serialization_Should_Work_As_Expected_With_A_Primitive_Type()
        {
            var numOfSlots = 12;
            var slotSize = TimeSpan.FromSeconds(10);
            var startingReference = new DateTime(2018, 12, 25, 10, 00, 00);
            var ctb = new CircularTimeSlotBuffer<string>(numOfSlots, slotSize, startingReference);
            var reference = startingReference;
            for (int i = 0; i < numOfSlots; i++)
            {
                reference += slotSize;
                ctb.UpdateOrCreateContentAtReference(reference, "TEST " + i);
            }

            var output = ctb.AsJson();
            Assert.IsInstanceOf<string>(output);
            var deserialized = CircularTimeSlotBuffer<string>.FromJson(output);
            var secondOutput = deserialized.AsJson();
            Assert.AreEqual(secondOutput, output);
        }

        [Test]
        public void Serialization_Should_Work_As_Expected_With_A_Complex_Type()
        {
            var numOfSlots = 12;
            var slotSize = TimeSpan.FromSeconds(10);
            var startingReference = new DateTime(2018, 12, 25, 10, 00, 00);
            var ctb = new CircularTimeSlotBuffer<SomeComplexType>(numOfSlots, slotSize, startingReference);
            var reference = startingReference;
            for (int i = 0; i < numOfSlots; i++)
            {
                reference += slotSize;
                ctb.UpdateOrCreateContentAtReference(reference, SomeComplexType.ComplexTypeBuilder());
            }

            var output = ctb.AsJson();
            Assert.IsInstanceOf<string>(output);
            var deserialized = CircularTimeSlotBuffer<SomeComplexType>.FromJson(output);
            var secondOutput = deserialized.AsJson();
            Assert.AreEqual(secondOutput, output);

        }
    }
}
