using System.Globalization;
using System.Text;
using CsvHelper;

namespace NsxLibraryManager.Utils;

public class CsvGenerator
{
    public static byte[] GenerateCsv<T>(List<T> data, string fileName = "export.csv")
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(data); 

        writer.Flush(); 
        memoryStream.Position = 0;

        return memoryStream.ToArray();
    }
}
