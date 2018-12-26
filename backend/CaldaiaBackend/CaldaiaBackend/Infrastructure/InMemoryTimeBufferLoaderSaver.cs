using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaldaiaBackend.Infrastructure
{
    public class InMemoryTimeBufferLoaderSaver<T> : ITimeSlotBufferLoaderSaver<T>
    {
        private CircularTimeSlotBuffer<T> buffer;
        public CircularTimeSlotBuffer<T> Load()
        {
            return buffer;
        }

        public void Save(CircularTimeSlotBuffer<T> toSave)
        {
            buffer = toSave;
        }
    }
}
