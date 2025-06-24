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

        public record MatchAmbiguity(
            [property: JsonPropertyName("alternateVersionIds")] IReadOnlyList<string> AlternateVersionIds,
            [property: JsonPropertyName("kbArtifactMatchPercentage")] double? KbArtifactMatchPercentage
        );

        public record ComponentOrigin(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("origin")] string Origin,
            [property: JsonPropertyName("externalNamespace")] string ExternalNamespace,
            [property: JsonPropertyName("externalId")] string ExternalId,
            [property: JsonPropertyName("externalNamespaceDistribution")] bool? ExternalNamespaceDistribution,
            [property: JsonPropertyName("packageUrl")] string PackageUrl,
            [property: JsonPropertyName("_meta")] Meta Meta
        );

        public record ActivityData(
            [property: JsonPropertyName("lastCommitDate")] DateTime? LastCommitDate,
            [property: JsonPropertyName("newerReleases")] int? NewerReleases,
            [property: JsonPropertyName("contributorCount12Month")] int? ContributorCount12Month,
            [property: JsonPropertyName("commitCount12Month")] int? CommitCount12Month,
            [property: JsonPropertyName("trending")] string Trending
        );

        public record Count(
            [property: JsonPropertyName("countType")] string CountType,
            [property: JsonPropertyName("count")] int? Count_
        );

        public record ActivityRiskProfile(
            [property: JsonPropertyName("counts")] IReadOnlyList<Count> Counts
        );

        public record License(
            [property: JsonPropertyName("licenseDisplay")] string LicenseDisplay,
            [property: JsonPropertyName("licenseType")] string LicenseType,
            [property: JsonPropertyName("licenses")] IReadOnlyList<License> Licenses,
            [property: JsonPropertyName("license")] string LicenseName,
            [property: JsonPropertyName("licenseFamilyName")] string LicenseFamilyName,
            [property: JsonPropertyName("ownership")] string Ownership,
            [property: JsonPropertyName("spdxId")] string SpdxId
        );

        public record LicenseRiskProfile(
            [property: JsonPropertyName("counts")] IReadOnlyList<Count> Counts
        );
        public record OperationalRiskProfile(
            [property: JsonPropertyName("counts")] IReadOnlyList<Count> Counts
        );
        public record SecurityRiskProfile(
            [property: JsonPropertyName("counts")] IReadOnlyList<Count> Counts
        );

        public record VersionRiskProfile(
            [property: JsonPropertyName("counts")] IReadOnlyList<Count> Counts
        );

        public record ComponentItem(
            [property: JsonPropertyName("componentName")] string ComponentName,
            [property: JsonPropertyName("componentVersionName")] string ComponentVersionName,
            [property: JsonPropertyName("component")] string Component,
            [property: JsonPropertyName("componentVersion")] string ComponentVersion,
            [property: JsonPropertyName("totalFileMatchCount")] int? TotalFileMatchCount,
            [property: JsonPropertyName("matchConfidence")] double? MatchConfidence,
            [property: JsonPropertyName("matchConfidenceStatus")] string MatchConfidenceStatus,
            [property: JsonPropertyName("matchAmbiguity")] MatchAmbiguity MatchAmbiguity,
            [property: JsonPropertyName("licenses")] IReadOnlyList<License> Licenses,
            [property: JsonPropertyName("origins")] IReadOnlyList<ComponentOrigin> Origins,
            [property: JsonPropertyName("usages")] IReadOnlyList<string> Usages,
            [property: JsonPropertyName("matchTypes")] IReadOnlyList<string> MatchTypes,
            [property: JsonPropertyName("inputExternalIds")] IReadOnlyList<string> InputExternalIds,
            [property: JsonPropertyName("releasedOn")] DateTime? ReleasedOn,
            [property: JsonPropertyName("licenseRiskProfile")] LicenseRiskProfile LicenseRiskProfile,
            [property: JsonPropertyName("securityRiskProfile")] SecurityRiskProfile SecurityRiskProfile,
            [property: JsonPropertyName("versionRiskProfile")] VersionRiskProfile VersionRiskProfile,
            [property: JsonPropertyName("activityRiskProfile")] ActivityRiskProfile ActivityRiskProfile,
            [property: JsonPropertyName("operationalRiskProfile")] OperationalRiskProfile OperationalRiskProfile,
            [property: JsonPropertyName("activityData")] ActivityData ActivityData,
            [property: JsonPropertyName("reviewStatus")] string ReviewStatus,
            [property: JsonPropertyName("approvalStatus")] string ApprovalStatus,
            [property: JsonPropertyName("policyStatus")] string PolicyStatus,
            [property: JsonPropertyName("componentModified")] bool? ComponentModified,
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
            [property: JsonPropertyName("links")] IReadOnlyList<Link> Links,
            [property: JsonPropertyName("href")] string Href
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
