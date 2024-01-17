using ApiToExcel.Clients;
using ApiToExcel.Models;
using ApiToExcel.Services;
using ClosedXML.Excel;
using Newtonsoft.Json.Linq;

namespace ApiToExcel
{
    public partial class FrmPessoFornecedores : Form, IDisposable
    {
        private VarejoFacilClient? _varejoFacil;

        public FrmPessoFornecedores()
        {
            InitializeComponent();
        }

        // TODO: Implementar
        private async Task<JArray> ConsultarProdutos()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        // // TODO: método VarejoFacilClient.ConsultarProdutos precisa ser criado
        // private async Task<JArray> ConsultarProdutos()
        // {
        //     JObject pagina = _varejoFacil.ConsultarProdutos(max: int.MaxValue);
        //     return (JArray)pagina.items;
        // }

        // TODO: Implementar
        private async Task<JArray> ConsultarFornecedores()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        // // TODO: método VarejoFacilClient.ConsultarFornecedores precisa ser criado
        // private async Task<JArray> ConsultarFornecedores()
        // {
        //     JObject pagina = _varejoFacil.ConsultarFornecedores(max: int.MaxValue);
        //     return (JArray)pagina["items"];
        // }

        private async Task<JArray> ConsultarVinculos(JArray produtos)
        {
            return await ConsultarVinculos(produtos.Select(p => p["id"]!.Value<int>()));
        }

        private async Task<JArray> ConsultarVinculos(IEnumerable<int> produtoIds)
        {   
            JArray vinculos = new();

            foreach(int produtoId in produtoIds)
            {
                JArray vinculosProduto = await ConsultarVinculos(produtoId);
                JArray items = (JArray)vinculosProduto["items"]!;
                
                vinculos.Add(items);
            }

            return vinculos;
        }

        // TODO: Implementar
        private async Task<JArray> ConsultarVinculos(int produtoId)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        // // TODO: método VarejoFacilClient.ConsultarVinculos precisa ser criado
        // private async Task<JArray> ConsultarVinculos(int produtoId)
        // {
        //     JObject pagina = varejoFacil.ConsultarVinculos(
        //         produtoId: produtoId,
        //         max: int.MaxValue);

        //     return (JArray)pagina["items"];
        // }

        // Passo 1: Consultar items genêricos da Rota
        private async Task<JArray> GerarRelatorioGenerico()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        // Passo 0: Criar os métodos de consulta em VarejoFacilClient

        // VarejoFacilClient.ConsultarProdutos(int max=100)
        // GET: /api/v1/produto/produtos?limite=max

        // VarejoFacilClient.ConsultarFornecedores(int max=100)
        // GET: /api/v1/pessoa/fornecedores?limite=max

        // VarejoFacilClient.ConsultarVinculos(int produtoId, int max=100)
        // GET: /api/v1/produto/produtos/{produtoId}/?limite=max
        private async Task<JArray> GerarRelatorioVinculos()
        {
            // Passo 1: Você precisar ter as tres coleções preenchida com dados
            JArray produtos = await ConsultarProdutos();
            JArray fornecedores = await ConsultarFornecedores();
            JArray vinculos = await ConsultarVinculos(produtos);

            // Passo 2: Faz o join e escolher as propriedades e seus nomes
            JArray relatorio =
                from p in produtos
                join v in vinculos
                    on p["produtoId"]!.Value<int>() equals v["produtoId"]!.Value<int>()
                join f in fornecedores
                    on v["fornecedorId"]!.Value<int>() equals f["fornecedorId"]!.Value<int>()
                select new
                {
                    descricao = p["descricao"],
                    fornecedorId = f["fornecedorId"],
                    nome = f["nome"],
                    referencia = v["referencia"],
                    unidade = v["unidade"],
                    quantidade = v.["quantidade"],
                    nivel = v.["nivel"]
                };

            return relatorio;
        }

