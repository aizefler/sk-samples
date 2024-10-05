using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace AlexandreIzefler.AI.SK.Demos.Demo1
{
    public class SK_PlanningEAP_Agent_1
    {
        private readonly ILogger<SK_PlanningEAP_Agent_1> _logger;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly string _key;

        public SK_PlanningEAP_Agent_1(ILogger<SK_PlanningEAP_Agent_1> logger, IConfiguration configuration)
        {
            _logger = logger;
            _model = configuration["AZURE_OPENAI_SERVICES_MODEL"]!;
            _endpoint = configuration["AZURE_OPENAI_SERVICES_ENDPOINT"]!;
            _key = configuration["AZURE_OPENAI_SERVICES_KEY"]!;
        }

        [Function("SK_PlanningEAP_Agent_1_Http")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function iniciando o processamento.");

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(_model, _endpoint, _key);
            var kernel = builder.Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory("Como posso ajudá-lo?");

            string? userInput = "Olá quero criar o planejamento de minha obra, pode me ajudar?";
            history.AddUserMessage(userInput);

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: kernel);

            return new OkObjectResult(result.ToString());
        }
    }
}
