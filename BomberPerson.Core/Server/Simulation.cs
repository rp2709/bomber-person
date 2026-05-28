using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class Simulation(State.State state)
{
    public State.State ProcessMessage(IMessage message)
    {
        throw  new System.NotImplementedException();
    }
}