// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Populate values for OpenRouter (https://openrouter.ai/)
var modelId = Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ?? "deepseek/deepseek-v4.1-flash";
var endpoint = "https://openrouter.ai/api/v1";
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
             ?? throw new InvalidOperationException("OPENROUTER_API_KEY not set");

// Create a kernel backed by OpenRouter's OpenAI-compatible endpoint
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(modelId, new Uri(endpoint), apiKey);

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Enable planning
var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
