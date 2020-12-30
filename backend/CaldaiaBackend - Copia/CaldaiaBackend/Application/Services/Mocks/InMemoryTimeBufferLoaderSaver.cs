using CaldaiaBackend.Infrastructure;

namespace CaldaiaBackend.Application.Services.Mocks
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
