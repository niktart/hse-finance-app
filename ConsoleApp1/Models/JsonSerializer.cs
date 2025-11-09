using System;
using System.IO;
using System.Text.Json;

public class JsonDataSerializer : DataSerializer
{
    public override void ExportData(ExportData data, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(filePath, json);
    }

    public override ExportData ImportData(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Deserialize<ExportData>(json, options);
    }

    public override string FileExtension => ".json";
}