        private async Task GerarArquivoXML()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        private async void BtnExecutar_Click(object sender, EventArgs e)
        {
            string urlEntrada = TxUrlAPI.Text;
            string url = $"https://{urlEntrada}";

            try
            {
                await UpdateTxtJson("Iniciando");

                if (string.IsNullOrWhiteSpace(urlEntrada))
                {
                    await UpdateTxtJson("A url da API está vazia.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxRouter.Text))
                {
                    await UpdateTxtJson("A Rota da API está vazia.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxUser.Text))
                {
                    await UpdateTxtJson("O Usuario está vazio.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxPass.Text))
                {
                    await UpdateTxtJson("A Senha está vazia.\n\nTente novamente!");
                    return;
                }

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var credential = new PasswordCredential(
                        Username: TxUser.Text,
                        Password: TxPass.Text);

                    await UpdateTxtJson("Solicitado o acesso.");

                    // WARNING: ...
                    _varejoFacil = new VarejoFacilClient(
                        baseAddress: url,
                        passwordCredential: credential);

                    // TODO: substitui o uso da variável local varejoFacil pelo camplo _varejoFacil
                    using var varejoFacil = new VarejoFacilClient(
                        baseAddress: url,
                        passwordCredential: credential);

                    await UpdateTxtJson("Acessando a o caminho da API.");

                    dynamic resultados = varejoFacil.GetFromRoute<dynamic>(TxRouter.Text);

                    await UpdateTxtJson("Criando Arquivo Excel.");

                    var io = new IOService();

                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Planilha");

                    // Verifique se fornecedores.items é uma coleção JSON
                    if (resultados.items is JArray jsonArray)
                    {
                        // Preencha a planilha com os dados da segunda rota
                        int rowIndex = 1; // Começando da primeira linha

                        List<string> columns = new List<string>();

                        foreach (var item in jsonArray)
                        {
                            if (item is JObject jsonObject && jsonObject["id"] is JValue idValue)
                            {
                                string produtoId = idValue.ToString();

                                // Consulte a segunda rota com base no ID
                                string novaRota = $"/api/v1/produto/produtos/{produtoId}/fornecedores";
                                dynamic resultadoNovaRota = varejoFacil.GetFromRoute<dynamic>(novaRota);

                                if (resultadoNovaRota.items is JArray novaRotaArray && novaRotaArray.Count > 0)
                                {
                                    // Adicione a coluna "descricao" à planilha se ainda não existir
                                    if (rowIndex == 1 && columns.Count == 0)
                                    {
                                        columns = GetColumnNames(novaRotaArray[0]).ToList();

                                        // Adicione a coluna "descricao"
                                        columns.Add("produto");

                                        for (var colIndex = 0; colIndex < columns.Count; colIndex++)
                                        {
                                            worksheet.Cell(1, colIndex + 1).Value = columns[colIndex];
                                        }
                                    }

                                    // Adicione os dados da coluna "descricao" à planilha
                                    if (jsonObject["descricao"] is JValue descricaoValue)
                                    {
                                        // Adicione "produto" ao nome da coluna
                                        var columnName = "produto";

                                        worksheet.Cell(rowIndex, columns.IndexOf(columnName) + 1).Value = descricaoValue.ToString();
                                    }


                                    // Adicione os dados da segunda rota à planilha
                                    foreach (var novaRotaItem in novaRotaArray)
                                    {
                                        rowIndex++;

                                        // Adicione os dados do novo item à planilha
                                        var colIndex = 1;
                                        if (novaRotaItem is JObject obj)
                                        {
                                            foreach (var property in obj.Properties())
                                            {
                                                worksheet.Cell(rowIndex, colIndex).Value = property.Value?.ToString();
                                                colIndex++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Salve o arquivo Excel
                    workbook.SaveAs(saveFileDialog1.FileName);
                    await UpdateTxtJson("Arquivo Excel criado com sucesso.");
                }
            }
            catch (Exception ex)
            {

                UpdateTxtJson($"Erro na requisição: {ex.Message}");
                throw ex;
            }
        }

        public void TxtJson_TextChanged(object sender, EventArgs e)
        {

        }

        async Task UpdateTxtJson(string message)
        {
            TxtJson.Text = message;
            await Task.Delay(1000); // Pausa de 1 segundo
        }

        private void FrmPessoFornecedores_Load(object sender, EventArgs e)
        {

        }

        private static JToken GetValueFromNestedProperty(JObject obj, string propertyName)
        {
            var propertyNames = propertyName.Split('.');
            var currentObj = (JToken)obj;

            foreach (var name in propertyNames)
            {
                if (currentObj is JObject && currentObj[name] is JObject nestedObject)
                {
                    currentObj = nestedObject;
                }
                else if (currentObj is JObject && currentObj[name] is JValue value)
                {
                    return value;
                }
                else
                {
                    return null; // Propriedade não encontrada
                }
            }

            return currentObj;
        }

        private List<string> GetColumnNames(JToken jsonToken)
        {
            var columns = new List<string>();

            if (jsonToken is JObject jsonObject)
            {
                foreach (var property in jsonObject.Properties())
                {
                    columns.Add(property.Name);
                }
            }
            else if (jsonToken is JArray jsonArray && jsonArray.Count > 0)
            {
                if (jsonArray[0] is JObject firstObject)
                {
                    foreach (var property in firstObject.Properties())
                    {
                        columns.Add(property.Name);
                    }
                }
            }

            return columns;
        }

        public void Dispose()
        {
            _varejoFacil.Dispose();
        }
    }
}