using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public interface ISimulationMessage
{
    IMessage Process(State.State state);
}