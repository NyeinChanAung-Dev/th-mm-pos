namespace th_mm_pos.web.Models.Components
{
    /// <summary>
    /// View model for the Button component.
    /// Supports multiple variants, sizes, icons, and states.
    /// </summary>
    public class ButtonViewModel
    {
        /// <summary>
        /// The text displayed on the button.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// The button variant/style.
        /// Options: primary, secondary, success, danger, outline, ghost
        /// Default: primary
        /// </summary>
        public string Variant { get; set; } = "primary";

        /// <summary>
        /// The button size.
        /// Options: sm, md, lg
        /// Default: md
        /// </summary>
        public string Size { get; set; } = "md";

        /// <summary>
        /// Bootstrap icon class to display alongside text (e.g., "bi bi-plus-circle").
        /// Optional.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// The button type attribute.
        /// Options: button, submit, reset
        /// Default: button
        /// </summary>
        public string Type { get; set; } = "button";

        /// <summary>
        /// Whether the button is in a loading state.
        /// When true, displays a spinner and disables the button.
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// Whether the button is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// JavaScript onclick handler (optional).
        /// </summary>
        public string? OnClick { get; set; }

        /// <summary>
        /// Additional HTML attributes to apply to the button element.
        /// </summary>
        public Dictionary<string, string>? Attributes { get; set; }

        /// <summary>
        /// Additional CSS classes to apply to the button.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// The URL to navigate to when the button is clicked (for link-style buttons).
        /// </summary>
        public string? Href { get; set; }
    }
}
