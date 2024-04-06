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
            var str = JsonSerializer.Serialize(e, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText("C:\\Users\\TJ\\source\\repos\\Anarchy\\Anarchy\\UI\\src\\lang\\en-US.json", str);


            /*
            var file = "C:\\Users\\TJ\\source\\repos\\Anarchy\\Anarchy\\l10n\\l10n.csv";
            if (File.Exists(file))
            {
                var fileLines = File.ReadAllLines(file).Select(x => x.Split('\t'));
                Console.Write("file exists");
                string[] languages = { "de-DE", "es-ES", "fr-FR", "it-IT", "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-HANS", "zh-HANT" };
                foreach (string lang in languages)
                {
                    var valueColumn = Array.IndexOf(fileLines.First(), lang);
                    if (valueColumn > 0)
                    {
                        var e = new Dictionary<string, string>();
                        IDictionary<string, string?> f = fileLines.Skip(1).ToDictionary(x => x[0], x => x.ElementAtOrDefault(valueColumn));
                        e = f as Dictionary<string, string>;
                        var str = JsonSerializer.Serialize(e, new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });

                        File.WriteAllText($"C:\\Users\\TJ\\source\\repos\\Anarchy\\Anarchy\\lang\\{lang}.json", str);
                        Console.Write(lang.ToString());
                    }
                }
            }
            */
        }
    }
}