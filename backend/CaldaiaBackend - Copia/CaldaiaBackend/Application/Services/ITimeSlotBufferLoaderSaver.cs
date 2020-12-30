using CaldaiaBackend.Infrastructure;

namespace CaldaiaBackend.Application.Services
{
    public interface ITimeSlotBufferLoaderSaver<T>
    {
        CircularTimeSlotBuffer<T> Load();

        void Save(CircularTimeSlotBuffer<T> toSave);
    }
}
