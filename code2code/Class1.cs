using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace code2code
{
    public interface ICustomConverter
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
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(ushort?),
            typeof(uint?),
            typeof(ulong?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?)

            // note, bool is missing (has custom converter)
        };

        public static string GenerateFromJson<T>(string value, IEnumerable<ICustomConverter> customConverters = null)
        {
            return Generate(JsonConvert.DeserializeObject<T>(value), customConverters);
        }

        public static string Generate<T>(T value, IEnumerable<ICustomConverter> customConverters = null)
        {
            var converters = customConverters?.ToDictionary(x => x.Type) ?? new Dictionary<Type, ICustomConverter>();
            if (!converters.ContainsKey(typeof(string)))
                converters.Add(typeof(string), new StringConverter());
            if (!converters.ContainsKey(typeof(Guid)))
                converters.Add(typeof(Guid), new GuidConverter());
            if (!converters.ContainsKey(typeof(char)))
                converters.Add(typeof(char), new CharConverter());
            if (!converters.ContainsKey(typeof(DateTime)))
                converters.Add(typeof(DateTime), new DateTimeConverter());
            if (!converters.ContainsKey(typeof(bool)))
                converters.Add(typeof(bool), new BoolConverter());
            if (!converters.ContainsKey(typeof(bool?)))
                converters.Add(typeof(bool?), new NullableBoolConverter());


            return Generate(value, converters, new Dictionary<Type, Type>());
        }
        
        static string Generate(object value, Dictionary<Type, ICustomConverter> customConverters, Dictionary<Type, Type> genericTypeParams)
        {
            if (value == null) return "null";
            var type = value.GetType();

            if (type.IsEnum)
            {
                value = Convert.ChangeType(value, typeof(int));
                return $"({GetTypeName(type, genericTypeParams)})" + Generate(value, customConverters, genericTypeParams);
            }

            if (customConverters.TryGetValue(type, out var converter))
                return converter.Convert(value);

            if (Primitives.Contains(type))
                return value.ToString();

            if (!type.IsArray)
            {
                var constructor = type.GetConstructor(new Type[0]);
                if (constructor == null)
                    throw new Exception($"Your class must have a default constructor: {type}. You probably need to add a custom converter.");
            }

            return $"new {GetTypeName(type, genericTypeParams)}\n{{\n{GenerateProperties(value, customConverters, genericTypeParams)}\n}}";
        }

        static string GenerateProperties(object value, Dictionary<Type, ICustomConverter> customConverters, Dictionary<Type, Type> genericTypeParams)
        {
            if (value is IEnumerable values)
                return GenerateValues(values, customConverters,  genericTypeParams).JoinString("\n,");

            var type = value.GetType();
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => (n: x.Name, v: x.GetMethod.Invoke(value, new object[0])))
                .Concat(type
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => (n: x.Name, v: x.GetValue(value))));

            return props
                .Select(p => GenerateProperty(p.n, p.v, customConverters, genericTypeParams))
                .JoinString(",\n");
        }

        static IEnumerable<string> GenerateValues(IEnumerable values, Dictionary<Type, ICustomConverter> customConverters, Dictionary<Type, Type> genericTypeParams)
        {
            foreach (var v in values)
                yield return Generate(v, customConverters, genericTypeParams);
        }

        static string GenerateProperty(string name, object value, Dictionary<Type, ICustomConverter> customConverters, Dictionary<Type, Type> genericTypeParams)
        {
            return $"{name} = {Generate(value, customConverters, genericTypeParams)}";
        }

        public static string GetTypeName(Type t) => GetTypeName(t, new Dictionary<Type, Type>());

        static string GetTypeName(Type t, Dictionary<Type, Type> genericTypeParams)
        {
            // TODO: is generic stuff even needed?

            if (t.IsGenericParameter)
            {
                if (!genericTypeParams.TryGetValue(t, out t))
                    throw new InvalidOperationException("Cannot find generic type param.");
            }

            var outerGenerics = 0;
            if (t.IsConstructedGenericType)
            {
                // combine each generic type with its concrete type
                var gens = t.GetGenericTypeDefinition()
                    .GetGenericArguments()
                    .Zip(t.GetGenericArguments(), (x, y) => (x: x, y: y))
                    .ToArray();

                // add to map
                foreach (var g in gens)
                {
                    if (!genericTypeParams.ContainsKey(g.x))
                        genericTypeParams.Add(g.x, g.y);
                }

                // generics of an outer class will be added
                // to the nested type. Add maps for this situation
                if (t.DeclaringType != null
                    && t.DeclaringType.IsGenericType)
                {
                    var outerGens = t.DeclaringType.GetGenericArguments();
                    outerGenerics = outerGens.Length;
                    for (var i = 0; i < outerGenerics; i++)
                    {
                        if (!genericTypeParams.ContainsKey(outerGens[i]))
                            genericTypeParams.Add(outerGens[i], gens[i].y);
                    }
                }
            }
            else if (t.IsGenericTypeDefinition)
            {
                var gens = t
                    .GetGenericArguments()
                    .Select(genericTypeParams.FindNonGeneric);
                    
                t = t.MakeGenericType(gens.ToArray());
            }

            if (t.DeclaringType != null 
                && t.DeclaringType.DeclaringType != null)
                throw new Exception("Type nesting is only supported for 2 levels");

            var nested = t.DeclaringType != null
                ? GetTypeName(t.DeclaringType, genericTypeParams) + "."
                : "";

            var ns = nested == "" 
                ? t.Namespace + "."
                : "";

            var name = t.Name.Contains("`") 
                ? t.Name.Substring(0, t.Name.IndexOf("`"))
                : t.Name;

            var generics = t.IsGenericType 
                ? "<" + t.GetGenericArguments()
                    // generics of an outer class will be added
                    // to the nested type. Strip them out
                    .Skip(outerGenerics)
                    .Select(g => GetTypeName(g, genericTypeParams))
                    .JoinString(", ") + ">"
                : "";

            return ns + nested + name + generics;
        }
    }

    class DateTimeConverter : ICustomConverter
    {
        public Type Type => typeof(DateTime);

        public string Convert(object value)
        {
            var dt = (DateTime)value;
            return $"new System.DateTime({dt.Ticks}L, System.DateTimeKind.{dt.Kind})";
        }
    }

    class GuidConverter : ICustomConverter
    {
        public Type Type => typeof(Guid);

        public string Convert(object value) => $"new System.Guid(\"{value.ToString()}\")";
    }

    class BoolConverter : ICustomConverter
    {
        public Type Type => typeof(bool);

        public string Convert(object value) => value.ToString().ToLower();
    }

    class NullableBoolConverter : ICustomConverter
    {
        public Type Type => typeof(bool?);

        public string Convert(object value) => value?.ToString().ToLower() ?? "null";
    }

    class StringConverter : ICustomConverter
    {
        public Type Type => typeof(string);

        public string Convert(object value) => $"\"{value.ToString()}\"";
    }

    class CharConverter : ICustomConverter
    {
        public Type Type => typeof(char);

        public string Convert(object value) => $"'{value.ToString()}'";
    }

    public static class Utils
    {
        public static string JoinString(this IEnumerable<string> values, string separator) => string.Join(separator, values);
        
        public static Type FindNonGeneric(this Dictionary<Type, Type> genericTypeParams, Type t)
        {
            if (!t.IsGenericParameter) return t;

            if (genericTypeParams.TryGetValue(t, out var t1))
            {
                try
                {
                    return genericTypeParams.FindNonGeneric(t1);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + $" <= {t}", e);
                }
            }

            throw new Exception($"Cannot find type for generic {t}");
        }
    }
}
