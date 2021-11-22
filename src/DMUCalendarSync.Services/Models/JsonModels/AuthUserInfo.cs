using System.Text.Json.Serialization;

namespace DMUCalendarSync.Services.Models.JsonModels;

public class AuthUserInfo
{
    [JsonPropertyName("personId")] public int PersonId { get; set; }

    [JsonPropertyName("orgCode")] public int OrgCode { get; set; }

    [JsonPropertyName("profileGroupId")] public int ProfileGroupId { get; set; }

    [JsonPropertyName("integrationProfileId")]
    public int IntegrationProfileId { get; set; }
}