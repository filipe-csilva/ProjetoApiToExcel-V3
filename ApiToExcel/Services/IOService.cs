using ClosedXML.Excel;
using Newtonsoft.Json.Linq;

namespace ApiToExcel.Services;

public class IOService
{
    // padrão: ele salvaria um arquivo com um nome aleatório dentro da pasta Desktop
    // ou especificar onde salvar o arquivo

    public IOService()
    {
        DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }

    public IOService(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public string DirectoryPath { get; }

    public void WriteXmlFile(JArray jsonArray)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha");

        // Verifique se há itens na coleção
        if (jsonArray.Any())
        {
            var firstItem = jsonArray[0];
            if (firstItem is JObject jsonObject)
            {
                // Crie uma lista para armazenar todos os nomes de coluna
                var columns = new List<string>();

                // Adicione os nomes das colunas à lista
                foreach (var property in jsonObject.Properties())
                {
                    if (property.Value is JValue)
                    {
                        columns.Add(property.Name);
                    }
                    else if (property.Value is JObject nestedObject)
                    {
                        // Se for um objeto JSON aninhado, decomponha-o em colunas separadas
                        foreach (var nestedProperty in nestedObject.Properties())
                        {
                            columns.Add($"{property.Name}.{nestedProperty.Name}");
                        }
                    }
                }

                // Adicione os nomes das colunas à primeira linha da planilha
                for (var colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    worksheet.Cell(1, colIndex + 1).Value = columns[colIndex];
                }

                // Preencha a planilha com os dados
                for (var rowIndex = 0; rowIndex < jsonArray.Count; rowIndex++)
                {
                    var item = (JObject)jsonArray[rowIndex];
                    for (var colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        var columnName = columns[colIndex];
                        var columnValue = GetValueFromNestedProperty(item, columnName);
                        worksheet.Cell(rowIndex + 2, colIndex + 1).Value = columnValue?.ToString();
                    }
                }
            }
        }

        const string fileName = "merenomeie.xlsx";
        string filePath = Path.Combine(DirectoryPath, fileName);
        workbook.SaveAs(filePath);
    }

    private static JToken GetValueFromNestedProperty(JObject obj, string propertyName)
    {
        var propertyNames = propertyName.Split('.');
        var currentObj = (JToken)obj;

        foreach (var name in propertyNames)
        {
            if (currentObj[name] is JObject nestedObject)
            {
                currentObj = nestedObject;
            }
            else if (currentObj[name] is JValue value)
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

}