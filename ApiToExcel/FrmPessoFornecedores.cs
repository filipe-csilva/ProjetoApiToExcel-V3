using ApiToExcel.Clients;
using ApiToExcel.Models;
using ApiToExcel.Services;
using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ApiToExcel
{
    public partial class FrmPessoFornecedores : Form, IDisposable
    {
        private VarejoFacilClient? _varejoFacil;
        private IOService _io = new();

        public FrmPessoFornecedores()
        {
            InitializeComponent();
        }

        private async Task<JArray> ConsultarProdutos()
        {
            await UpdateTxtJson("Consultando Produtos");
            JObject pagina = _varejoFacil.ConsultarProdutos(count: int.MaxValue);
            return (JArray)pagina["items"]!;
        }

        private async Task<JArray> ConsultarFornecedores()
        {
            await UpdateTxtJson("Consultando Fornecedores ");
            JObject pagina = _varejoFacil.ConsultarFornecedores(count: int.MaxValue);
            return (JArray)pagina["items"]!;
        }

        private async Task<JArray> ConsultarVinculos(JArray produtos)
        {
            await UpdateTxtJson("Consultando Vinculos");
            return await ConsultarVinculos(produtos.Select(p => p["id"]!.Value<long>()));
        }

        int quantidadeAtual = 0;

        private async Task<JArray> ConsultarVinculos(IEnumerable<long> produtoIds)
        {
            JArray vinculos = new();

            int quantidadeTotal = produtoIds.Count();

            foreach (long produtoId in produtoIds)
            {
                JArray items = await ConsultarVinculos(produtoId);

                foreach (JToken item in items)
                {
                    vinculos.Add(item);
                }
                quantidadeAtual++;
                await UpdateTxtJson($"Consultando vinculo do item {quantidadeAtual} de {quantidadeTotal}");
            }

            return vinculos;
        }

        private async Task<JArray> ConsultarVinculos(long produtoId)
        {
            JObject pagina = _varejoFacil.ConsultarVinculos(
                produtoId: produtoId,
                count: int.MaxValue);

            return (JArray)pagina["items"]!;
        }

        // Passo 1: Consultar items genericos pela rota informada pelo usuario
        private async Task<JArray> GerarRelatorioGenerico()
        {
            await UpdateTxtJson("Acessando Rotina Solicitada");
            JObject pagina = _varejoFacil.GetFromRoute<JObject>(TxRouter.Text);
            await UpdateTxtJson("Arquivo JSON Recolhido");
            JArray items = (JArray)pagina["items"]!;
            return items;
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
            // Passo 1: Voce precisar ter as tres coleções preenchida com dados
            JArray produtos = await ConsultarProdutos();
            JArray fornecedores = await ConsultarFornecedores();
            JArray vinculos = await ConsultarVinculos(produtos);

            // Passo 2: Faz o join e escolher as propriedades e seus nomes
            IEnumerable<JToken> items =
                from p in produtos
                join v in vinculos
                    on p["id"]!.Value<long>() equals v["produtoId"]!.Value<long>()
                join f in fornecedores
                    on v["fornecedorId"]!.Value<int>() equals f["id"]!.Value<int>()
                select JObject.FromObject(new
                {
                    codigo = p["id"],
                    descricao = p["descricao"],
                    fornecedorId = f["id"],
                    nome = f["nome"],
                    referencia = v["referencia"],
                    unidade = v["unidade"],
                    quantidade = v["quantidade"],
                    nivel = v["nivel"]
                });

            return new JArray(items);
        }

        private async Task GerarArquivoXML(JArray items, string pathSave)
        {
            await UpdateTxtJson("Gerando arquivo Excel");
            _io.WriteXmlFile(items, pathSave);
            await UpdateTxtJson("Arquivo Excel criado com sucesso.");
        }

        private async void BtnExecutar_Click(object sender, EventArgs e)
        {
            /* Inserir o código aqui */

            string url = TxUrlAPI.Text;

            try
            {
                await UpdateTxtJson("Iniciando");

                if (string.IsNullOrWhiteSpace(url))
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

                    await UpdateTxtJson("Solicitando Autorização");

                    _varejoFacil ??= new VarejoFacilClient(
                        baseAddress: url,
                        passwordCredential: credential);

                    // TODO: adicionar checkbox ou radiobox
                    bool vinculoAtivo = false;
                    if (Cb_Vinculo.Checked) vinculoAtivo = true;
                    bool isModuloProdutosFornecedores = vinculoAtivo;

                    JArray relatorio = isModuloProdutosFornecedores switch
                    {
                        true => await GerarRelatorioVinculos(),
                        false => await GerarRelatorioGenerico()
                    };

                    await GerarArquivoXML(relatorio, saveFileDialog1.FileName);

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

        public async Task UpdateTxtJson(string message)
        {
            TxtJson.Text = message;
        }

        private void FrmPessoFornecedores_Load(object sender, EventArgs e)
        {

        }

        public void Dispose()
        {
            _varejoFacil.Dispose();
        }
    }
}