using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tasklib
{
    public class TaskManager
    {
        private List<Task> Tasks { get; set; }

        private List<Topic> topics;

        [JsonIgnore]
        public Topic[] Topics { get { return this.topics.ToArray(); } }

        public TaskManager()
        {
            this.Tasks = new List<Task>();
            this.topics = new List<Topic>();
        }

#nullable enable
        public Task? CreateTask(string task)
        {
            string taskContent = task;
            if (string.IsNullOrWhiteSpace(taskContent))
            {
                return null;
            }

            Task t = new Task(taskContent);
            this.Tasks.Add(t);

            if (!this.topics.Contains(Topic.None))
            {
                this.topics.Add(Topic.None);
            }
            return t;
        }

        public Task? CreateTask(string task, string topic)
        {
            string taskContent = task;
            if (string.IsNullOrWhiteSpace(taskContent))
            {
                return null;
            }

            string tasktopic = topic;
            if (string.IsNullOrWhiteSpace(tasktopic))
            {
                return this.CreateTask(taskContent);
            }

            Topic? t = this.GetTopicFromName(tasktopic);
            if (t == null)
            {
                t = new Topic(tasktopic);
                this.topics.Add(t);
            }

            Task newTask = new Task(taskContent, t);
            this.Tasks.Add(newTask);
            return newTask;
        }
        private Topic? GetTopicFromName(string tasktopic)
        {
            string topic = tasktopic.Trim().ToLower();
            return this.topics.Where(t => { return t.Name.Trim().ToLower() == topic; }).FirstOrDefault();
        }

#nullable disable
        public Task[] GetOpenTasks()
        {
            return this.Tasks.Where((t) => { return !t.Done; }).ToArray();
        }

        public Task[] GetTasksByTopic(string topicname)
        {
            string topic = topicname.Trim().ToLower();

            return this.Tasks.Where(t => { return t.Topic.Trim().ToLower() == topic; }).ToArray();
        }

        public Task[] GetTasksByTopic(string topicname, DateTime date)
        {
            string topic = topicname.Trim().ToLower();
            DateTime taskdate = date;

            return this.Tasks.Where(t => { return (t.Topic.Trim().ToLower() == topic) && (t.CreationTime.Date == taskdate.Date); }).ToArray();
        }

        public Task[] GetOpenTasksByTopic(string topicname)
        {
            return GetTasksByTopic(topicname).Where(t => { return t.Done; }).ToArray();
        }

        public Topic[] GetTopicsWithOpenTasks()
        {
            return this.Tasks.Where(t => { return !t.Done; }).Select(t => { return this.GetTopicFromName(t.Topic); }).Distinct().ToArray();
        }

        public Dictionary<DateTime, Task[]> GetTasksByDate()
        {
            Dictionary<DateTime, List<Task>> tasksByDate = new Dictionary<DateTime, List<Task>>();
            foreach (Task t in this.Tasks)
            {
                DateTime date = t.CreationTime.Date;
                if (tasksByDate.ContainsKey(date))
                {
                    tasksByDate[date].Add(t);
                }
                else
                {
                    tasksByDate.Add(date, new List<Task>() { t });
                }
            }
            return tasksByDate.ToDictionary(t => { return t.Key; }, t => { return t.Value.ToArray(); });
        }

        public bool ToJson(Stream stream)
        {
            Stream jsonStream = stream;
            if (jsonStream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                if (jsonStream.CanWrite)
                {
                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                    jsonSerializerOptions.Converters.Add(new TaskManager.TaskManagerConverter());
                    jsonStream.Write(JsonSerializer.SerializeToUtf8Bytes(this, jsonSerializerOptions));
                    jsonStream.Flush();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

#nullable enable
        public static TaskManager? FromJson(Stream stream)
        {
            Stream jsonStream = stream;
            if (jsonStream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }


            TaskManager? taskManager = null;
            try
            {
                if (!stream.CanRead)
                    throw new InvalidOperationException("Can not read from stream");

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                    jsonSerializerOptions.Converters.Add(new TaskManager.TaskManagerConverter());
                    taskManager = (TaskManager)JsonSerializer.Deserialize(streamReader.ReadToEnd(), typeof(TaskManager), jsonSerializerOptions);
                    streamReader.Close();
                }
            }
            catch
            {
            }
            return taskManager;
        }

        public Dictionary<DateTime, Topic[]> GetTopicsByDate()
        {
            Dictionary<DateTime, List<Topic>> tasksByDate = new Dictionary<DateTime, List<Topic>>();
            foreach (Task t in this.Tasks)
            {
                DateTime date = t.CreationTime.Date;
                Topic topic = this.GetTopicFromName(t.Topic) ?? Topic.None;

                if (tasksByDate.ContainsKey(date))
                {
                    if (!tasksByDate[date].Contains(topic))
                    {
                        tasksByDate[date].Add(topic);
                    }
                }
                else
                {
                    tasksByDate.Add(date, new List<Topic>() { topic });
                }
            }
            return tasksByDate.ToDictionary(t => { return t.Key; }, t => { return t.Value.ToArray(); });
        }
#nullable disable

        public class TaskManagerConverter : JsonConverter<TaskManager>
        {
            public override TaskManager Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                TaskManager taskManager = new TaskManager();
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                jsonSerializerOptions.Converters.Add(new Task.TaskConverter());
                jsonSerializerOptions.Converters.Add(new Topic.TopicConverter());
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return taskManager;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName.ToLower())
                        {
                            case "tasks":
                                taskManager.Tasks = JsonSerializer.Deserialize<List<Task>>(ref reader, jsonSerializerOptions);
                                break;
                            case "topics":
                                taskManager.topics = JsonSerializer.Deserialize<List<Topic>>(ref reader, jsonSerializerOptions);
                                break;
                        }
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, TaskManager value, JsonSerializerOptions options)
            {
                TaskManager taskManager = value;
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(taskManager.Tasks));
                JsonSerializer.Serialize(writer, taskManager.Tasks);
                writer.WritePropertyName(nameof(taskManager.topics));
                JsonSerializer.Serialize(writer, taskManager.topics);
                writer.WriteEndObject();
            }
        }
    }
}
