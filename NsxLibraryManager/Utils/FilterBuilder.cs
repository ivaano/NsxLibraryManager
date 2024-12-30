using Radzen;

namespace NsxLibraryManager.Utils;

public static class FilterBuilder
{
    private static string BuildInClause(string property, string value)
    {
        var values = value.Split(',')
            .Select(v => $"\"{v.Trim()}\"");
        return $"@{property}.Contains({string.Join(",", values)})";
    }
    
    public static string BuildFilterString(IEnumerable<FilterDescriptor> filters)
    {
        var filterExpressions = (from filter in filters
            let value = filter.FilterValue?.ToString() ?? ""
            let property = filter.Property
            select filter.FilterOperator switch
            {
                FilterOperator.Equals => $"{property} == \"{value}\"",
                FilterOperator.NotEquals => $"{property} != \"{value}\"",
                FilterOperator.Contains => $"{property}.ToLower().Contains(\"{value.ToLower()}\")",
                FilterOperator.StartsWith => $"{property}.ToLower().StartsWith(\"{value.ToLower()}\")",
                FilterOperator.EndsWith => $"{property}.ToLower().EndsWith(\"{value.ToLower()}\")",
                FilterOperator.GreaterThan => $"{property} > {value}",
                FilterOperator.GreaterThanOrEquals => $"{property} >= {value}",
                FilterOperator.LessThan => $"{property} < {value}",
                FilterOperator.LessThanOrEquals => $"{property} <= {value}",
                FilterOperator.In => BuildInClause(property, value),
                _ => $"{property}.Contains(\"{value}\")" // Default to Contains
            }).ToList();

        // Combine all expressions with AND operator
        return string.Join(" and ", filterExpressions);
    }
}