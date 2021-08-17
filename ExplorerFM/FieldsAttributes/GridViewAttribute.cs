using System;
using System.Windows.Data;

namespace ExplorerFM.FieldsAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class GridViewAttribute : Attribute
    {
        public double Priority { get; }
        public string Name { get; }
        public bool NoPath { get; }
        public IValueConverter Converter { get; }
        public object ConverterParameter { get; }

        public GridViewAttribute(string name, double priority)
            : this(name, priority, null, false, null)
        { }

        public GridViewAttribute(string name, double priority, Type converterType)
            : this(name, priority, converterType, false, null)
        { }

        public GridViewAttribute(string name, double priority, Type converterType, bool noPath)
            : this(name, priority, converterType, noPath, null)
        { }

        public GridViewAttribute(string name, double priority, Type converterType, bool noPath, object converterParameter)
        {
            if (noPath && converterType == null)
                throw new ArgumentException("Field without converter should have a path.", nameof(noPath));

            if (converterType != null && !typeof(IValueConverter).IsAssignableFrom(converterType))
                throw new ArgumentException("Converter should inherit from IValueConverter.", nameof(converterType));

            if (converterType == null && converterParameter != null)
                throw new ArgumentException("Converter parameter should be null.", nameof(converterParameter));

            Name = name;
            Priority = priority;
            NoPath = noPath;
            Converter = converterType == null
                ? null
                : Activator.CreateInstance(converterType) as IValueConverter;
            ConverterParameter = converterParameter;
        }
    }
}
