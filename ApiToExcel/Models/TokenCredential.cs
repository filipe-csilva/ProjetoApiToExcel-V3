namespace ApiToExcel.Models;

public record TokenCredential(
    string AccessToken,
    string RefreshToken);