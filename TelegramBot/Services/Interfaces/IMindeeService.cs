using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions;

public interface IMindeeService
{
    /// <summary>
    /// Обробляє зображення паспорта та повертає розпізнані дані.
    /// </summary>
    /// <param name="pdfPath">Шлях до PDF-файлу зі сторонами паспорта.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Об'єкт PersonData з розпізнаною інформацією або null.</returns>
    Task<PersonData?> ProcessPassportAsync(string pdfPath, CancellationToken cancellationToken);

    /// <summary>
    /// Обробляє зображення техпаспорта та повертає розпізнані дані.
    /// </summary>
    /// <param name="pdfPath">Шлях до PDF-файлу зі сторонами техпаспорта.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Об'єкт VehicleRegistration з розпізнаною інформацією або null.</returns>
    Task<VehicleRegistration?> ProcessCarRegistrationAsync(string pdfPath, CancellationToken cancellationToken);
}