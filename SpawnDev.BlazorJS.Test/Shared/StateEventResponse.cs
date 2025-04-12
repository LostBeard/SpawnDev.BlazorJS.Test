using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SpawnDev.BlazorJS.Test.Shared
{
    public class EventContent
    {

    }
    public class StateEventResponse : StateEvent
    {
        [JsonPropertyName("origin_server_ts")]
        public long? OriginServerTs { get; set; }

        [JsonPropertyName("room_id")]
        public string? RoomId { get; set; }

        [JsonPropertyName("sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("unsigned")]
        public JsonObject? Unsigned { get; set; }

        [JsonPropertyName("event_id")]
        public string? EventId { get; set; }

        public class UnsignedData
        {
            [JsonPropertyName("age")]
            public ulong? Age { get; set; }

            [JsonPropertyName("redacted_because")]
            public object? RedactedBecause { get; set; }

            [JsonPropertyName("transaction_id")]
            public string? TransactionId { get; set; }

            [JsonPropertyName("replaces_state")]
            public string? ReplacesState { get; set; }

            [JsonPropertyName("prev_sender")]
            public string? PrevSender { get; set; }

            [JsonPropertyName("prev_content")]
            public JsonObject? PrevContent { get; set; }
        }
    }
    public class StateEvent
    {
        public static FrozenSet<Type> KnownStateEventTypes { get; } = default;

        public static FrozenDictionary<string, Type> KnownStateEventTypesByName { get; } = default;

        public static Type GetStateEventType(string? type) => default;

        [JsonIgnore]
        public Type MappedType => GetStateEventType(Type);

        [JsonIgnore]
        public bool IsLegacyType => default;// MappedType.GetCustomAttributes<MatrixEventAttribute>().FirstOrDefault(x => x.EventName == Type)?.Legacy ?? false;

        [JsonIgnore]
        public string FriendlyTypeName => default;// MappedType.GetFriendlyNameOrNull() ?? Type;

        [JsonIgnore]
        public string FriendlyTypeNamePlural => default;// MappedType.GetFriendlyNamePluralOrNull() ?? Type;

        private static readonly JsonSerializerOptions TypedContentSerializerOptions = new()
        {
            // We need these, NumberHandling covers other number types that we don't want to convert
            Converters = {
            //new JsonFloatStringConverter(),
            //new JsonDoubleStringConverter(),
            //new JsonDecimalStringConverter()
        }
        };

        [JsonIgnore]
        [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
        public EventContent? TypedContent
        {
            get
            {
                try
                {
                    return default;
                }
                catch (JsonException e)
                {
                    Console.WriteLine(e);
                   
                }

                return null;
            }
            set
            {
                if (value is null)
                    RawContent?.Clear();
                else
                    RawContent = default;
            }
        }

        public T? ContentAs<T>()
        {
            try
            {
                return RawContent.Deserialize<T>(TypedContentSerializerOptions)!;
            }
            catch (JsonException e)
            {
                Console.WriteLine(e);
                //Console.WriteLine("Content:\n" + (RawContent?.ToJson() ?? "null"));
            }

            return default;
        }

        [JsonPropertyName("state_key")]
        public string? StateKey { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("replaces_state")]
        public string? ReplacesState { get; set; }

        private JsonObject? _rawContent;

        [JsonPropertyName("content")]
        public JsonObject? RawContent
        {
            get => _rawContent;
            set => _rawContent = value;
        }
        //
        // [JsonIgnore]
        // public new Type GetType {
        //     get {
        //         var type = GetStateEventType(Type);
        //
        //         //special handling for some types
        //         // if (type == typeof(RoomEmotesEventContent)) {
        //         //     RawContent["emote"] = RawContent["emote"]?.AsObject() ?? new JsonObject();
        //         // }
        //         //
        //         // if (this is StateEventResponse stateEventResponse) {
        //         //     if (type == null || type == typeof(object)) {
        //         //         Console.WriteLine($"Warning: unknown event type '{Type}'!");
        //         //         Console.WriteLine(RawContent.ToJson());
        //         //         Directory.CreateDirectory($"unknown_state_events/{Type}");
        //         //         File.WriteAllText($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json",
        //         //             RawContent.ToJson());
        //         //         Console.WriteLine($"Saved to unknown_state_events/{Type}/{stateEventResponse.EventId}.json");
        //         //     }
        //         //     else if (RawContent is not null && RawContent.FindExtraJsonObjectFields(type)) {
        //         //         Directory.CreateDirectory($"unknown_state_events/{Type}");
        //         //         File.WriteAllText($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json",
        //         //             RawContent.ToJson());
        //         //         Console.WriteLine($"Saved to unknown_state_events/{Type}/{stateEventResponse.EventId}.json");
        //         //     }
        //         // }
        //
        //         return type;
        //     }
        // }

        //debug
        [JsonIgnore]
        public string InternalSelfTypeName
        {
            get
            {
                var res = GetType().Name switch
                {
                    "StateEvent`1" => "StateEvent",
                    _ => GetType().Name
                };
                return res;
            }
        }

        [JsonIgnore]
        public string InternalContentTypeName => TypedContent?.GetType().Name ?? "null";
    }
}
