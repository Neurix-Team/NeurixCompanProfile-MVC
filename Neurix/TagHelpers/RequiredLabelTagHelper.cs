using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Neurix.TagHelpers
{
    /// <summary>
    /// Appends a red asterisk to every <c>&lt;label asp-for="..."&gt;</c> whose bound
    /// property is required, so the cue is driven by model metadata instead of being
    /// hand-written per view. Any form added later inherits it automatically.
    ///
    /// "Required" here is deliberately the same condition MVC uses to emit
    /// <c>data-val-required</c> — <see cref="ModelMetadata.IsRequired"/> — so the visual
    /// cue can never drift from what validation actually enforces. That covers explicit
    /// [Required] attributes, non-nullable value types (int, Guid) and, because the
    /// project enables nullable reference types, non-nullable strings.
    ///
    /// Booleans are excluded: they render as checkboxes with a hidden false companion,
    /// so a value is always posted and the requirement can never fail. Marking
    /// "Published" as required would be misleading.
    /// </summary>
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class RequiredLabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; } = default!;

        // The built-in LabelTagHelper runs at the default order and fills the label text
        // from [Display(Name = ...)]. Running after it means the asterisk is appended to
        // whatever text ended up there, including labels with hand-written content.
        public override int Order => 100;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For?.Metadata is null || !IsRequired(For.Metadata))
            {
                return;
            }

            output.PostContent.AppendHtml(
                "<span class=\"required-indicator\" aria-hidden=\"true\" title=\"Required\">*</span>");

            // Screen readers get the requirement from the input's aria-required /
            // validation attributes, so the glyph itself stays hidden from them.
        }

        private static bool IsRequired(ModelMetadata metadata)
        {
            if (!metadata.IsRequired)
            {
                return false;
            }

            var type = metadata.UnderlyingOrModelType;
            return type != typeof(bool);
        }
    }
}
