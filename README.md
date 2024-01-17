# ConversorApiToExcel

API para gerenciamento de tarefas por usuario

| :placard: Vitrine.Dev |     |
| -------------  | --- |
| :sparkles: Nome        | ConversorApiToExcel
| :label: Tecnologias | c#, .Net
| :rocket: URL         | 
| :fire: Desafio     | 

<!-- Inserir imagem com a #vitrinedev ao final do link -->
![](https://cdn.discordapp.com/attachments/769394667531534386/1153395937146834994/Captura_de_tela_2023-09-18_150434.png)

## Descrição do projeto

Este é um aplicativo simples desenvolvido em C# WinForms que permite converter dados de uma API em um arquivo XLSX. Com esta ferramenta, você pode facilmente buscar dados de uma API da web e salvá-los em um arquivo XLSX para análise posterior ou compartilhamento.

Requisitos
Certifique-se de que você tenha os seguintes componentes instalados em seu ambiente de desenvolvimento:

Visual Studio ou outra IDE de sua escolha que suporte C#.
Biblioteca Newtonsoft.Json para lidar com JSON (disponível via NuGet).
Biblioteca ClosedXML para manipulação de arquivos XLSX (disponível via NuGet).
Conexão com a Internet para acessar a API.
Como usar
Clone ou faça o download deste repositório.
Abra o projeto no Visual Studio ou em sua IDE de preferência.
Compile e execute o aplicativo.
Na interface do aplicativo, você verá campos para inserir a URL da API.
Clique no botão "Execultar" para iniciar o processo de conversão.
Interface do Aplicativo

Fluxo de funcionamento
O aplicativo segue este fluxo para converter dados da API em um arquivo XLSX:

O usuário insere a URL da API que deseja consultar.
O aplicativo faz uma solicitação HTTP GET para a URL da API especificada.
Os dados JSON obtidos da API são desserializados usando a biblioteca Newtonsoft.Json.
Os dados são então transformados em uma planilha XLSX usando a biblioteca ClosedXML.
O arquivo XLSX resultante é salvo no local especificado.
Exemplo
Suponha que você deseje converter dados de uma API de exemplo para um arquivo XLSX. Você pode inserir a seguinte URL da API:

arduino
Copy code
api.example.com/data

Após clicar em "Execultar", o aplicativo buscará os dados da API e os salvará.

Licença
Este projeto é distribuído sob a licença MIT.

Contribuições
Contribuições são bem-vindas! Sinta-se à vontade para fazer um fork deste repositório e enviar pull requests para melhorar o aplicativo.

Autor
Este aplicativo foi desenvolvido por Filipe Silva.
