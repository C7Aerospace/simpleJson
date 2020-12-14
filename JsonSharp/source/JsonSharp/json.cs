using System;
using System.Collections.Generic;
namespace JsonSharp
{
    public enum JsonValueType {
        str = 1,
        number = 2,
        json = 3,
        array = 4,
        boolean = 5,
        nulltype = -1
    }
    // String Readers
    static class Reader {
        public static string Escape(string val) {
            return val
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\b", "\\b")
                .Replace("\0", "\\0")
                .Replace("\a", "\\a")
                .Replace("\f", "\\f")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v")
            ;
        }
        // Read a string type.
        public static string ReadString(string str) {
            int p = 0;
            return ReadString(str, ref p);
        }
        public static string ReadString(string str, ref int p) {
            string ret = "";
            p++;
            while(true) {
                if(str[p] == '\"') {
                    p++;
                    return ret;
                }
                if(str[p] == '\\') {
                    switch(str[p + 1]) {
                        case '\\': { ret += '\\'; break; }
                        case '\"': { ret += '\"'; break; }
                        case 'n': { ret += '\n'; break; }
                        case 'b': { ret += '\b'; break; }
                        case '0': { ret += '\0'; break; }
                        case 'a': { ret += '\a'; break; }
                        case 'f': { ret += '\f'; break; }
                        case 'r': { ret += '\r'; break; }
                        case 't': { ret += '\t'; break; }
                        case 'v': { ret += '\v'; break; }
                    }
                    p += 2;
                    continue;
                }
                ret += str[p];
                p++;
            }
        }
        // Read a string type, but return the original string
        public static string ReadRawString(string str, ref int p) {
            string ret = "\"";
            p++;
            while(true) {
                if(str[p] == '\"') {
                    ret += '\"';
                    p++;
                    return ret;
                }
                if(str[p] == '\\') {
                    ret += '\\';
                    ret += str[p + 1];;
                    p += 2;
                    continue;
                }
                ret += str[p];
                p++;
            }
        }
        // Read to next comma
        public static string ReadToComma(string str, ref int p) {
            string ret = "";
            int depth = 0;
            while(true) {
                if(str[p] == ',' && depth == 0) return ret;
                if(str[p] == '{' || str[p] == '[') depth++;
                if(str[p] == '}' || str[p] == ']') depth--;
                if(str[p] == '"') {
                    ret += Reader.ReadRawString(str, ref p);
                    continue;
                } else {
                    ret += str[p];
                    p++;
                }
            }
        }
    }
    // Value type.
    public class JsonValue {
        public JsonValueType type;
        public object value;
        public JsonValue(JsonValueType type, object value) {
            this.type = type;
            this.value = value;
        }
        public static JsonValue Parse(string json) {
            json = json.Trim();
            if(json[0] == '\"' && json[json.Length - 1] == '\"')
                return new JsonValue(JsonValueType.str, Reader.ReadString(json));
            if(json == "true" || json == "false")
                return new JsonValue(JsonValueType.boolean, bool.Parse(json));
            if(json[0] == '{' && json[json.Length - 1] == '}')
                return new JsonValue(JsonValueType.json, JsonObject.Parse(json));
            if(json[0] == '[' && json[json.Length - 1] == ']')
                return new JsonValue(JsonValueType.array, JsonArray.Parse(json));
            if(json == "null")
                return new JsonValue(JsonValueType.nulltype, null);
            decimal dem;
            if(decimal.TryParse(json, out dem))
                return new JsonValue(JsonValueType.number, dem);
            return null;
        }
        public override string ToString() {
            switch(type) {
                case JsonValueType.str:
                    return "\"" + Reader.Escape(value.ToString()) + "\"";
                case JsonValueType.nulltype:
                    return "null";
                case JsonValueType.boolean:
                    return (bool)value ? "true" : "false";
                default: return value.ToString();
            }
        }
    }
    // Array Object
    public class JsonArray {
        public List<JsonValue> array;
        public JsonArray() {
            array = new List<JsonValue>();
        }
        public JsonArray(JsonValue[] values) {
            array = new List<JsonValue>();
            foreach(JsonValue value in values)
                array.Add(value);
        }
        public static JsonArray Parse(string json) {
            json = json.Substring(1, json.Length - 2) + ",";
            JsonArray ret = new JsonArray();
            int p = 0;
            if(json == ",") return ret;
            while(p < json.Length) {
                string val = Reader.ReadToComma(json, ref p);
                ret.array.Add(JsonValue.Parse(val));
                p++;
            }
            return ret;
        }

        public override string ToString() {
            string ret = "[";
            for(int i = 0; i < array.Count; i++) {
                ret += array[i].ToString();
                if(i != array.Count - 1)
                    ret += ", ";
            }
            ret += "]";
            return ret;
        }
    }
    // JSON Object
    public class JsonObject {
        public List<KeyValuePair<string, JsonValue>> valuePairs;
        public JsonObject() {
            valuePairs = new List<KeyValuePair<string, JsonValue>>();
        }
        public JsonValue this[string index] {
            get {
                foreach(KeyValuePair<string, JsonValue> pair in valuePairs) {
                    if(pair.Key == index)
                        return pair.Value;
                }
                throw new KeyNotFoundException();
            } set {
                for(int i = 0; i < valuePairs.Count; i++)
                    if(valuePairs[i].Key == index) {
                        valuePairs[i] = new KeyValuePair<string, JsonValue>(index, value);
                    }
                throw new KeyNotFoundException();
            }
        }
        public static JsonObject Parse(string json) {
            JsonObject ret = new JsonObject();
            json = json.Replace("\n", "");
            json = json.Replace("\b", "");
            json = json.Replace("\0", "");
            json = json.Replace("\a", "");
            json = json.Replace("\f", "");
            json = json.Replace("\r", "");
            json = json.Replace("\t", "");
            json = json.Replace("\v", "");
            json = json.Substring(1, json.Length - 2).Trim() + ',';
            int p = 0;
            if(json == ",") return ret;
            while(p < json.Length) {
                while(json[p] != '\"') p++;
                string key = Reader.ReadString(json, ref p);
                while(json[p] != ':') p++; p++;
                string valString = Reader.ReadToComma(json, ref p);
                ret.valuePairs.Add(new KeyValuePair<string, JsonValue>(key, JsonValue.Parse(valString)));            
                while(json[p] != ',') p++; p++;
            }
            return ret;
        }
        public override string ToString() {
            string ret = "{";
            for(int i = 0; i < valuePairs.Count; i++) {
                ret += "\"" + valuePairs[i].Key + "\": " + valuePairs[i].Value.ToString();
                if(i != valuePairs.Count - 1)
                    ret += ", "; 
            }
            ret += "}";
            return ret;
        }
    }
}
