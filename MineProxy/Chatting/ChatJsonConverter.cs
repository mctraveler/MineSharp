using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using MineProxy.Chatting;

namespace MineProxy.Chatting
{
    class ChatJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(ChatJson))
                return true;
            if (objectType == typeof(List<ChatJson>))
                return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(List<ChatJson>))
            {
                if (reader.TokenType != JsonToken.StartArray)
                    throw new InvalidOperationException();

                var l = new List<ChatJson>();
                while (true)
                {
                    if (!reader.Read())
                        break;
                    if (reader.TokenType == JsonToken.EndArray)
                        break;
                    l.Add((ChatJson)ReadJson(reader, typeof(ChatJson), null, serializer));
                }    
                return l;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var a = new ChatJson();
                a.Raw = true;
                a.Text = (string)reader.Value;
                return a;
            }

            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();
            var x = new ChatJson();
            while (true)
            {
                if (!reader.Read())
                    break;
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new NotImplementedException(reader.TokenType.ToString());

                string key = (string)reader.Value;
                if (!reader.Read())
                    break;

                switch (key)
                {
                    case "color":
                        x.Color = (string)reader.Value;
                        break;
                    case "italic":
                        x.Italic = (bool)reader.Value;
                        break;
                    case "translate":
                        x.Translate = (string)reader.Value;
                        break;
                    case "with":
                            //x.With = (List<ChatJson>)serializer.Deserialize(reader, typeof(List<ChatJson>));
                        x.With = (List<ChatJson>)ReadJson(reader, typeof(List<ChatJson>), null, serializer);
                        break;
                    case "clickEvent":
                        x.ClickEvent = (ChatEvent)serializer.Deserialize(reader, typeof(ChatEvent));
                        break;
                    case "hoverEvent":
                        x.HoverEvent = (ChatEvent)serializer.Deserialize(reader, typeof(ChatEvent));
                        break;
                    case "text":
                        x.Text = (string)reader.Value;
                        break;
                    case "extra":
                        x.Extra = (List<ChatJson>)ReadJson(reader, typeof(List<ChatJson>), null, serializer);
                        break;
                    case "insertion":
                        x.Insertion = (string)reader.Value;
                        break;
                    default:
                        throw new NotImplementedException(key);
                }
            }
            return x;
        }

        public override void WriteJson(JsonWriter w, object value, JsonSerializer serializer)
        {
            if (value is List<ChatJson>)
            {
                w.WriteStartArray();
                var a = (List<ChatJson>)value;
                foreach (var c in a)
                {
                    WriteJson(w, c, serializer);
                }
                w.WriteEndArray();
                return;
            }

            var s = (ChatJson)value;
            if (s.Raw)
            {
                w.WriteValue(s.Text);
                return;
            }
            w.WriteStartObject();
            if (s.Italic == true)
            {
                w.WritePropertyName("italic");
                w.WriteValue(s.Italic);
            }
            if (s.Color != null)
            {
                w.WritePropertyName("color");
                w.WriteValue(s.Color);
            }
            if (s.ClickEvent != null)
            {
                w.WritePropertyName("clickEvent");
                serializer.Serialize(w, s.ClickEvent);
            }
            if (s.HoverEvent != null)
            {
                w.WritePropertyName("hoverEvent");
                serializer.Serialize(w, s.HoverEvent);
            }
            if (s.Translate != null)
            {
                w.WritePropertyName("translate");
                w.WriteValue(s.Translate);
            }
            if (s.With != null)
            {
                w.WritePropertyName("with");
                //serializer.Serialize(w, s.With);
                WriteJson(w, s.With, serializer);
            }
            if (s.Extra != null)
            {
                w.WritePropertyName("extra");
                WriteJson(w, s.Extra, serializer);
            }
            if (s.Text != null)
            {
                w.WritePropertyName("text");
                w.WriteValue(s.Text);
            }
            w.WriteEndObject();
        }
    }
}

