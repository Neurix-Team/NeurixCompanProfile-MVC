using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace Neurix.Localization
{
    /// <summary>
    /// Bridges ICmsLocalizer to the IStringLocalizer that MVC's DataAnnotations
    /// localization expects, so every [Required]/[StringLength]/... ErrorMessage on the
    /// view models is translated with no changes to the models themselves.
    ///
    /// The lookup key is the English message already written in the attribute, e.g.
    /// "Arabic service name is required." An untranslated message therefore renders as
    /// the original English rather than a placeholder.
    /// </summary>
    public sealed class CmsStringLocalizerAdapter : IStringLocalizer
    {
        private readonly ICmsLocalizer _localizer;

        public CmsStringLocalizerAdapter(ICmsLocalizer localizer)
        {
            _localizer = localizer;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = _localizer[name];
                return new LocalizedString(name, value, resourceNotFound: value == name);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = _localizer.Format(name, arguments);
                return new LocalizedString(name, value, resourceNotFound: false);
            }
        }

        // MVC only ever uses the indexers for DataAnnotations messages.
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Enumerable.Empty<LocalizedString>();
    }

    /// <summary>
    /// Runs every [Display(Name = "...")] through the localizer.
    ///
    /// DataAnnotations localization covers validation messages but NOT display names, so
    /// without this the form labels would stay English while their error messages became
    /// Arabic. DisplayName is a Func evaluated at render time, so wrapping it keeps the
    /// result culture-sensitive even though ModelMetadata itself is cached once.
    /// </summary>
    public sealed class LocalizedDisplayMetadataProvider : IDisplayMetadataProvider
    {
        private readonly ICmsLocalizer _localizer;

        public LocalizedDisplayMetadataProvider(ICmsLocalizer localizer)
        {
            _localizer = localizer;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var original = context.DisplayMetadata.DisplayName;

            if (original is not null)
            {
                context.DisplayMetadata.DisplayName = () =>
                {
                    var english = original();
                    return string.IsNullOrWhiteSpace(english) ? english : _localizer[english];
                };
                return;
            }

            // No [Display(Name)] — MVC falls back to the raw property name at render time,
            // which is how most CRM form labels ("Industry", "Website", "City") are produced.
            // Localizing the property name here catches those too; the English UI is
            // unchanged because a missing key returns the key itself.
            var propertyName = context.Key.Name;

            if (context.Key.MetadataKind == ModelMetadataKind.Property &&
                !string.IsNullOrEmpty(propertyName))
            {
                context.DisplayMetadata.DisplayName = () => _localizer[propertyName];
            }
        }
    }

    /// <summary>
    /// Gives validation attributes that carry no ErrorMessage an explicit one, set to the
    /// framework's own English template.
    ///
    /// Why this is needed: MVC only routes a message through the localizer when
    /// attribute.ErrorMessage is non-empty — otherwise it calls FormatErrorMessage and the
    /// framework's English resource wins. That left three families untranslated:
    /// implicit [Required] on non-nullable types (int/Guid/non-nullable string), and the
    /// 66 bare [StringLength(n)] / [Range(a,b)] attributes in the view models.
    ///
    /// Assigning the default template as the ErrorMessage makes it a normal dictionary key,
    /// so the Arabic file can translate all of them with three entries.
    /// </summary>
    /// <summary>
    /// Supplies the framework's default message as an explicit ErrorMessage at the point
    /// the validation adapter is built.
    ///
    /// MVC only routes a message through the localizer when attribute.ErrorMessage is
    /// non-empty. Setting it from an IValidationMetadataProvider proved unreliable for the
    /// implicitly-added [Required] on non-nullable types — the attribute reaching the
    /// adapter still had no message, so "The {0} field is required." stayed English while
    /// the field name around it was Arabic. This hook runs later and always sees the
    /// attribute the adapter will actually use.
    /// </summary>
    public sealed class CmsValidationAttributeAdapterProvider
        : Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider
    {
        // The built-in implementation's method is not virtual, so wrap rather than inherit.
        private readonly Microsoft.AspNetCore.Mvc.DataAnnotations.ValidationAttributeAdapterProvider _inner = new();

        public Microsoft.AspNetCore.Mvc.DataAnnotations.IAttributeAdapter? GetAttributeAdapter(
            ValidationAttribute attribute,
            IStringLocalizer? stringLocalizer)
        {
            if (string.IsNullOrEmpty(attribute.ErrorMessage) &&
                string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
            {
                attribute.ErrorMessage = attribute switch
                {
                    RequiredAttribute => DefaultValidationMessageProvider.RequiredTemplate,
                    StringLengthAttribute { MinimumLength: > 0 } => DefaultValidationMessageProvider.MinMaxLengthTemplate,
                    StringLengthAttribute => DefaultValidationMessageProvider.MaxLengthTemplate,
                    RangeAttribute => DefaultValidationMessageProvider.RangeTemplate,
                    _ => attribute.ErrorMessage
                };
            }

            return _inner.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }

    public sealed class DefaultValidationMessageProvider : IValidationMetadataProvider
    {
        // Placeholders match what MVC's adapters pass: {0} display name, then attribute args.
        public const string RequiredTemplate = "The {0} field is required.";
        public const string MaxLengthTemplate = "The field {0} must be a string with a maximum length of {1}.";
        public const string MinMaxLengthTemplate = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";
        public const string RangeTemplate = "The field {0} must be between {1} and {2}.";

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                if (!string.IsNullOrEmpty(attribute.ErrorMessage) ||
                    !string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
                {
                    continue;   // author supplied a message; leave it as the key
                }

                attribute.ErrorMessage = attribute switch
                {
                    RequiredAttribute => RequiredTemplate,
                    StringLengthAttribute { MinimumLength: > 0 } => MinMaxLengthTemplate,
                    StringLengthAttribute => MaxLengthTemplate,
                    RangeAttribute => RangeTemplate,
                    _ => attribute.ErrorMessage
                };
            }
        }
    }
}
