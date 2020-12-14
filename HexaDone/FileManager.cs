using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;
using Tasklib;

namespace HexaDone
{
#nullable enable
    public class FileManager
    {
        public static bool SaveTopicColors(string filepath, Dictionary<string, HSVColor> topicColors)
        {
            string path = filepath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            try
            {


                using (StreamWriter streamWriter = new StreamWriter(filepath))
                {
                    Dictionary<string, string> topicCols = topicColors.ToDictionary((t) => { return t.Key; }, (t) => { return t.Value.ToRgbHexString(); });
                    streamWriter.Write(JsonSerializer.Serialize(topicCols));
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SaveTasks(string filepath, TaskManager taskManager)
        {
            TaskManager manager = taskManager;
            if (manager == null)
            {
                return false;
            }

            string path = filepath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Write))
                {
                    taskManager.ToJson(stream);
                    stream.Flush();
                    stream.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static Dictionary<string, HSVColor> LoadTopicColors(string filepath)
        {
            string path = filepath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new Dictionary<string, HSVColor>();
            }


            Dictionary<string, HSVColor> topicColors = new Dictionary<string, HSVColor>();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        Dictionary<string, string> colors = JsonSerializer.Deserialize<Dictionary<string, string>>(streamReader.ReadToEnd());
                        streamReader.Close();
                        topicColors = colors.ToDictionary(t => { return t.Key; }, t =>
                        {
                            Color color = (Color)ColorConverter.ConvertFromString(t.Value);
                            return HSVColor.FromRGB(color.R, color.G, color.B);
                        });
                    }
                }
                catch
                {

                }
                return topicColors;
            }

        }

        public static TaskManager? LoadTaskManager(string filepath)
        {
            string path = filepath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            TaskManager? manager = null;
            try
            {
                using (FileStream taskStream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    manager = TaskManager.FromJson(taskStream);
                    taskStream.Close();
                }
            }
            catch (Exception)
            {
            }

            return manager;
        }
    }
}
