using application.infrastructure;
namespace application.services;

// Provides reading from the Rotex Accumulator
public interface IRotexReader : IStartable
{
    object GetRotexConfig();
}
