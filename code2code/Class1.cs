using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace code2code
{
    public interface ICustomConvertor
    {
        Type Type { get; }
        string Convert(object value);
    }

    public static class Cd2Cd
    {
        static HashSet<Type> Primitives = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(bool),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(byte?),
            typeof(sbyte?),
            typeof(bool?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(ushort?),
            typeof(uint?),
            typeof(ulong?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?)
        };

        public static string Generate<T>(T value, IEnumerable<ICustomConvertor> customConvertors)
        {
            var convertors = customConvertors.ToDictionary(x => x.Type);
            if (!convertors.ContainsKey(typeof(string)))
                convertors.Add(typeof(string), new StringConvertor());
            if (!convertors.ContainsKey(typeof(Guid)))
                convertors.Add(typeof(Guid), new GuidConvertor());
            if (!convertors.ContainsKey(typeof(char)))
                convertors.Add(typeof(char), new CharConvertor());
            if (!convertors.ContainsKey(typeof(DateTime)))
                convertors.Add(typeof(DateTime), new DateTimeConvertor());

            return Generate(typeof(T), value, convertors);
        }
        
        static string Generate(Type type, object value, Dictionary<Type, ICustomConvertor> customConvertors)
        {
            if (value == null) return "null";
            if (customConvertors.TryGetValue(type, out var convertor))
                return convertor.Convert(value);

            if (Primitives.Contains(type))
                return value.ToString();

            var constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
                throw new Exception("Your class must hace a default constructor");

            return $"new {GetTypeName(type)}\n{{\n{GenerateProperties(type, value, customConvertors)}\n}}";
        }

        static string GenerateProperties(Type type, object value, Dictionary<Type, ICustomConvertor> customConvertors)
        {
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => (t: x.PropertyType, n: x.Name, v: x.GetMethod.Invoke(value, new object[0])))
                .Concat(type
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => (t: x.FieldType, n: x.Name, v: x.GetValue(value))));

            return props
                .Select(p => GenerateProperty(p.t, p.n, p.v, customConvertors))
                .JoinString("\n");
        }

        static string GenerateProperty(Type type, string name, object value, Dictionary<Type, ICustomConvertor> customConvertors)
        {
            return $"{name} = {Generate(type, value, customConvertors)},";
        }

        public static string GetTypeName(Type t)
        {
            if (t.DeclaringType != null && t.DeclaringType.IsGenericType)
                throw new NotImplementedException();

            var nested = t.DeclaringType != null
                ? GetTypeName(t.DeclaringType) + "."
                : "";

             //   t.IsGenericParameter

            var ns = nested == "" 
                ? t.Namespace + "."
                : "";

            var name = t.Name.Contains("`") 
                ? t.Name.Substring(0, t.Name.IndexOf("`"))
                : t.Name;

            var generics = t.IsGenericType 
                ? "<" + t.GetGenericArguments().Select(g => GetTypeName(g)).JoinString(", ") + ">"
                : "";

            return ns + nested + name + generics;

        }
    }

    class DateTimeConvertor : ICustomConvertor
    {
        public Type Type => typeof(DateTime);

        public string Convert(object value)
        {
            var dt = (DateTime)value;
            return $"new System.DateTime({dt.Ticks}L, System.DateTimeKind.{dt.Kind})";
        }
    }

    class GuidConvertor : ICustomConvertor
    {
        public Type Type => typeof(Guid);

        public string Convert(object value) => $"new System.Guid(\"{value.ToString()}\")";
    }

    class StringConvertor : ICustomConvertor
    {
        public Type Type => typeof(string);

        public string Convert(object value) => $"\"{value.ToString()}\"";
    }

    class CharConvertor : ICustomConvertor
    {
        public Type Type => typeof(char);

        public string Convert(object value) => $"'{value.ToString()}'";
    }

    public static class Utils
    {
        public static string JoinString(this IEnumerable<string> values, string separator) => string.Join(separator, values);
    }
}
