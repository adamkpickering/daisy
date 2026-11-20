// Import packages
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

var modelId = Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ?? "deepseek/deepseek-v4.1-flash";
var endpoint = new Uri("https://openrouter.ai/api/v1");
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
             ?? throw new InvalidOperationException("OPENROUTER_API_KEY not set");

OpenAIClient client = new(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = endpoint });

ChatClient chatClient = client.GetChatClient(modelId);

var history = new List<ChatMessage> {
	new SystemChatMessage("You are a helpful assistant.")
};

ChatTool readFileTool = ChatTool.CreateFunctionTool(
	functionName: "read",
	functionDescription: "Read a text file.",
	functionParameters: BinaryData.FromString("""
	{
		"type": "object",
		"properties": {
			"path": {
				"type": "string",
				"description": "Path to the file (relative or absolute)."
			}
		},
		"required": ["path"]
	}
	""")
);

var options = new ChatCompletionOptions {
	Tools = { readFileTool }
};

string? userInput;
while (true) {
	Console.Write("User > ");
	userInput = Console.ReadLine();
	if (userInput is null) {
		break;
	}
	history.Add(new UserChatMessage(userInput));

	while (true) {
		ChatCompletion c = chatClient.CompleteChat(history, options);
		history.Add(new AssistantChatMessage(c));

		if (c.ToolCalls.Count == 0) {
			Console.WriteLine(c.Content[0].Text);
			break;
		}

		foreach (var call in c.ToolCalls) {
			Console.WriteLine($"Tool > {call.FunctionName} {call.FunctionArguments}");
			string output = DispatchToolCall(call);
			history.Add(new ToolChatMessage(call.Id, output));
		}
	}
}

string DispatchToolCall(ChatToolCall call) => call.FunctionName switch {
	"read" => readFile(call),
	_ => throw new InvalidOperationException($"Unexpected function name \"{call.FunctionName}\""),
};

string readFile(ChatToolCall call) {
	string? path = call.FunctionArguments
		.ToObjectFromJson<Dictionary<string, JsonElement>>()?["path"]
		.GetString()
		?? throw new InvalidOperationException("Could not get path out of read tool call args.");
	return File.ReadAllText(path);
}
