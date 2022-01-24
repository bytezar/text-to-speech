namespace TextToSpeech.Options
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class OptionsParser
    {
        private class OptionProperty
        {
            public PropertyInfo Property { get; set; }
            public ConsoleParameterAttribute Attribute { get; set; }
        }

        private static OptionProperty[] Properties { get; }

        static OptionsParser()
        {
            Properties = typeof(Options).GetProperties()
                .Select(property => new OptionProperty
                {
                    Property = property,
                    Attribute = property.GetCustomAttribute<ConsoleParameterAttribute>()
                })
                .Where(_ => _.Attribute != null)
                .ToArray();
        }

        public Options Parse(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments. First argument should be path of file.");
                return null;
            }
            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File {args[0]} argument should be path of file.");
                return null;
            }

            var options = new Options()
            {
                FilePath = Path.GetFullPath(args[0])
            };

            var argumentValues = this.GetArgumentValues(args);
            if (argumentValues == null)
            {
                return null;
            }
            
            return this.FillOptions(options, argumentValues) ? options : null;
        }

        private bool FillOptions(Options options, Dictionary<OptionProperty, string> argumentValues)
        {
            foreach (var argumentValue in argumentValues)
            {
                var property = argumentValue.Key.Property;
                var attribute = argumentValue.Key.Attribute;
                if (attribute.IsFlag)
                {
                    property.SetValue(options, true);
                    continue;
                }

                object value = argumentValue.Value;
                if (property.PropertyType == typeof(int))
                {
                    if (!int.TryParse(argumentValue.Value, out int intValue))
                    {
                        Console.WriteLine($"Argument --{attribute.Name} should be integer.");
                        return false;
                    }

                    value = intValue;
                }

                property.SetValue(options, value);
            }

            return true;
        }

        private Dictionary<OptionProperty, string> GetArgumentValues(string[] args)
        {
            var argumentValues = new Dictionary<OptionProperty, string>();
            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];

                OptionProperty optionProperty = null;
                if (arg.StartsWith("--"))
                {
                    optionProperty = Properties.FirstOrDefault(_ =>
                        string.Equals(_.Attribute.Name, arg[2..], StringComparison.OrdinalIgnoreCase));
                }
                else if (arg.StartsWith("-"))
                {
                    optionProperty = Properties.FirstOrDefault(_ =>
                        string.Equals(_.Attribute.Abbreviation, arg[1..], StringComparison.OrdinalIgnoreCase));
                }

                if (optionProperty == null)
                {
                    Console.WriteLine($"Unknown argument: {arg}");
                    return null;
                }

                var attribute = optionProperty.Attribute;
                if (argumentValues.ContainsKey(optionProperty))
                {
                    Console.WriteLine($"Argument --{attribute.Name} was defined two times.");
                    return null;
                }

                if (optionProperty.Attribute.IsFlag)
                {
                    argumentValues.Add(optionProperty, null);
                    continue;
                }

                i++;
                if (i >= args.Length)
                {
                    Console.WriteLine($"No value for argument --{attribute.Name}.");
                    return null;
                }

                argumentValues.Add(optionProperty, args[i]);
            }

            return argumentValues;
        }
    }
}
