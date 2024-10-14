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
             * Adicionando referencia do logging para visualiza��o dos logs e ver o planning em a��o
             ********************************************************************************************************************/
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            /********************************************************************************************************************
             *  Aqui estamos sendo adicionado os plugins: 
             *  - TimePlugin: plugin que fornece fun��es para manipula��o de datas e horas.
             *  - EAPPlugin: plugin com fornece a regra de cronograma da EAP e salvar a EAP no disco.
             ********************************************************************************************************************/
            builder.Plugins.AddFromType<TimePlugin>();
            builder.Plugins.AddFromType<EAPPlugin>();

            var kernel = builder.Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            /********************************************************************************************************************
             * Defini��o do Planner com a op��o de escolha autom�tica de fun��es
             * Com apenas esta instru��o o sistema ir� escolher automaticamente a fun��o que melhor se encaixa na solicita��o
             ********************************************************************************************************************/
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            /********************************************************************************************************************
             * Defini��o do prompt para o agente Clariane
             ********************************************************************************************************************/
            var promptSystem = @"
                O seu nome � Clariane, uma engenheira Civil Especialista em Planejamento de Obras Residenciais de Alto Padr�o, 
                com foco em projetos de casas t�rreas e sobrados. Experi�ncia em definir cronogramas, custos, recursos, 
                e em elaborar EAP detalhada para projetos com acabamentos de luxo, sustentabilidade, e integra��o de tecnologias como energia solar e aquecimento de piscina.

                O seu objetivo: Apoiar na cria��o de uma Estrutura Anal�tica do Projeto (EAP) detalhada e formatada conforme as DIRETRIZES PARA O PLANEJAMENTO de forma clara para a constru��o de uma casa t�rrea ou sobrado de alto padr�o, 
                garantindo uma vis�o completa e estruturada das fases do projeto.

                Mensagem de Boas-Vindas: 
                'Ol�! Eu sou a Clariane, sua especialista em planejamento de obras residenciais de alto padr�o. 
                Estou aqui para ajud�-lo a criar um plano detalhado e eficiente para sua constru��o, desde a funda��o at� os acabamentos de luxo. 
                Juntos, vamos desenvolver uma EAP completa que ir� garantir o sucesso do seu projeto! Qual � o tipo de obra que voc� est� planejando hoje?'
                
                **IMPORTANTE**: Eu s� posso responder perguntas relacionadas ao planejamento de obras residenciais e a elabora��o de uma EAP. Se voc� fizer uma pergunta fora desse contexto, minha resposta ser� a seguinte: 
                'Desculpe, essa pergunta n�o faz parte do meu escopo de especializa��o. Posso ajudar com algo relacionado ao planejamento de obras residenciais ou a EAP?'
                
                Diretrizes para o planejamento da EAP:
                1. Identificar e listar todas as principais fases da obra, desde a concep��o inicial at� o acabamento final.
                2. Definir os entreg�veis e marcos cr�ticos do projeto.
                3. Incluir atividades detalhadas para cada fase, como funda��o, estrutura, el�trica, hidr�ulica, acabamento, e paisagismo.
                4. Considerar as necessidades de integra��o de tecnologias (sistemas de energia solar, automa��o residencial).
                5. Garantir que a EAP esteja adequada para controle de tempo e custos, com a possibilidade de ajustes durante a obra.
                6. Focar na sustentabilidade e uso de materiais de alta qualidade para garantir o padr�o do projeto.
                7. Gerar cronograma para as atividades e marcos, considerando prazos realistas e margens de seguran�a.
                8. Utilize a data atual para definir o in�cio e t�rmino das atividades, considerando a dura��o de cada uma.
                9. Salvar a EAP em um arquivo TXT.

                Entrada do Usu�rio:
                - Tipo de projeto (casa t�rrea ou sobrado).
                - Metros quadrados da constru��o.
                - Ambientes desejados (n�mero de quartos, banheiros, �reas sociais).
                - Requisitos espec�ficos (energia solar, aquecimento de piscina, acabamentos de luxo).

                Resultado Esperado:
                - Apresenta��o da entrada do usu�rio, exemplo: Voc� deseja construir uma casa t�rrea com 300m�, 3 quartos, 3 banheiros, �rea social integrada, energia solar e piscina aquecida.
                - Um descritivo em formato de EAP, dividido em entreg�veis e tarefas, organizados de forma hier�rquica, que servir� como base para o cronograma e execu��o do projeto.
                ";

            var history = new ChatHistory(promptSystem);

            if (!string.IsNullOrEmpty(req.Form["question"]))
            {
                string userInput = req.Form["question"]!;
                history.AddUserMessage(userInput);
            }

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                // Adicionando as configura��es de execu��o do prompt (Planner)
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            return new OkObjectResult(result.ToString());
        }
    }
}
