using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace BootNext
{
    public class ConfigParser
    {
        private static readonly Regex SectionRegex = new Regex(@"^\[(.*)\]$");
        private static readonly Regex KeyValueRegex = new Regex(@"^([^=]*)=(.*)$");
        private static readonly Regex BootSectionRegex = new Regex(@"^B:(.*)$");

        public static Config Parse(string configPath)
        {
            var parser = new ConfigParser();
            return parser.ParseFile(configPath);
        }

        private readonly Config _config;
        private string? _currentBootSection;
        private string? _section;

        public ConfigParser()
        {
            _config = new Config();
        }
        
        private static string TrimComment(string line)
        {
            var start = line.IndexOf('#');
            return start == -1 ? line : line.Substring(0, start);
        }

        public void HandleSection(string section)
        {
            _currentBootSection = null;
            
            var match = BootSectionRegex.Match(section);
            if (!match.Success) return;

            var bootSectionName = match.Groups[1].Value.Trim();
            if (_config.BootEntries.ContainsKey(bootSectionName))
            {
                Debug.WriteLine($"Warning: Duplicate section key [B:{bootSectionName}]");
            }
            else
            {
                _config.BootEntries[bootSectionName] = new BootSection(bootSectionName);
            }

            _currentBootSection = bootSectionName;
        }

        public void HandleKeyValue(string key, string value)
        {
            if (_currentBootSection == null) return;

            var bootEntry = _config.BootEntries[_currentBootSection];

            switch (key)
            {
                case "Title":
                    bootEntry.Title = value;
                    break;
                
                case "Icon":
                    bootEntry.Icon = value;
                    break;
            }

            _config.BootEntries[_currentBootSection] = bootEntry;
        }
        
        public Config ParseFile(string filePath)
        {
            var file = new StreamReader(filePath);
            string? line;
            while ((line = file.ReadLine()) != null)
            {
                line = TrimComment(line);
                line = line.Trim();
                
                if (line.Length == 0) continue;

                Match match;
                if ((match = SectionRegex.Match(line)).Success)
                {
                    var section = match.Groups[1].Value.Trim();
                    HandleSection(section);
                    _section = section;
                } else if ((match = KeyValueRegex.Match(line)).Success)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    if (_section == null)
                    {
                        Debug.WriteLine($"Warning: Key/Value pair in unknown section: {key}");
                        continue;
                    }
                    
                    Debug.WriteLine($"[{_section}] {key}: {value}");
                    HandleKeyValue(key, value);
                }
                else
                {
                    Debug.WriteLine($"Warning: Unknown line encountered: {line}");
                }
            }
            
            return _config;
        }
    }
}