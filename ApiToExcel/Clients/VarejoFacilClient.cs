using ApiToExcel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace ApiToExcel.Clients;

public class VarejoFacilClient : IDisposable
{
    private const string AuthRoute = "api/auth";
    private readonly HttpClient _http;
    private readonly TokenCredential _tokenCredential;
    private bool _disposed;

    public VarejoFacilClient(string baseAddress, PasswordCredential passwordCredential)
    {
        _http = new HttpClient()
        {
            BaseAddress = new Uri(baseAddress)
        };

        _tokenCredential = Authorize(passwordCredential);
    }

    public T GetFromRoute<T>(string route)
    {
        _http.DefaultRequestHeaders.Authorization = new(_tokenCredential.AccessToken);

        string json = _http.GetStringAsync(route).Result!;

        return JsonConvert.DeserializeObject<T>(json)!;
    }

    public JObject ConsultarProdutos(int count = 1)
    {
        return GetFromRoute<JObject>($"/api/v1/produto/produtos?count={count}");
    }

    public JObject ConsultarFornecedores(int count = 1)
    {
        return GetFromRoute<JObject>($"/api/v1/pessoa/fornecedores?count={count}");
    }

    public JObject ConsultarVinculos(long produtoId, int count = 1000)
    {
        return GetFromRoute<JObject>($"/api/v1/produto/produtos/{produtoId}/fornecedores?count={count}");
    }

    public TokenCredential Authorize(PasswordCredential credential)
    {
        var response = _http.PostAsJsonAsync(AuthRoute, credential).Result;
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<TokenCredential>().Result!;
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