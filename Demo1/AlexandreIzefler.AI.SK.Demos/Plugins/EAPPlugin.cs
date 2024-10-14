using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AlexandreIzefler.AI.SK.Demos.Demo1.Plugins
{
    public class EAPPlugin
    {
        [KernelFunction("cronograma_eap")]
        [Description("Cronograma EAP")]
        [return: Description("EAP formatada com datas e dias do cronograma")]
        public async Task<string> ScheduleAsync()
        {
            return @"
                    REGRAS DO CRONOGRAMA:
                    1. Condiderar no minimo 3 dias para cada atvidade e para os marco de entrega no minimo 1 semana.
                    2. Adicionar em parênteses a quantidade de dias, data inicio e fim conforme exemplo: Fundação (60 dias - Inicio: 02/08/2024 Fim: 02/10/2024).
                    ";
        }

        [KernelFunction("salvar_eap")]
        [Description("Salve a EAP em um arquivo TXT")]
        public async Task SaveAsync(string eap)
        {
            File.WriteAllText("eap.txt", eap);
        }
    }
}
