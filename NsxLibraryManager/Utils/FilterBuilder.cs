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
    
    private static string HandleDateFilter(FilterDescriptor filter)
    {
        if (filter.FilterValue is null)
        {
            return filter.FilterOperator switch
            {
                FilterOperator.IsNotNull => $"{filter.Property} != null",
                _ => $"{filter.Property} == null"
            };
        }
        var dateValue = DateTime.Parse(filter.FilterValue.ToString());
        return filter.FilterOperator switch
        {
            FilterOperator.LessThan => $"{filter.Property} < DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.LessThanOrEquals => $"{filter.Property} <= DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.GreaterThan => $"{filter.Property} > DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.GreaterThanOrEquals => $"{filter.Property} >= DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.Equals => $"{filter.Property} == DateTime(\"{dateValue:yyyy-MM-dd}\")",
            _ => throw new NotSupportedException($"Operator {filter.FilterOperator} not supported for dates")
        };
    }
    
    public static string BuildFilterString(IEnumerable<FilterDescriptor> filters)
    {
        var filterExpressions = (from filter in filters
            let value = filter.FilterValue?.ToString() ?? ""
            let property = filter.Property
            select property is "ReleaseDate" or "ReleaseDateUtc"
                ? HandleDateFilter(filter)
                : filter.FilterOperator switch
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