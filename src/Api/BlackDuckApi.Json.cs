using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using BlackDuckReport.GitHubAction.JsonConverters;

using static BlackDuckReport.GitHubAction.Api.BlackDuckApi.Json;

namespace BlackDuckReport.GitHubAction.Api;

public partial class BlackDuckApi
{
    public static class Json
    {
        public class OAuth2Token
        {
            [JsonPropertyName("bearerToken")]
            public string? BearerToken { get; set; }

            [JsonPropertyName("expiresInMilliseconds")]
            [JsonConverter(typeof(ExpiresInMilliseconds2DateTimeJsonConverter))]
            public DateTime ExpiresAt { get; set; }

            [JsonIgnore]
            public bool IsLogged => !string.IsNullOrEmpty(BearerToken);

            [JsonIgnore]
            public bool IsTokenExpired => DateTime.Now >= ExpiresAt;
        }

        public class Error
        {
            [JsonPropertyName("logRef")]
            public string? LogRef { get; set; }

            [JsonPropertyName("errorMessage")]
            public string? ErrorMessage { get; set; }

            [JsonPropertyName("errorCode")]
            public string? ErrorCode { get; set; }
        }

        public record Vulnerabilities(
            [property: JsonPropertyName("CRITICAL")] int? CRITICAL,
            [property: JsonPropertyName("HIGH")] int? HIGH,
            [property: JsonPropertyName("MEDIUM")] int? MEDIUM,
            [property: JsonPropertyName("LOW")] int? LOW,
            [property: JsonPropertyName("OK")] int? OK,
            [property: JsonPropertyName("UNKNOWN")] int? UNKNOWN
        );

        public record Categories(
            [property: JsonPropertyName("VERSION")] Vulnerabilities VERSION,
            [property: JsonPropertyName("ACTIVITY")] Vulnerabilities ACTIVITY,
            [property: JsonPropertyName("OPERATIONAL")] Vulnerabilities OPERATIONAL,
            [property: JsonPropertyName("VULNERABILITY")] Vulnerabilities VULNERABILITY,
            [property: JsonPropertyName("LICENSE")] Vulnerabilities LICENSE
        );

        public record ProjectItem(
            [property: JsonPropertyName("projectName")] string ProjectName,
            [property: JsonPropertyName("versionName")] string VersionName,
            [property: JsonPropertyName("releasePhase")] string ReleasePhase,
            [property: JsonPropertyName("licenseName")] string LicenseName,
            [property: JsonPropertyName("componentCount")] int? ComponentCount,
            [property: JsonPropertyName("lastScanModifiedAt")] DateTime? LastScanModifiedAt,
            [property: JsonPropertyName("createdAt")] DateTime? CreatedAt,
            [property: JsonPropertyName("lastUpdatedAt")] DateTime? LastUpdatedAt,
            [property: JsonPropertyName("riskProfile")] RiskProfile RiskProfile,
            [property: JsonPropertyName("releasePolicyProfileCounts")] ReleasePolicyProfileCounts ReleasePolicyProfileCounts,
            [property: JsonPropertyName("parentProjectGroupName")] string ParentProjectGroupName,
            [property: JsonPropertyName("tags")] IReadOnlyList<object> Tags,
            [property: JsonPropertyName("_meta")] Meta Meta
        );

        public record ComponentItem(
            [property: JsonPropertyName("component")] string Component,
            [property: JsonPropertyName("componentName")] string ComponentName,
            [property: JsonPropertyName("componentVersion")] string ComponentVersion,
            [property: JsonPropertyName("componentVersionName")] string ComponentVersionName,
            [property: JsonPropertyName("componentVersionOrigin")] string ComponentVersionOrigin,
            [property: JsonPropertyName("componentVersionOriginName")] string ComponentVersionOriginName,
            [property: JsonPropertyName("componentVersionOriginId")] string ComponentVersionOriginId,
            [property: JsonPropertyName("riskPriorityDistribution")] Vulnerabilities RiskPriorityDistribution,
            [property: JsonPropertyName("componentType")] string ComponentType,
            [property: JsonPropertyName("_meta")] Meta Meta
        );

        public record Link(
            [property: JsonPropertyName("rel")] string Rel,
            [property: JsonPropertyName("href")] string Href,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("label")] string Label
        );

        public record Meta(
            [property: JsonPropertyName("allow")] IReadOnlyList<object> Allow,
            [property: JsonPropertyName("href")] string Href,
            [property: JsonPropertyName("links")] IReadOnlyList<object> Links
        );

        public record ReleasePolicyProfileCounts(
            [property: JsonPropertyName("policyBlockerCount")] int? PolicyBlockerCount,
            [property: JsonPropertyName("policyCriticalCount")] int? PolicyCriticalCount,
            [property: JsonPropertyName("policyMajorCount")] int? PolicyMajorCount,
            [property: JsonPropertyName("policyMinorCount")] int? PolicyMinorCount,
            [property: JsonPropertyName("policyTrivialCount")] int? PolicyTrivialCount,
            [property: JsonPropertyName("policyUnspecifiedCount")] int? PolicyUnspecifiedCount
        );

        public record RiskProfile(
            [property: JsonPropertyName("categories")] Categories Categories
        );

        public record ProjectListQueryResult(
            [property: JsonPropertyName("appliedFilters")] IReadOnlyList<object> AppliedFilters,
            [property: JsonPropertyName("searchType")] string SearchType,
            [property: JsonPropertyName("totalCount")] int? TotalCount,
            [property: JsonPropertyName("items")] IReadOnlyList<ProjectItem> Items,
            [property: JsonPropertyName("_meta")] Meta Meta,
            [property: JsonPropertyName("lastRefreshedAt")] DateTime? LastRefreshedAt
        );

        public record ComponentListQueryResult(
           [property: JsonPropertyName("totalCount")] int? TotalCount,
           [property: JsonPropertyName("items")] IReadOnlyList<ComponentItem> Items,
           [property: JsonPropertyName("appliedFilters")] IReadOnlyList<object> AppliedFilters,
           [property: JsonPropertyName("_meta")] Meta Meta
       );
    }
}

[JsonSerializable(typeof(Error))]
internal partial class ErrorContext : JsonSerializerContext
{
}
[JsonSerializable(typeof(OAuth2Token))]
internal partial class OAuth2TokenContext : JsonSerializerContext
{
}
[JsonSerializable(typeof(ProjectListQueryResult))]
internal partial class ProjectListQueryResultContext : JsonSerializerContext
{
}
[JsonSerializable(typeof(ComponentListQueryResult))]
internal partial class ComponentListQueryResultContext : JsonSerializerContext
{
}
