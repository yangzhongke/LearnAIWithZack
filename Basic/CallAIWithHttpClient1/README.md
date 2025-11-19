# CallAIWithHttpClient1

A simple example demonstrating how to call OpenAI-compatible LLM APIs using HttpClient with strong-typed models for JSON deserialization.

## Features

- Uses `HttpClient` for making HTTP requests to OpenAI-compatible APIs
- Strong-typed C# models for JSON serialization/deserialization
- Proper `[JsonPropertyName]` attributes for snake_case to PascalCase mapping
- Error handling for HTTP requests
- Displays AI responses and token usage information

## Prerequisites

- .NET 9.0 SDK
- An OpenAI API key or compatible API endpoint

## Setup

1. Set your OpenAI API key as an environment variable:

   **Windows (PowerShell):**
   ```powershell
   $env:OPENAI_API_KEY="your-api-key-here"
   ```

   **Windows (Command Prompt):**
   ```cmd
   set OPENAI_API_KEY=your-api-key-here
   ```

   **Linux/Mac:**
   ```bash
   export OPENAI_API_KEY="your-api-key-here"
   ```

2. Build and run the project:
   ```bash
   dotnet run
   ```

## Key Implementation Details

### HttpClient Configuration

The application configures `HttpClient` with:
- Base URL: `https://api.openai.com/v1`
- Bearer token authentication using the API key from environment variable
- Relative endpoint path: `chat/completions` (without leading slash)

**Important:** When using `HttpClient.BaseAddress`, relative paths should NOT start with a slash. Using `chat/completions` instead of `/chat/completions` ensures the URL correctly resolves to `https://api.openai.com/v1/chat/completions`.

### Strong-Typed Models

The application uses the following models for JSON deserialization:

- `ChatCompletionResponse` - Main response wrapper
- `Choice` - Individual completion choices
- `Message` - Message content and role
- `Usage` - Token usage statistics

All models use `[JsonPropertyName]` attributes to map between C# PascalCase property names and JSON snake_case field names.

## Example Output

```
AI Response:
Hello! I'm doing well, thank you for asking. How can I assist you today?

Token Usage:
  Prompt tokens: 13
  Completion tokens: 18
  Total tokens: 31
```

## Common Issues

### 404 Not Found Error

If you encounter a 404 error, ensure:
1. The endpoint path does not start with a slash when using `BaseAddress`
2. Your API key is valid
3. The base URL is correct for your OpenAI-compatible service

### Missing API Key

If you see "Error: OPENAI_API_KEY environment variable is not set", make sure you've set the environment variable before running the application.

## Alternative API Endpoints

To use with other OpenAI-compatible services (like Azure OpenAI, Ollama, etc.), modify the `baseUrl` variable:

```csharp
// For Azure OpenAI
var baseUrl = "https://YOUR-RESOURCE.openai.azure.com/openai/deployments/YOUR-DEPLOYMENT";

// For Ollama (local)
var baseUrl = "http://127.0.0.1:11434/v1";
```
