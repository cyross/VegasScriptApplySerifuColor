using System.Collections.Generic;
using System.Drawing;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VegasScriptApplySerifuColor
{
    internal class VegasScriptSettings
    {
        public static float AudioInsertInterval;
        public static string OpenDirectory;
        public static bool IsRecursive;
        public static bool StartFrom;
        public static string[] SupportedAudioFile = null;
        public readonly static Dictionary<string, Color> TextColorByActor = new Dictionary<string, Color>();

        public static void Load()
        {
            Properties.Vegas.Default.Upgrade();
            Properties.SupportedAudioFileSettings.Default.Upgrade();
            Properties.VoiceActorColor.Default.Upgrade();

            AudioInsertInterval = Properties.Vegas.Default.audioInsertInterval;
            OpenDirectory = Properties.Vegas.Default.openDirectory;
            IsRecursive = Properties.Vegas.Default.isRecursive;
            StartFrom = Properties.Vegas.Default.startFrom;

            List<string> audioFileExts = new List<string>();
            foreach (SettingsProperty property in Properties.SupportedAudioFileSettings.Default.Properties)
            {
                PropertyInfo pinfo = typeof(Properties.SupportedAudioFileSettings).GetProperty(property.Name);
                audioFileExts.Add((string)pinfo.GetValue(Properties.SupportedAudioFileSettings.Default));
            }
            SupportedAudioFile = audioFileExts.ToArray();

            TextColorByActor.Clear();
            foreach(SettingsProperty property in Properties.VoiceActorColor.Default.Properties)
            {
                PropertyInfo pinfo = typeof(Properties.VoiceActorColor).GetProperty(property.Name);
                TextColorByActor[property.Name] = (Color)pinfo.GetValue(Properties.VoiceActorColor.Default);
            }
        }

        public static void Save()
        {
            // VoiceActorColor, SupportedAudioFileSettingはマスタ情報なので保存不要
            Properties.Vegas.Default.startFrom = StartFrom;
            Properties.Vegas.Default.isRecursive = IsRecursive;
            Properties.Vegas.Default.audioInsertInterval = AudioInsertInterval;
            Properties.Vegas.Default.openDirectory = OpenDirectory;
            Properties.Vegas.Default.Save();
        }

        public static string FormatKey(string org_key)
        {
            return Regex.Replace(org_key, @"[\s()\.\/]", "_");
        }
    }
}
