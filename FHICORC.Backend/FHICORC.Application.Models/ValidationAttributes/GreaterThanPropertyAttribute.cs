using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FHICORC.Application.Models.ValidationAttributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class GreaterThanPropertyAttribute : ValidationAttribute
    {
        public GreaterThanPropertyAttribute(string otherProperty) : this(otherProperty, false)
        {
        }
        public GreaterThanPropertyAttribute(string otherProperty, bool bothPropertiesMustBeSpecified) : base("{0} must be greater than {1}")
        {
            OtherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
            BothPropertiesMustBeSpecified = bothPropertiesMustBeSpecified;
        }
        public bool BothPropertiesMustBeSpecified { get; set; }
        public string OtherProperty { get; }
        public string OtherPropertyDisplayName { get; internal set; }
        public override bool RequiresValidationContext => true;
        public override string FormatErrorMessage(string name) =>
            string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty);
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Property {0} doesn't exist", OtherProperty));
            }
            if (otherPropertyInfo.GetIndexParameters().Any())
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "{0}.{1} must not be an indexed property", validationContext.ObjectType.FullName, OtherProperty));
            }
            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (BothPropertiesMustBeSpecified && value == null && otherPropertyValue != null)
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "If {0} is specified, so must {1} be", OtherProperty, validationContext.DisplayName));
            }
            if (BothPropertiesMustBeSpecified && value != null && otherPropertyValue == null)
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "If {0} is specified, so must {1} be", validationContext.DisplayName, OtherProperty));
            }
            if (value != null && !(value is IComparable))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Both {0} and {1} must be IComparable", validationContext.DisplayName, OtherProperty));
            }
            if (value != null && ((IComparable)value).CompareTo(otherPropertyValue) <= 0)
            {
                if (OtherPropertyDisplayName == null)
                {
                    OtherPropertyDisplayName = GetDisplayNameForProperty(otherPropertyInfo);
                }
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }
        private string GetDisplayNameForProperty(PropertyInfo property)
        {
            var attributes = CustomAttributeExtensions.GetCustomAttributes(property, true);
            var display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (display != null)
            {
                return display.GetName();
            }
            return OtherProperty;
        }
    }
}
