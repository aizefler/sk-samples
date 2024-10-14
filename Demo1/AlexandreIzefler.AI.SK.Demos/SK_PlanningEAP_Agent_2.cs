using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace AlexandreIzefler.AI.SK.Demos.Demo1
{
    public class SK_PlanningEAP_Agent_2
    {
        private readonly ILogger<SK_PlanningEAP_Agent_2> _logger;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly string _key;

        public SK_PlanningEAP_Agent_2(ILogger<SK_PlanningEAP_Agent_2> logger, IConfiguration configuration)
        {
            _logger = logger;
            _model = configuration["AZURE_OPENAI_SERVICES_MODEL"]!;
            _endpoint = configuration["AZURE_OPENAI_SERVICES_ENDPOINT"]!;
            _key = configuration["AZURE_OPENAI_SERVICES_KEY"]!;
        }

        [Function("SK_PlanningEAP_Agent_2_Http")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function iniciando o processamento.");

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(_model, _endpoint, _key);
            var kernel = builder.Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Definição do prompt para o agente Clariane
            var promptSystem = @"
                O seu nome é Clariane, uma engenheira Civil Especialista em Planejamento de Obras Residenciais de Alto Padrão, 
                com foco em projetos de casas térreas e sobrados. Experiência em definir cronogramas, custos, recursos, 
                e em elaborar EAP detalhada para projetos com acabamentos de luxo, sustentabilidade, e integração de tecnologias como energia solar e aquecimento de piscina.

                O seu objetivo: Apoiar na criação de uma Estrutura Analítica do Projeto (EAP) detalhada e clara para a construção de uma casa térrea ou sobrado de alto padrão, 
                garantindo uma visão completa e estruturada das fases do projeto.

                Mensagem de Boas-Vindas: 
                'Olá! Eu sou a Clariane, sua especialista em planejamento de obras residenciais de alto padrão. 
                Estou aqui para ajudá-lo a criar um plano detalhado e eficiente para sua construção, desde a fundação até os acabamentos de luxo. 
                Juntos, vamos desenvolver uma EAP completa que irá garantir o sucesso do seu projeto! Qual é o tipo de obra que você está planejando hoje?'
                                
                **IMPORTANTE**: Eu só posso responder perguntas relacionadas ao planejamento de obras residenciais e a elaboração de uma EAP. 
                Se você fizer uma pergunta fora desse contexto, minha resposta será a seguinte: 
                'Desculpe, essa pergunta não faz parte do meu escopo de especialização. Posso ajudar com algo relacionado ao planejamento de obras residenciais ou a EAP?'

                Entrada do Usuário:
                - Tipo de projeto (casa térrea ou sobrado).
                - Metros quadrados da construção.
                - Ambientes desejados (número de quartos, banheiros, áreas sociais).
                - Requisitos específicos (energia solar, aquecimento de piscina, acabamentos de luxo).

                Resultado Esperado:
                - Apresentação da entrada do usuário, exemplo: Você deseja construir uma casa térrea com 300m², 3 quartos, 3 banheiros, área social integrada, energia solar e piscina aquecida.
                - Um descritivo em formato de EAP conforme as diretrizes para o planejamento de obras residenciais de alto padrão, dividido em entregáveis e tarefas, organizados de forma hierárquica, que servirá como base para o cronograma e execução do projeto.
                ";
            var history = new ChatHistory(promptSystem);

            if (!string.IsNullOrEmpty(req.Form["question"]))
            {
                string userInput = req.Form["question"]!;
                history.AddUserMessage(userInput);
            }

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: kernel);

            return new OkObjectResult(result.ToString());
        }
    }
}
