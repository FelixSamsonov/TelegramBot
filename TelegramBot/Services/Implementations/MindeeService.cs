using Microsoft.Extensions.Logging;
using Mindee;
using Mindee.Http;
using Mindee.Input;
using Mindee.Parsing.Generated;
using Mindee.Product.Generated;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations;

public class MindeeService : IMindeeService
{
    private readonly MindeeClient _mindeeClient;
    private readonly ILogger<MindeeService> _logger;

    public MindeeService(MindeeClient mindeeClient, ILogger<MindeeService> logger)
    {
        _mindeeClient = mindeeClient;
        _logger = logger;
    }

    // Обробляє фото паспорта та повертає розпізнані дані.
    public async Task<PersonData?> ProcessPassportAsync(string pdfPath, CancellationToken cancellationToken)
    {
        try
        {
            var inputSource = new LocalInputSource(pdfPath);
            var endpoint = new CustomEndpoint("ukraine_id_card", "TestBot", "1");
            var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

            if (response?.Document?.Inference?.Prediction is null)
            {
                _logger.LogWarning("Mindee response for passport was null or did not contain a prediction.");
                return null;
            }

            var combinedFields = GetCombinedFields(response.Document.Inference.Prediction.Fields);

            return new PersonData
            {
                Surname = GetBestFieldValue(combinedFields, "surname"),
                Name = GetBestFieldValue(combinedFields, "name"),
                GivenName = GetBestFieldValue(combinedFields, "givenname"),
                DateOfBirth = GetBestFieldValue(combinedFields, "dateofbirth"),
                DocumentNumber = GetBestFieldValue(combinedFields, "documentnumber"),
                RecordNumber = GetBestFieldValue(combinedFields, "recordnumber"),
                RegisteredAddress = GetBestFieldValue(combinedFields, "registeredaddress"),
                Rntrc = GetBestFieldValue(combinedFields, "rntrc"),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing passport with Mindee.");
            throw new ApplicationException("Не вдалося обробити паспорт. Спробуйте ще раз.", ex);
        }
    }

    public async Task<VehicleRegistration?> ProcessCarRegistrationAsync(string pdfPath, CancellationToken cancellationToken)
    {
        try
        {
            var inputSource = new LocalInputSource(pdfPath);
            var endpoint = new CustomEndpoint("ukraine_registration_certificate", "TestBot", "1");
            var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

            if (response?.Document?.Inference?.Prediction is null)
            {
                _logger.LogWarning("Mindee response for car registration was null or did not contain a prediction.");
                return null;
            }

            var combinedFields = GetCombinedFields(response.Document.Inference.Prediction.Fields);

            return new VehicleRegistration
            {
                LicensePlate = GetBestFieldValue(combinedFields, "license_plate"),
                VehicleMake = GetBestFieldValue(combinedFields, "vehicle_make"),
                VehicleModel = GetBestFieldValue(combinedFields, "vehicle_model"),
                VinCode = GetBestFieldValue(combinedFields, "vin_code"),
                OwnerSurname = GetBestFieldValue(combinedFields, "owner_surname"),
                OwnerGivenNames = GetBestFieldValue(combinedFields, "owner_given_names"),
                OwnerAddress = GetBestFieldValue(combinedFields, "owner_address"),
                YearOfManufacture = GetBestFieldValue(combinedFields, "year_of_manufacture"),
                DocumentId = GetBestFieldValue(combinedFields, "document_id"),
                FirstRegistrationDate = GetBestFieldValue(combinedFields, "first_registration_date")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing car registration with Mindee.");
            return null;
        }
    }
    private Dictionary<string, List<string>> GetCombinedFields(Dictionary<string, GeneratedFeature> fields)
    {
        var dict = new Dictionary<string, List<string>>();
        foreach (var field in fields)
        {
            if (!dict.ContainsKey(field.Key))
            {
                dict[field.Key] = new List<string>();
            }
            var values = field.Value.Select(v => v.ToString()
            .Trim()).Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

            if (values.Any())
            {
                dict[field.Key].AddRange(values!);
            }
        }
        return dict;
    }

    private string? GetBestFieldValue(Dictionary<string, List<string>> dict, string fieldName)
    {
        if (!dict.TryGetValue(fieldName, out var list) || list.Count == 0) return null;

        return list
            .Select(v => v.Replace(":value:", "").Trim()).Where(v => !string.IsNullOrWhiteSpace(v))
            .OrderByDescending(v => v.Length)
            .FirstOrDefault();
    }
}
