using System.Collections.Generic;

public struct Chat
{
    public IMessageHandler MessageHandler;
    public string Name;
    public List<AIMessage> History;
}