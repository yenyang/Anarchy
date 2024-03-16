using System.Text.Encodings.Web;
using System.Text.Json;

using Colossal;
using Anarchy.Settings;

namespace Anarchy.LocaleGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var setting = new AnarchyModSettings(new AnarchyMod());
            var locale = new LocaleEN(setting);
            var e = new Dictionary<string, string>(
                locale.ReadEntries(new List<IDictionaryEntryError>(), new Dictionary<string, int>()));
            Console.WriteLine(JsonSerializer.Serialize(e, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }
    }
}