using System.Threading.Tasks;
using JetBrains.Annotations;

public interface IMessageHandler
{
    string Model { get; set; }
    
    /// <summary>
    /// Sends a message to the AI and returns the response.
    /// </summary>
    /// <param name="history">The chat history to complete.</param>
    /// <returns>The AI's response.</returns>
    Task<string> GetChatCompletion(AIMessage[] history);
}