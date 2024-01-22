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
            ConcatTxtJson("Consultando Produtos");
            return _varejoFacil.ConsultarProdutos();
        }

        private async Task<JArray> ConsultarFornecedores()
        {
            ConcatTxtJson("Consultando Fornecedores ");
            return _varejoFacil.ConsultarFornecedores();
        }

        private async Task<JArray> ConsultarVinculos(JArray produtos)
        {
            ConcatTxtJson("Consultando Vinculos");
            return await ConsultarVinculos(produtos.Select(p => p["id"]!.Value<long>()));
        }

        int quantidadeAtual = 0;

        private async Task<JArray> ConsultarVinculos(IEnumerable<long> produtoIds)
        {
            DateTime startTime = DateTime.Now; // [Exemplo]
            DateTime expirationTime = startTime.AddMinutes(28); // [Exemplo]

            JArray vinculos = new();

            int quantidadeTotal = produtoIds.Count();

            foreach (long produtoId in produtoIds)
            {
                if(DateTime.Now >= expirationTime)  // [Exemplo]
                {
                    _varejoFacil.RefreshToken();  // [Exemplo]
                }

                JArray items = await ConsultarVinculos(produtoId);

                foreach (JToken item in items)
                {
                    vinculos.Add(item);
                }
                quantidadeAtual++;
                ConcatTxtJson($"Consultando vinculo do item {quantidadeAtual} de {quantidadeTotal}");
            }

            return vinculos;
        }

        private async Task<JArray> ConsultarVinculos(long produtoId)
        {
            return _varejoFacil.ConsultarVinculos(produtoId);
        }

        // Passo 1: Consultar items genericos pela rota informada pelo usuario
        private async Task<JArray> GerarRelatorioGenerico()
        {
            ConcatTxtJson("Acessando Rotina Solicitada");
            return _varejoFacil.GetFromRoute<JObject>(TxRouter.Text);
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
            ConcatTxtJson("Gerando arquivo Excel");
            _io.WriteXmlFile(items, pathSave);
            ConcatTxtJson("Arquivo Excel criado com sucesso.");
        }

        private async void BtnExecutar_Click(object sender, EventArgs e)
        {

            string url = TxUrlAPI.Text;

            try
            {
                ConcatTxtJson("Iniciando");

                if (string.IsNullOrWhiteSpace(url))
                {
                    ConcatTxtJson("A url da API está vazia.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxRouter.Text))
                {
                    ConcatTxtJson("A Rota da API está vazia.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxUser.Text))
                {
                    ConcatTxtJson("O Usuario está vazio.\n\nTente novamente!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxPass.Text))
                {
                    ConcatTxtJson("A Senha está vazia.\n\nTente novamente!");
                    return;
                }

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    var credential = new PasswordCredential(
                        Username: TxUser.Text,
                        Password: TxPass.Text);

                    ConcatTxtJson("Solicitando Autorização");

                    if (_varejoFacil is null)
                        _varejoFacil = new VarejoFacilClient(url, credential, this);
                    else
                        _varejoFacil.RefreshToken();

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

                ConcatTxtJson($"Erro na requisição: {ex.Message}");
                throw;
            }
        }

        public void TxtJson_TextChanged(object sender, EventArgs e)
        {

        }

        public void ConcatTxtJson(string message)
        {
            TxtJson.Text += Environment.NewLine+message;
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