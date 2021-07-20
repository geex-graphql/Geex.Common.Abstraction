using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Converters;
using System.Text.Unicode;
using Geex.Common.Abstractions;
using Geex.Common.Json;

// ReSharper disable once CheckNamespace
namespace System.Text.Json
{
    public static class Json
    {
        public static void WriteRaw(this Utf8JsonWriter writer, string jsonRaw)
        {
            using JsonDocument document = JsonDocument.Parse(jsonRaw);
            document.RootElement.WriteTo(writer);
        }
        static Json()
        {
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRange(UnicodeRanges.All);
            DefaultSerializeSettings.Encoder = JavaScriptEncoder.Create(encoderSettings);
            DefaultSerializeSettings.PropertyNameCaseInsensitive = true;
            DefaultSerializeSettings.Converters.Add(new JsonStringEnumConverter());
            DefaultSerializeSettings.Converters.Add(new EnumerationConverter());
            DefaultSerializeSettings.Converters.Add(new DynamicJsonConverter());
            DefaultSerializeSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        public static JsonSerializerOptions DefaultSerializeSettings { get; set; } = new();

        public static string ToJsonSafe<T>(this T @this)
        {
            try
            {
                return @this.ToJson();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string ToJson<T>(this T @this)
        {
            return JsonSerializer.Serialize(@this, DefaultSerializeSettings);
        }

        public static T? ToObjectSafe<T>(this string @this)
        {
            try
            {
                return @this.ToObject<T>();
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public static T? ToObject<T>(this string @this)
        {
            return JsonSerializer.Deserialize<T>(@this, DefaultSerializeSettings);
        }
    }

    public class EnumerationConverter<T> : JsonConverter<T> where T : class
    {
        private static Type classEnumRealType = typeof(T).GetClassEnumRealType();
        private static Type classEnumValueType = typeof(T).GetClassEnumValueType();
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var data = reader.GetString();
            return typeof(Enumeration<,>).MakeGenericType(classEnumRealType, classEnumValueType).GetMethod(nameof(Enumeration.FromValue), types: new[] { classEnumValueType })?.Invoke(null, new[] { data }) as T;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var data = value.ToString();
            writer.WriteStringValue(data);
        }
    }
}
