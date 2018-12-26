using System.Threading.Tasks;

namespace CaldaiaBackend.Infrastructure
{
    public interface ITimeSlotBufferLoaderSaver<T>
    {
        CircularTimeSlotBuffer<T> Load();

        void Save(CircularTimeSlotBuffer<T> toSave);
    }
}
