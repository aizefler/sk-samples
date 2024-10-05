using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AlexandreIzefler.AI.SK.Demos.Demo1.Plugins
{
    public class EAPPlugin
    {
        [KernelFunction("format_eap")]
        [Description("Formate EAP conforme regras definidas")]
        [return: Description("EAP formatda")]
        public async Task<string> FormatAsync()
        {
            return @"
                    Formate a EAP conforme:
                    - Máximo 3 níveis.
                    - Primeiro nível representa o marco principal, exemplo: Projetos e Legalização, Fundação, Estrutura e Avenaria, Acabamentos, Pintura, Elétrica e Hidráulica.        
                    - Segundo nível representa as atividades principais, exemplo: Projeto Arquitetônico, Projeto Estrutural, Projeto Elétrico, Projeto Hidráulico.
                    - Terceiro nível representa as atividades detalhadas, exemplo: Projeto Arquitetônico: Planta Baixa, Cortes, Fachadas, Detalhes Construtivos.
                    - Considere o tempo minimo de 2 meses para cada marco principal e 1 semana para cada atividade principal.
                    - Considere a data atual para iniciar o planejamento e informe em parênteses a quantidade de dias e data inicio e fim, exemplo: Fundação (60 dias - Inicio: 02/08/2024 Fim: 02/10/2024).
                    ";
        }

        [KernelFunction("save_eap")]
        [Description("Salve EAP em um arquivo TXT")]
        public async Task SaveAsync(string eap)
        {
            File.WriteAllText("eap.txt", eap);
        }
    }
}
