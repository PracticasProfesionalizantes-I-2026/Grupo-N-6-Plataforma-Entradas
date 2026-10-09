using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EntradApp.Shared.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DniListAttribute : ValidationAttribute
{
    private static readonly Regex DniRegex = new(@"^\d{7,8}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> dnis)
            return new ValidationResult("La lista de DNI es requerida");

        if (dnis.Count == 0)
            return new ValidationResult("Debe proporcionar al menos un DNI");

        var invalidos = dnis.Where(d => string.IsNullOrWhiteSpace(d) || !DniRegex.IsMatch(d.Trim())).ToList();
        if (invalidos.Count > 0)
        {
            return new ValidationResult(
                $"Los siguientes DNI tienen formato inválido (deben ser 7 u 8 dígitos numéricos): {string.Join(", ", invalidos)}");
        }

        // Verificar duplicados en la lista
        var duplicados = dnis
            .Select(d => d.Trim())
            .GroupBy(d => d)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicados.Count > 0)
        {
            return new ValidationResult(
                $"DNI duplicados en la lista: {string.Join(", ", duplicados)}");
        }

        return ValidationResult.Success;
    }
}