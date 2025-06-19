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

        //public class Arguments
        //{
        //    [JsonPropertyName("uri")]
        //    public string? Uri { get; set; }
        //}

        public class Error
        {
            [JsonPropertyName("logRef")]
            public string? LogRef { get; set; }

            [JsonPropertyName("errorMessage")]
            public string? ErrorMessage { get; set; }

            //[JsonPropertyName("arguments")]
            //public Arguments? Arguments { get; set; }

            [JsonPropertyName("errorCode")]
            public string? ErrorCode { get; set; }

            //[JsonPropertyName("errors")]
            //public List<object>? Errors { get; set; }

            //[JsonPropertyName("links")]
            //public List<object>? Links { get; set; }
        }

        public class Meta
        {
            [JsonPropertyName("href")]
            public Uri? Href { get; set; }
        }

        public class ProjectItem
        {
            [JsonPropertyName("projectName")]
            public string? ProjectName { get; set; }

            [JsonPropertyName("versionName")]
            public string? VersionName { get; set; }

            [JsonPropertyName("releasePhase")]
            public string? ReleasePhase { get; set; }

            [JsonPropertyName("releaseDistribution")]
            public string? ReleaseDistribution { get; set; }

            [JsonPropertyName("licenseName")]
            public string? LicenseName { get; set; }

            [JsonPropertyName("componentCount")]
            public int ComponentCount { get; set; }

            [JsonPropertyName("lastScanModifiedAt")]
            public DateTime? LastScanModifiedAt { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime? CreatedAt { get; set; }

            [JsonPropertyName("lastUpdatedAt")]
            public DateTime? LastUpdatedAt { get; set; }

            //[JsonPropertyName("riskProfile")]
            //public RiskProfile? RiskProfile { get; set; }

            //[JsonPropertyName("releasePolicyProfileCounts")]
            //public ReleasePolicyProfileCounts? ReleasePolicyProfileCounts { get; set; }

            [JsonPropertyName("parentProjectGroupName")]
            public string? ParentProjectGroupName { get; set; }

            //[JsonPropertyName("tags")]
            //public List<object> Tags { get; set; }

            [JsonPropertyName("_meta")]
            public Meta? Meta { get; set; }
        }

        public class ProjectVersions
        {
            [JsonPropertyName("totalCount")]
            public int TotalCount { get; set; }

            [JsonPropertyName("items")]
            public List<ProjectItem>? Items { get; set; }

            [JsonPropertyName("_meta")]
            public Meta? Meta { get; set; }

            [JsonPropertyName("lastRefreshedAt")]
            public DateTime LastRefreshedAt { get; set; }
        }

        public class VulnerabilityWithRemediation
        {
            [JsonPropertyName("vulnerabilityName")]
            public string? VulnerabilityName { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("vulnerabilityPublishedDate")]
            public DateTime VulnerabilityPublishedDate { get; set; }

            [JsonPropertyName("vulnerabilityUpdatedDate")]
            public DateTime VulnerabilityUpdatedDate { get; set; }

            [JsonPropertyName("baseScore")]
            public double BaseScore { get; set; }

            [JsonPropertyName("overallScore")]
            public double OverallScore { get; set; }

            [JsonPropertyName("exploitabilitySubscore")]
            public double ExploitabilitySubscore { get; set; }

            [JsonPropertyName("impactSubscore")]
            public double ImpactSubscore { get; set; }

            [JsonPropertyName("source")]
            public string? Source { get; set; }

            [JsonPropertyName("severity")]
            public string? Severity { get; set; }

            [JsonPropertyName("remediationStatus")]
            public string? RemediationStatus { get; set; }

            [JsonPropertyName("remediationCreatedAt")]
            public DateTime RemediationCreatedAt { get; set; }

            [JsonPropertyName("remediationUpdatedAt")]
            public DateTime RemediationUpdatedAt { get; set; }

            [JsonPropertyName("remediationCreatedBy")]
            public string? RemediationCreatedBy { get; set; }

            [JsonPropertyName("remediationUpdatedBy")]
            public string? RemediationUpdatedBy { get; set; }

            [JsonPropertyName("relatedVulnerability")]
            public string? RelatedVulnerability { get; set; }

            [JsonPropertyName("bdsaTags")]
            public List<string>? BdsaTags { get; set; }

            [JsonPropertyName("cweId")]
            public string? CweId { get; set; }
        }

        public class RiskPriorityDistribution
        {
            [JsonPropertyName("HIGH")]
            public int HIGH { get; set; }

            [JsonPropertyName("MEDIUM")]
            public int MEDIUM { get; set; }

            [JsonPropertyName("LOW")]
            public int LOW { get; set; }

            [JsonPropertyName("OK")]
            public int OK { get; set; }

            [JsonPropertyName("UNKNOWN")]
            public int UNKNOWN { get; set; }

            [JsonPropertyName("CRITICAL")]
            public int CRITICAL { get; set; }
        }

        public class ComponentItem
        {
            [JsonPropertyName("componentVersion")]
            public string? ComponentVersion { get; set; }

            [JsonPropertyName("componentName")]
            public string? ComponentName { get; set; }

            [JsonPropertyName("componentVersionName")]
            public string? ComponentVersionName { get; set; }

            [JsonPropertyName("componentVersionOriginName")]
            public string? ComponentVersionOriginName { get; set; }

            [JsonPropertyName("componentVersionOriginId")]
            public string? ComponentVersionOriginId { get; set; }

            //[JsonPropertyName("license")]
            //public License? License { get; set; }

            [JsonPropertyName("vulnerabilityWithRemediation")]
            public VulnerabilityWithRemediation? VulnerabilityWithRemediation { get; set; }

            [JsonPropertyName("riskPriorityDistribution")]
            public RiskPriorityDistribution? RiskPriorityDistribution { get; set; }

            [JsonPropertyName("componentType")]
            public string? ComponentType { get; set; }

            [JsonPropertyName("_meta")]
            public Meta? Meta { get; set; }
        }

        public class ProjectVulnerabilities
        {
            [JsonPropertyName("totalCount")]
            public int TotalCount { get; set; }

            [JsonPropertyName("items")]
            public List<ComponentItem>? Items { get; set; }

            //[JsonPropertyName("appliedFilters")]
            //public List<object>? AppliedFilters { get; set; }

            [JsonPropertyName("_meta")]
            public Meta? Meta { get; set; }
        }
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
[JsonSerializable(typeof(ProjectVersions))]
internal partial class ProjectVersionsContext : JsonSerializerContext
{
}
[JsonSerializable(typeof(ProjectVulnerabilities))]
internal partial class ProjectVulnerabilitiesContext : JsonSerializerContext
{
}
