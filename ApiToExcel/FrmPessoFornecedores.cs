using ApiToExcel.Clients;
using ApiToExcel.Models;
using ApiToExcel.Services;
using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
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
            // DateTime expirationTime = startTime.AddMinutes(1); // [Exemplo]
            DateTime expirationTime = startTime.AddMinutes(20); // [Exemplo]

            JArray vinculos = new();

            int quantidadeTotal = produtoIds.Count();

            foreach (long produtoId in produtoIds)
            {
                if (DateTime.Now >= expirationTime)  // [Exemplo]
                {
                    expirationTime = DateTime.Now.AddMinutes(20);
                    await _varejoFacil.RefreshToken();  // [Exemplo]
                }

                JArray items = await ConsultarVinculos(produtoId);

                foreach (JToken item in items)
                {
                    vinculos.Add(item);
                }
                quantidadeAtual++;
                UpdateTxtJson($"Consultando vinculo do item {quantidadeAtual} de {quantidadeTotal}");
            }

            return vinculos;
        }

        private async Task<JArray> ConsultarVinculos(long produtoId)
        {
            return _varejoFacil.ConsultarVinculos(produtoId);
        }

        private async Task<JArray> GerarRelatorioGenerico()
        {
            ConcatTxtJson("Acessando Rotina Solicitada");
            string router = TxRouter.Text;
            return _varejoFacil.GetFromRoute<JObject>($"{router}");
        }

        private async Task<JArray> GerarRelatorioVinculos()
        {
            JArray produtos = await ConsultarProdutos();
            JArray fornecedores = await ConsultarFornecedores();
            JArray vinculos = await ConsultarVinculos(produtos);

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
                UpdateTxtJson("Iniciando");

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
                        await _varejoFacil.RefreshToken();

                    // so para test
                    await _varejoFacil.RefreshToken();

                    bool isModuloProdutosFornecedores = Cb_Vinculo.Checked;

                    Task<JArray> taskRelatorio = isModuloProdutosFornecedores switch
                    {
                        true => GerarRelatorioVinculos(),
                        false => GerarRelatorioGenerico()
                    };

                    // using CancellationTokenSource source = new();
                    // CancellationToken token = source.Token;

                    // var _ = Task.Run(async () => {
                    //     while(!token.IsCancellationRequested)
                    //     {
                    //         await Task.Delay(TimeSpan.FromSeconds(15), token);
                    //         await _varejoFacil.RefreshToken(token);
                    //         Debug.WriteLine("Token Atualizado, talvez, se tivermos sorte!");
                    //     }
                    // }, token);

                    JArray relatorio = await taskRelatorio;
                    //source.Cancel();

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
            if (!string.IsNullOrEmpty(TxtJson.Text))
            {
                TxtJson.AppendText(Environment.NewLine);
            }

            TxtJson.AppendText(message);

            // Role automaticamente para o final
            TxtJson.SelectionStart = TxtJson.Text.Length;
            TxtJson.ScrollToCaret();
        }

        public void UpdateTxtJson(string message)
        {
            // Verifica se há pelo menos uma linha
            if (TxtJson.Lines.Length > 0)
            {
                // Divide o conteúdo do RichTextBox por linhas
                string[] linhas = TxtJson.Lines;

                // Atualiza a última linha com o novo conteúdo
                linhas[linhas.Length - 1] = message;

                // Atualiza o conteúdo do RichTextBox
                TxtJson.Lines = linhas;

                // Role automaticamente para o final
                TxtJson.SelectionStart = TxtJson.Text.Length;
                TxtJson.ScrollToCaret();
            }
            else
            {
                    TxtJson.Text = message;
            }
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