using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tasklib
{
    public class Task
    {
        public DateTime CreationTime { get; protected set; }

        public DateTime CompletionTime { get; protected set; }

        public string Content { get; protected set; }

        public string Topic { get; protected set; }

        public bool Done { get; protected set; }

        private Task()
        {
            this.CreationTime = DateTime.Now;
            this.CompletionTime = DateTime.MaxValue;
            this.Topic = Tasklib.Topic.None.Name;
        }

        public Task(string content) : this(content, Tasklib.Topic.None)
        {
        }

        public Task(string content, Topic topic) : this()
        {
            this.Content = content;
            if (string.IsNullOrWhiteSpace(this.Content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (topic != null)
            {
                this.Topic = topic.Name;
            }
        }

        public void SetDone()
        {
            this.Done = true;
            this.CompletionTime = DateTime.Now;
        }

        public void SetUndone()
        {
            this.Done = false;
            this.CompletionTime = DateTime.MaxValue;
        }

        public class TaskConverter : JsonConverter<Task>
        {
            public override Task Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                Task task = new Task();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (string.IsNullOrWhiteSpace(task.Content))
                        {
                            return null;
                        }
                        else
                        {
                            return task;
                        }
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName.ToLower())
                        {
                            case "creationtime":
                                task.CreationTime = reader.GetDateTime();
                                break;
                            case "completiontime":
                                task.CompletionTime = reader.GetDateTime();
                                break;
                            case "content":
                                task.Content = reader.GetString();
                                break;
                            case "topic":
                                task.Topic = reader.GetString();
                                break;
                            case "done":
                                task.Done = reader.GetBoolean();
                                break;
                        }
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Task value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value);
            }
        }
    }
}
