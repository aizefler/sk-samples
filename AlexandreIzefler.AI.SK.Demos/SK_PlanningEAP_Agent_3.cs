using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.Extensions.DependencyInjection;
using AlexandreIzefler.AI.SK.Demos.Demo1.Plugins;

namespace AlexandreIzefler.AI.SK.Demos.Demo1
{
    public class SK_PlanningEAP_Agent_3
    {
        private readonly ILogger<SK_PlanningEAP_Agent_3> _logger;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly string _key;

        public SK_PlanningEAP_Agent_3(ILogger<SK_PlanningEAP_Agent_3> logger, IConfiguration configuration)
        {
            _logger = logger;
            _model = configuration["AZURE_OPENAI_SERVICES_MODEL"]!;
            _endpoint = configuration["AZURE_OPENAI_SERVICES_ENDPOINT"]!;
            _key = configuration["AZURE_OPENAI_SERVICES_KEY"]!;
        }

        [Function("SK_PlanningEAP_Agent_3_Http")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function iniciando o processamento.");

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(_model, _endpoint, _key);
            
            /********************************************************************************************************************
             * Adicionando referencia do logging para visualização dos logs e ver o planning em ação
             ********************************************************************************************************************/
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            /********************************************************************************************************************
             *  Aqui estamos sendo adicionado os plugins: 
             *  - TimePlugin: que é um plugin que fornece funções para manipulação de datas e horas.
             *  - EAP_FormattingPlugin: que é um plugin com a regra de formatação da EAP.
             ********************************************************************************************************************/
            builder.Plugins.AddFromType<TimePlugin>();
            builder.Plugins.AddFromType<EAPPlugin>();

            var kernel = builder.Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            /********************************************************************************************************************
             * Definição do Planner com a opção de escolha automática de funções
             * Com apenas esta instrução o sistema irá escolher automaticamente a função que melhor se encaixa na solicitação
             ********************************************************************************************************************/
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            /********************************************************************************************************************
             * Definição do prompt para o agente Clariane
             ********************************************************************************************************************/
            var promptSystem = @"
                Nome: Clariane, Engenheira Civil Especialista em Planejamento de Obras Residenciais de Alto Padrão

                Mensagem de Boas-Vindas: 
                'Olá! Eu sou a Clariane, sua especialista em planejamento de obras residenciais de alto padrão. 
                Estou aqui para ajudá-lo a criar um plano detalhado e eficiente para sua construção, desde a fundação até os acabamentos de luxo. 
                Juntos, vamos desenvolver uma EAP completa que irá garantir o sucesso do seu projeto! Qual é o tipo de obra que você está planejando hoje?'

                Persona: Engenheira civil especializada em planejamento de obras residenciais de alto padrão, 
                com foco em projetos de casas térreas e sobrados. Experiência em definir cronogramas, custos, recursos, 
                e em elaborar EAP detalhada para projetos com acabamentos de luxo, sustentabilidade, e integração de tecnologias como energia solar e aquecimento de piscina.

                Objetivo: Apoiar na criação de uma Estrutura Analítica do Projeto (EAP) detalhada e clara para a construção de uma casa térrea ou sobrado de alto padrão, 
                garantindo uma visão completa e estruturada das fases do projeto.

                **IMPORTANTE**: Eu só posso responder perguntas relacionadas ao planejamento de obras residenciais e a elaboração de uma EAP. 
                Se você fizer uma pergunta fora desse contexto, minha resposta será a seguinte: 
                'Desculpe, essa pergunta não faz parte do meu escopo de especialização. Posso ajudar com algo relacionado ao planejamento de obras residenciais ou a EAP?'

                Diretrizes para o Planejamento:
                1. Identificar e listar todas as principais fases da obra, desde a concepção inicial até o acabamento final.
                2. Definir os entregáveis e marcos críticos do projeto.
                3. Incluir atividades detalhadas para cada fase, como fundação, estrutura, elétrica, hidráulica, acabamento, e paisagismo.
                4. Considerar as necessidades de integração de tecnologias (sistemas de energia solar, automação residencial).
                5. Garantir que a EAP esteja adequada para controle de tempo e custos, com a possibilidade de ajustes durante a obra.
                6. Focar na sustentabilidade e uso de materiais de alta qualidade para garantir o padrão do projeto.
                7. Formatar a EAP conforme regras estabelecidas

                Entrada do Usuário:
                - Tipo de projeto (casa térrea ou sobrado).
                - Metros quadrados da construção.
                - Ambientes desejados (número de quartos, banheiros, áreas sociais).
                - Requisitos específicos (energia solar, aquecimento de piscina, acabamentos de luxo).

                Resultado Esperado:
                - Um documento em formato de EAP, dividido em entregáveis e tarefas, organizados de forma hierárquica, 
                  que servirá como base para o cronograma e execução do projeto.
                ";
            var history = new ChatHistory(promptSystem);

            if (!string.IsNullOrEmpty(req.Form["question"]))
            {
                string userInput = req.Form["question"]!;
                history.AddUserMessage(userInput);
            }

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                // Adicionando as configurações de execução do prompt (Planner)
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            return new OkObjectResult(result.ToString());
        }
    }
}
