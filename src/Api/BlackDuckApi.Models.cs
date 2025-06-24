using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackDuckReport.GitHubAction.Api;

public partial class BlackDuckApi
{
    public static class Models
    {
        public struct Vulnerabilities
        {
            public int Critical;
            public int High;
            public int Medium;
            public int Low;
            public int Count;
        }

        public class Component
        {
            public string? Name { get; }
            public string? Version { get; }
            public string? Id { get; }
            public Vulnerabilities Vulnerabilities { get; }
            public string? MatchType { get; }

            internal Component(Json.ComponentItem component)
            {
                ArgumentNullException.ThrowIfNull(component);

                Name = component.ComponentName;
                Version = component.ComponentVersionName;
                Id = Name + ":" + Version;
                Vulnerabilities = new Vulnerabilities()
                {
                    Critical = component.SecurityRiskProfile?.Counts?.FirstOrDefault(c => "CRITICAL".Equals(c.CountType, StringComparison.OrdinalIgnoreCase))?.Count_ ?? 0,
                    High = component.SecurityRiskProfile?.Counts?.FirstOrDefault(c => "HIGH".Equals(c.CountType, StringComparison.OrdinalIgnoreCase))?.Count_ ?? 0,
                    Medium = component.SecurityRiskProfile?.Counts?.FirstOrDefault(c => "MEDIUM".Equals(c.CountType, StringComparison.OrdinalIgnoreCase))?.Count_ ?? 0,
                    Low = component.SecurityRiskProfile?.Counts?.FirstOrDefault(c => "LOW".Equals(c.CountType, StringComparison.OrdinalIgnoreCase))?.Count_ ?? 0,
                    Count = component.SecurityRiskProfile?.Counts?.Where(c => !"OK".Equals(c.CountType, StringComparison.OrdinalIgnoreCase)).Sum(v => v.Count_) ?? 0,
                };
                MatchType = component.MatchTypes?.Count > 0 ? component.MatchTypes[0] : "";
            }
        }

        [DebuggerDisplay("{Name} ({Version})")]
        public class Project
        {
            public string? Name { get; }
            public string? Version { get; }
            public DateTime? LastUpdatedAt { get; }
            public IReadOnlyList<Component> Components { get; }
            public Vulnerabilities Vulnerabilities { get; }
            public IEnumerable<Component> ComponentsWithCritical => [.. Components.Where(c => c.Vulnerabilities.Critical != 0)];
            public IEnumerable<Component> ComponentsWithHigh => [.. Components.Where(c => c.Vulnerabilities.Critical == 0 && c.Vulnerabilities.High != 0)];
            public IEnumerable<Component> ComponentsWithMedium => [.. Components.Where(c => c.Vulnerabilities.Critical == 0 && c.Vulnerabilities.High == 0 && c.Vulnerabilities.Medium != 0)];
            public IEnumerable<Component> ComponentsWithLow => [.. Components.Where(c => c.Vulnerabilities.Critical == 0 && c.Vulnerabilities.High == 0 && c.Vulnerabilities.Medium == 0 && c.Vulnerabilities.Low != 0)];
            public IEnumerable<Component> DirectDependencies => [.. Components.Where(c => c.MatchType?.Contains("DIRECT", StringComparison.OrdinalIgnoreCase) == true)];

            internal Project(Json.ProjectItem project, IReadOnlyList<Json.ComponentItem> components)
            {
                ArgumentNullException.ThrowIfNull(project);
                ArgumentNullException.ThrowIfNull(components);

                Name = project.ProjectName;
                Version = project.VersionName;
                LastUpdatedAt = project.LastUpdatedAt?.ToLocalTime();
                Components = [.. components.Select(c => new Component(c))];
                Vulnerabilities = new Vulnerabilities()
                {
                    Critical = project.RiskProfile.Categories.VULNERABILITY.CRITICAL ?? 0,
                    High = project.RiskProfile.Categories.VULNERABILITY.HIGH ?? 0,
                    Medium = project.RiskProfile.Categories.VULNERABILITY.MEDIUM ?? 0,
                    Low = project.RiskProfile.Categories.VULNERABILITY.LOW ?? 0,
                };
            }
        }
    }
}
