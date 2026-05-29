using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public interface ISimulationMessage
{
    void Process(State.State state);
}