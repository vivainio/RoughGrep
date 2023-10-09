using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RoughGrep
{
    public class StoredSettings
    {
        public List<string> SearchHistory { get; set; } = new List<string>();
        public List<string> DirHistory { get; set; } = new List<string>();
    }

    public class SettingsStorage<T>
        where T : new()
    {
        public SettingsStorage(string dirName, string fileName)
        {
            Location = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                dirName,
                fileName
            );
        }

        public string Location;

        public void LoadAndModify(Action<T> modify)
        {
            T storedSettings;
            if (!File.Exists(Location))
            {
                storedSettings = new T();
                Directory.CreateDirectory(Path.GetDirectoryName(Location));
            }
            else
            {
                var cont = File.ReadAllBytes(Location);

                storedSettings = JsonSerializer.Deserialize<T>(cont);
            }
            modify(storedSettings);
            var newCont = JsonSerializer.SerializeToUtf8Bytes(storedSettings);
            File.WriteAllBytes(Location, newCont);
        }
    }
}
