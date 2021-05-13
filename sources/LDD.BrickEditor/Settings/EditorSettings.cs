using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace LDD.BrickEditor.Settings
{
    public class EditorSettings : ISettingsClass
    {
        [JsonProperty("workspaceFolder")]
        public string ProjectWorkspace { get; set; }

        [JsonProperty("backupInterval")]
        public int BackupInterval { get; set; } = -1;
        //[JsonProperty("backupInterval")]
        //public bool AutoSaveBackup { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("defaultFlexAttributes")]
        public double[] DefaultFlexAttributes { get; set; }

        public void InitializeDefaults()
        {
            if (BackupInterval < 0)
                BackupInterval = 60;
            if (DefaultFlexAttributes == null)
                DefaultFlexAttributes = new double[] { -0.06, 0.06, 20, 10, 10 };
            if (string.IsNullOrEmpty(Language))
                Language = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
        }

        public bool ShouldSerializeLanguage()
        {
            if (string.IsNullOrEmpty(Language))
                return false;

            return !string.Equals(Language, CultureInfo.InstalledUICulture.TwoLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
