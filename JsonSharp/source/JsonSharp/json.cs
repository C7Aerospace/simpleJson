﻿using System;
using System.Text;
using System.Collections.Generic;
namespace JsonSharp
{
    public enum ValueType {
        json = 0,
        array = 1,
        str = 2,
        integer = 3,
        number = 4,
        boolean = 5,
        nulltype = -1
    }
    // String Readers
    public static class Reader {
        public static bool IsSpace(char ch) {
            return (
                ch == ' ' ||
                ch == '\n' ||
                ch == '\r' ||
                ch == '\t' ||
                ch == '\f'
                );
        }
        public static void IgnoreSpaces(ref string str, ref int ptr) {
            while(IsSpace(str[ptr])) ptr++;
        }
        public static string ReadString(ref string str, ref int ptr) {
            if(str[ptr] != '\"') throw new Exception("Expected \"\"\" at the head of a string definition.");
            string ret = "";
            ptr++;
            while(str[ptr] != '\"') {
                if(str[ptr] == '\\') {
                    switch(str[ptr + 1]) {
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
                    ptr += 2;
                    continue;
                }
                ret += str[ptr];
                ptr++;
            }
            ptr++;
            return ret;
        }
        public static string ReadString(string str) {
            int ptr = 0;
            return ReadString(ref str, ref ptr);
        }
        public static string ReadToEnd(ref string str, ref int ptr) {
            string ret = "";
            while(
                ptr < str.Length &&
                str[ptr] != ',' && 
                str[ptr] != '}' && 
                str[ptr] != ']') ret += str[ptr++];
            return ret.Trim();
        }
        public static string Escape(string str) {
            string ret = "";
            foreach(char i in str) {
                switch(i) {
                        case '\\': { ret += "\\\\"; break; }
                        case '\"': { ret += "\\\""; break; }
                        case '\n': { ret += "\\n"; break; }
                        case '\b': { ret += "\\b"; break; }
                        case '\0': { ret += "\\0"; break; }
                        case '\a': { ret += "\\a"; break; }
                        case '\f': { ret += "\\f"; break; }
                        case '\r': { ret += "\\r"; break; }
                        case '\t': { ret += "\\t"; break; }
                        case '\v': { ret += "\\v"; break; }
                        default: { ret += i; break; }
                    }
            }
            return ret;
        }
    }
    public class JsonValue {
        ValueType type;
        object value;
        public JsonValue(ValueType type, object value) {
            this.type = type;
            this.value = value;
        }
        public static JsonValue Parse(ref string json, ref int ptr) {
            if(json[ptr] == '\"')
                return new JsonValue(ValueType.str, Reader.ReadString(ref json, ref ptr));
            if(json[ptr] == '{')
                return new JsonValue(ValueType.json, JsonObject.Parse(ref json, ref ptr));
            if(json[ptr] == '[')
                return new JsonValue(ValueType.array, JsonArray.Parse(ref json, ref ptr));
            string content = Reader.ReadToEnd(ref json, ref ptr);
            if(content == "null")
                return new JsonValue(ValueType.nulltype, null);
            if(content == "true" || content == "false")
                return new JsonValue(ValueType.boolean, content == "true" ? true : false);
            Int64 retInt64;
            if(!content.Contains(".") && Int64.TryParse(content, out retInt64))
                return new JsonValue(ValueType.integer, retInt64);
            else
                return new JsonValue(ValueType.number, decimal.Parse(content));
        }
        public static JsonValue Parse(string json) {
            int ptr = 0;
            return JsonValue.Parse(ref json, ref ptr);
        }
        public void RawToString(ref StringBuilder strb) {
            switch(type) {
                case ValueType.json: { ((JsonObject)value).RawToString(ref strb); break; }
                case ValueType.array: { ((JsonArray)value).RawToString(ref strb); break; }
                default: {
                    switch(type) {
                        case ValueType.nulltype: { strb.Append(strb); break;}
                        case ValueType.str: { strb.Append("\"" + Reader.Escape(value.ToString()) + "\""); break; }
                        case ValueType.boolean: { strb.Append((bool)value ? "true" : "false"); break; }
                        default: { strb.Append(value.ToString()); break; }
                    }
                    break;
                };
            }
        }
        public override string ToString() {
            StringBuilder strb = new StringBuilder();
            this.RawToString(ref strb);
            return strb.ToString();
        }
        public void Serialize(ref StringBuilder strb, string currentTab = "", string tab = "    ") {
            switch(type) {
                case ValueType.json: { ((JsonObject)value).Serialize(ref strb, currentTab, tab); break; }
                case ValueType.array: { ((JsonArray)value).Serialize(ref strb, currentTab, tab); break; }
                default: { this.RawToString(ref strb); break;}
            }
        }
        public string Serialize(string currentTab = "", string tab = "    ") {
            StringBuilder strb = new StringBuilder();
            this.RawToString(ref strb);
            return strb.ToString();
        }
    }
    public class JsonArray {
        public List<JsonValue> elements;
        public JsonArray() {
            elements = new List<JsonValue>();
        }
        public JsonValue this[int index] {
            get { return elements[index]; }
            set { elements[index] = value; }
        }
        public static JsonArray Parse(ref string json, ref int ptr) {
            JsonArray ret = new JsonArray();
            Reader.IgnoreSpaces(ref json, ref ptr);
            if(json[ptr] != '[') throw new Exception("Expected \"[\" of a array definition.");
            ptr++;
            Reader.IgnoreSpaces(ref json, ref ptr);
            if(json[ptr] == ']') { ptr++; return ret; };
            while(ptr < json.Length) {
                Reader.IgnoreSpaces(ref json, ref ptr);
                JsonValue val = JsonValue.Parse(ref json, ref ptr);
                ret.elements.Add(val);
                Reader.IgnoreSpaces(ref json, ref ptr);
                if(json[ptr] == ',') ptr++;
                else if(json[ptr] == ']') break;
                else throw new Exception("Excepted \",\" or \"}\" after a Key-Value pair.");
            }
            ptr++;
            return ret;
        }
        public static JsonArray Parse(string json) {
            int ptr = 0;
            return JsonArray.Parse(ref json, ref ptr);
        }
        public void RawToString(ref StringBuilder strb) {
            strb.Append("[");
            for(int i = 0; i < elements.Count; i++) {
                elements[i].RawToString(ref strb);
                if(i != elements.Count - 1)
                    strb.Append(", ");
            }
            strb.Append("]");
        }
        public override string ToString() {
            StringBuilder strb = new StringBuilder();
            this.RawToString(ref strb);
            return strb.ToString();
        }
        public void Serialize(ref StringBuilder strb, string currentTab = "", string tab = "    ") {
            strb.Append("[\n");
            for(int i = 0; i < elements.Count; i++) {
                strb.Append(currentTab + tab);
                elements[i].Serialize(ref strb, currentTab + tab, tab);
                if(i != elements.Count - 1)
                    strb.Append(", ");
                strb.Append('\n');
            }
            strb.Append(currentTab + "]");
        }
        public string Serialize(string currentTab = "", string tab = "    ") {
            StringBuilder strb = new StringBuilder();
            this.RawToString(ref strb);
            return strb.ToString();
        }
    }
    public class JsonObject {
        Dictionary<string, JsonValue> pairs;
        List<string> keys;
        public JsonObject() {
            pairs = new Dictionary<string, JsonValue>();
            keys = new List<string>();
        }
        public void Add(string key, JsonValue val) {
            pairs.Add(key, val);
            keys.Add(key);
        }
        public void Delete(string key) {
            pairs.Remove(key);
            keys.Remove(key);
        }
        public JsonValue this[string index] {
            get { return pairs[index]; }
            set { pairs[index] = value; }
        }
        public static JsonObject Parse(ref string json, ref int ptr) {
            JsonObject ret = new JsonObject();
            Reader.IgnoreSpaces(ref json, ref ptr);
            if(json[ptr] != '{') throw new Exception("Expected \"{\" of a JSON object definition.");
            ptr++;
            Reader.IgnoreSpaces(ref json, ref ptr);
            if(json[ptr] == '}') { ptr++; return ret; }
            while(ptr < json.Length) {
                Reader.IgnoreSpaces(ref json, ref ptr);
                string key = Reader.ReadString(ref json, ref ptr);
                Reader.IgnoreSpaces(ref json, ref ptr);
                if(json[ptr] != ':') throw new Exception("Expected \":\" after key.");
                ptr++;
                Reader.IgnoreSpaces(ref json, ref ptr);
                JsonValue val = JsonValue.Parse(ref json, ref ptr);
                ret.Add(key, val);
                Reader.IgnoreSpaces(ref json, ref ptr);
                if(json[ptr] == ',') ptr++;
                else if(json[ptr] == '}') break;
                else throw new Exception("Excepted \",\" or \"}\" after a Key-Value pair.");
            }
            ptr++;
            return ret;
        }
        public static JsonObject Parse(string json) {
            int ptr = 0;
            return JsonObject.Parse(ref json, ref ptr);
        }
        public void RawToString(ref StringBuilder strb) {
            strb.Append("{");
            for(int i = 0; i < keys.Count; i++) {
                strb.Append("\"" + Reader.Escape(keys[i]) + "\": ");
                pairs[keys[i]].RawToString(ref strb);
                if(i != keys.Count - 1)
                    strb.Append(", ");
            }
            strb.Append("}");
        }
        public override string ToString() {
            StringBuilder strb = new StringBuilder();
            this.RawToString(ref strb);
            return strb.ToString();
        }
        public void Serialize(ref StringBuilder strb, string currentTab = "", string tab = "    ") {
            strb.Append("{\n");
            for(int i = 0; i < keys.Count; i++) {
                strb.Append(currentTab + tab + "\"");
                strb.Append(Reader.Escape(keys[i]));
                strb.Append("\": ");
                pairs[keys[i]].Serialize(ref strb, currentTab + tab, tab);
                if(i != keys.Count - 1)
                    strb.Append(", ");
                strb.Append('\n');
            }
            strb.Append(currentTab + "}");
        }
        public string Serialize(string currentTab = "", string tab = "    ") {
            StringBuilder strb = new StringBuilder();
            this.Serialize(ref strb, currentTab, tab);
            return strb.ToString();
        }
    }
}
