using ApiToExcel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace ApiToExcel.Clients;

public class VarejoFacilClient : IDisposable
{
    private const string AuthRoute = "api/auth";
    private readonly FrmPessoFornecedores _frm;
    private readonly HttpClient _http;
    private TokenCredential _tokenCredential;
    private bool _disposed;
    const int maxCount = 500;
    const int startCount = 0;

    public VarejoFacilClient(
        string baseAddress,
        PasswordCredential passwordCredential,
        FrmPessoFornecedores frm)
    {
        _http = new HttpClient()
        {
            BaseAddress = new Uri(baseAddress)
        };

        _tokenCredential = Authorize(passwordCredential);
        _frm = frm;
    }

    public JArray GetFromRoute<T>(string route)
    {
        _http.DefaultRequestHeaders.Authorization = new(_tokenCredential.AccessToken);

        int inicio = 0;
        int limite = 500;
        
        int numeroDaPagina = 1;

        _frm.ConcatTxtJson($"Consultando a página {numeroDaPagina}");

        string json = ConsultarItens(route: route, limite: limite, inicio: inicio);

        var pContar = JsonConvert.DeserializeObject<Paginacao<T>>(json)!;

        var ItensTotal = pContar.ItemsTotal;

        JArray dadosConsultados = new JArray();

        double totalPaginas = Math.Ceiling((double)ItensTotal / limite);

        foreach(var item in pContar.Dados)
        {
            dadosConsultados.Add(item);
        }

        numeroDaPagina++;

        for (int inicial = limite; inicial <= ItensTotal; inicial += limite)
        {
            _frm.ConcatTxtJson($"Consultando a página {numeroDaPagina} de {totalPaginas}");

            string paginaAtual = ConsultarItens(route: route, limite: limite, inicio: inicial);

            var paginaAtualConvertida = JsonConvert.DeserializeObject<Paginacao<T>>(paginaAtual)!;

            foreach (var item in paginaAtualConvertida.Dados)
            {
                dadosConsultados.Add(item);
            }

            numeroDaPagina++;
        }

        return dadosConsultados;
    }

    private string ConsultarItens(string route, int limite, int inicio = 0)
    {
        var routeAdd = $"{route}?start={inicio}&count={limite}";

        return _http.GetStringAsync(routeAdd).Result!;
    }

    public JArray ConsultarProdutos()
    {
        return GetFromRoute<JObject>($"/api/v1/produto/produtos");
    }

    public JArray ConsultarFornecedores()
    {
        return GetFromRoute<JObject>($"/api/v1/pessoa/fornecedores");
    }

    public JArray ConsultarVinculos(long produtoId)
    {
        return GetFromRoute<JObject>($"/api/v1/produto/produtos/{produtoId}/fornecedores");
    }

    public TokenCredential Authorize(PasswordCredential credential)
    {
        var response = _http.PostAsJsonAsync(AuthRoute, credential).Result;
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<TokenCredential>().Result!;
    }

    public void RefreshToken()
    {
        string refreshToken = _tokenCredential.AccessToken;

        TokenCredential novoToken = _http.GetFromJsonAsync<TokenCredential>(
            $"auth/refresh?refreshToken={refreshToken}").Result!;

        _tokenCredential = novoToken;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _http.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}