using System.Text.Json.Serialization;

namespace DMUCalendarSync.Services.Models.JsonModels;

public class CampusmUserInfo
{
    [JsonPropertyName("authUserInfo")] public AuthUserInfo? AuthUserInfo { get; set; }

    [JsonPropertyName("personID")] public string PersonId { get; set; } = null!;

    [JsonPropertyName("firstname")] public string Firstname { get; set; } = null!;

    [JsonPropertyName("surname")] public string Surname { get; set; } = null!;

    [JsonPropertyName("email")] public string Email { get; set; } = null!;

    [JsonPropertyName("orgCode")] public string? OrgCode { get; set; }

    [JsonPropertyName("serviceUsername_3871")]
    public string? ServiceUsername3871 { get; set; }

    [JsonPropertyName("loginChange")] public bool? LoginChange { get; set; }

    [JsonPropertyName("anonUser")] public object? AnonUser { get; set; }

    [JsonPropertyName("profileID")] public string? ProfileId { get; set; }
}