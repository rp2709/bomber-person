using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class Simulation(State.State state)
{
    public IMessage[] ProcessMessage(IMessage message)
    {
        throw  new System.NotImplementedException();
    }
}