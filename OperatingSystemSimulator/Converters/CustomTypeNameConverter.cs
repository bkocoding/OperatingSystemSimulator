using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OperatingSystemSimulator.Converters;
public class CustomTypeNameConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        JObject obj = JObject.FromObject(value, new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        // Ana nesne için $type değerini değiştir
        ReplaceType(obj);

        // Alt nesneleri işlemek için MemoryDump gibi alanları kontrol et
        if (obj["MemoryDump"] is JArray memoryDumpArray)
        {
            foreach (var item in memoryDumpArray)
            {
                if (item is JObject processObj)
                {
                    ReplaceType(processObj);
                }
            }
        }

        obj.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        // Ana nesne için $type değerini geri değiştir
        RestoreType(obj);

        // Alt nesneleri işlemek için MemoryDump gibi alanları kontrol et
        if (obj["MemoryDump"] is JArray memoryDumpArray)
        {
            foreach (var item in memoryDumpArray)
            {
                if (item is JObject processObj)
                {
                    RestoreType(processObj);
                }
            }
        }

        return obj.ToObject(objectType, new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
    }

    private void ReplaceType(JObject obj)
    {
        if (obj["$type"] != null)
        {
            string originalType = obj["$type"]!.ToString();
            if (originalType.Contains("BugCheckFile"))
                obj["$type"] = "BugCheck";
            else if (originalType.Contains("ProcessBlock"))
                obj["$type"] = "Process";
        }
    }

    private void RestoreType(JObject obj)
    {
        if (obj["$type"] != null)
        {
            string modifiedType = obj["$type"]!.ToString();
            if (modifiedType == "BugCheck")
                obj["$type"] = "OperatingSystemSimulator.Models.BugCheckFile, OperatingSystemSimulator";
            else if (modifiedType == "Process")
                obj["$type"] = "OperatingSystemSimulator.ProcessHelper.ProcessBlock, OperatingSystemSimulator";
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}
