namespace TextToSpeech.Options
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConsoleParameterAttribute : Attribute
    {
        public string Name { get; }
        public string Abbreviation { get; }
        public bool IsFlag { get; }

        public ConsoleParameterAttribute(string name, string abbreviation = null, bool isFlag = false)
        {
            Name = name;
            Abbreviation = abbreviation;
            IsFlag = isFlag;
        } 
    }
}
