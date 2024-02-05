using ApiToExcel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
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
            _frm.UpdateTxtJson($"Consultando a página {numeroDaPagina} de {totalPaginas}");
            //_frm.UpdateTxtJson(_tokenCredential.ToString());

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

    public async Task RefreshToken(CancellationToken cancellationToken = default)
    {
        string refreshToken = _tokenCredential.AccessToken;

        //_http.DefaultRequestHeaders.Add("Authorization", _tokenCredential.RefreshToken);

        HttpRequestMessage httpRequestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("api/auth/refresh", UriKind.Relative),
            Headers = { 
                { HttpRequestHeader.Authorization.ToString(), _tokenCredential.RefreshToken },
                { HttpRequestHeader.Accept.ToString(), "application/json" }
            },
        };

        HttpResponseMessage message = await _http.SendAsync(httpRequestMessage, cancellationToken);
        
        // HttpResponseMessage message = await _http.GetAsync($"api/auth/refresh", cancellationToken);

        string json = await message.Content.ReadAsStringAsync();

        TokenCredential? novoToken = JsonConvert.DeserializeObject<TokenCredential>(json);

        // TokenCredential? novoToken = await _http.GetFromJsonAsync<TokenCredential>(
        //     $"api/auth/refresh", cancellationToken);

        _tokenCredential = novoToken!;
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