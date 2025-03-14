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
        var dateValue = DateTime.Parse(filter.FilterValue.ToString() ?? $"{filter.Property} != null");
        return filter.FilterOperator switch
        {
            FilterOperator.LessThan => $"{filter.Property} < DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.LessThanOrEquals => $"{filter.Property} <= DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.GreaterThan => $"{filter.Property} > DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.GreaterThanOrEquals => $"{filter.Property} >= DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.Equals => $"{filter.Property} == DateTime(\"{dateValue:yyyy-MM-dd}\")",
            FilterOperator.NotEquals => $"{filter.Property} != DateTime(\"{dateValue:yyyy-MM-dd}\")",
            _ => throw new NotSupportedException($"Operator {filter.FilterOperator} not supported for dates")
        };
    }
    
    private static string HandleNullFilter(FilterDescriptor filter)
    {
        return filter.FilterOperator switch
        {
            FilterOperator.Equals => $"{filter.Property} == null",
            FilterOperator.IsNull => $"{filter.Property} == null",
            FilterOperator.NotEquals => $"{filter.Property} != null",
            FilterOperator.IsNotNull => $"{filter.Property} != null",
            _ => throw new NotSupportedException($"Operator {filter.FilterOperator} not supported for null values")
        };
    }
    
    public static string BuildFilterString(IEnumerable<FilterDescriptor> filters)
    {
        var filterExpressions = (from filter in filters
            let value = filter.FilterValue?.ToString() ?? ""
            let property = filter.Property
            select filter.FilterValue switch
            {
                null => HandleNullFilter(filter),
                DateTime dateTime => HandleDateFilter(filter),
                _ => filter.FilterOperator switch
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
                }
            }).ToList();

        // Combine all expressions with AND operator
        return string.Join(" and ", filterExpressions);
    }
}