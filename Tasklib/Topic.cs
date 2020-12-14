using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tasklib
{
    public class Topic
    {
        public static readonly Topic None = new Topic(string.Empty);

        private List<string> comments;

        public string Name { get; private set; }

        [JsonIgnore]
        public string[] Comments { get { return this.comments.ToArray(); } }

        public Topic(string name)
        {
            this.Name = name.Trim() ?? string.Empty;
            this.comments = new List<string>();
        }

        public void AddComment(string comment)
        {
            this.comments.Add(comment);
        }

        public override bool Equals(object obj)
        {
            Topic topic = obj as Topic;
            if (topic != null)
            {
                return topic.Name == this.Name;
            }

            string topicname = obj as string;
            if (topicname != null)
            {
                return topicname.Trim().ToLower() == this.Name.Trim().ToLower();
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public class TopicConverter : JsonConverter<Topic>
        {
            public override Topic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                Topic topic = new Topic(string.Empty);
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (string.IsNullOrWhiteSpace(topic.Name))
                        {
                            return Topic.None;
                        }
                        else
                        {
                            return topic;
                        }
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName.ToLower())
                        {
                            case "comments":
                                topic.comments = JsonSerializer.Deserialize<List<string>>(ref reader);
                                break;
                            case "name":
                                topic.Name = reader.GetString();
                                break;
                        }
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Topic value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value);
            }
        }
    }
}
