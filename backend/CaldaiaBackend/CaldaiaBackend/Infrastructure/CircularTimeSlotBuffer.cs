using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CaldaiaBackend.Infrastructure
{
    [Serializable]
    public class TimeSlot<T>
    {
        public DateTime SlotEnd { get; private set; }
        public TimeSpan SlotSize { get; private set; }
        public DateTime SlotStart => SlotEnd - SlotSize;
        public T Content { get; set; }

        public TimeSlot(DateTime slotEnd, TimeSpan slotSize, T content = default(T))
        {
            this.SlotEnd = slotEnd;
            this.SlotSize = slotSize;
            this.Content = content;
        }

    }

    [Serializable]
    public class CircularTimeSlotBuffer<T>
    {
        private readonly List<TimeSlot<T>> _buffer = new List<TimeSlot<T>>();
        private DateTime lastSlotEndTime;
        private readonly TimeSpan slotSize;
        private readonly object _bufferLock = new object();

        public CircularTimeSlotBuffer()
        {

        }

        public CircularTimeSlotBuffer(IEnumerable<TimeSlot<T>> timeSlots)
        {
            this.slotSize = timeSlots.First().SlotSize;
            this.lastSlotEndTime = timeSlots.Max(t => t.SlotEnd);
            this._buffer = timeSlots.ToList();
        }

        public CircularTimeSlotBuffer(
            int numberOfSlots,
            TimeSpan slotSize,
            DateTime lastSlotEndTime
            )
        {
            this.lastSlotEndTime = lastSlotEndTime.ToUniversalTime();
            this.slotSize = slotSize;
            DateTime currSlotEnd = this.lastSlotEndTime;
            for (var i = 0; i < numberOfSlots; i++)
            {
                var slot = new TimeSlot<T>(currSlotEnd, slotSize);
                _buffer.Add(slot);
                currSlotEnd -= slotSize;
            }

            _buffer.Reverse();
        }

        private void SetContentAtReference(DateTime referenceDate, T content = default(T))
        {
            var utcReference = referenceDate.ToUniversalTime();
            var slot = _buffer.FirstOrDefault(e => e.SlotStart < utcReference && utcReference <= e.SlotEnd);
            if (slot == null) throw new IndexOutOfRangeException(
                $"Unable to find a Slot for specified reference date. [{referenceDate:0}/UTC:{utcReference:o}]");

            slot.Content = content;
        }

        /// <summary>
        /// Returns the value stored in a particular Slot
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>dafault(T) if no slot was found for reference date</returns>
        public T GetContentAtReference(DateTime referenceDate)
        {
            var utcReference = referenceDate.ToUniversalTime();
            var slot = _buffer.FirstOrDefault(e => e.SlotStart < utcReference && utcReference <= e.SlotEnd);
            return slot == null ? default(T) : slot.Content;
        }

        /// <summary>
        /// Updates and existing slot content, or creates as neede up to the reference specified.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="content"></param>
        /// <exception cref="IndexOutOfRangeException">If the requested reference is "previous" to the oldest Slot in the collection.</exception> 
        /// <returns>An IEnumerable of all the removed elements"/></returns>
        public IEnumerable<T> UpdateOrCreateContentAtReference(DateTime reference, T content = default(T))
        {
            lock (_bufferLock)
            {
                var removed = new List<T>();
                var utcReference = reference.ToUniversalTime();
                while (lastSlotEndTime < utcReference)
                {
                    var aRemoved = AddNextSlot();
                    removed.Add(aRemoved);
                }

                SetContentAtReference(utcReference, content);
                return removed;
            }
        }

        private T AddNextSlot(T content = default(T))
        {
            lastSlotEndTime += slotSize;
            var slot = new TimeSlot<T>(lastSlotEndTime, slotSize, content);
            _buffer.Add(slot);
            var toReturn = _buffer[0] == null ? default(T) : _buffer[0].Content;
            _buffer.RemoveAt(0);
            return toReturn;
        }

        public IEnumerable<TimeSlot<T>> GetBuffer()
        {
            lock (_bufferLock)
            {
                return _buffer.ToList();
            }
        }

        public string AsJson()
        {
            lock (_bufferLock)
            {
                return JsonConvert.SerializeObject(_buffer);
            }
        }

        public static CircularTimeSlotBuffer<T> FromJson(string jsonSource)
        {
            var timeSlots = JsonConvert.DeserializeObject<IEnumerable<TimeSlot<T>>>(jsonSource);
            return new CircularTimeSlotBuffer<T>(timeSlots);
        }
    }
}
