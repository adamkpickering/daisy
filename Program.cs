// Import packages
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using System.ClientModel;

var modelId = Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ?? "deepseek/deepseek-v4.1-flash";
var endpoint = new Uri("https://openrouter.ai/api/v1");
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
             ?? throw new InvalidOperationException("OPENROUTER_API_KEY not set");

var client = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = endpoint });

var service = new OpenAIChatCompletionService(modelId, client);
var history = new ChatHistory();
history.AddSystemMessage("You are an expert coding assistant.");

string? userInput;
while (true) {
    Console.Write("User > ");
    userInput = Console.ReadLine();
    if (userInput is null) {
        break;
    }
    history.AddUserMessage(userInput);

    var result = await service.GetChatMessageContentAsync(history);
    Console.WriteLine("Assistant > " + result);
    history.AddMessage(result.Role, result.Content ?? string.Empty);
}
