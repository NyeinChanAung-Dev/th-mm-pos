# UI Components Documentation

This directory contains reusable UI components for the TH-MM POS application. Each component is built as a Razor partial view with a corresponding view model.

## Button Component

### Overview
The Button component provides a consistent, accessible button interface with multiple variants, sizes, and states.

### Usage

#### Basic Usage
```cshtml
@{
    var buttonModel = new ButtonViewModel
    {
        Text = "Click Me",
        Variant = "primary",
        Size = "md",
        Type = "button"
    };
}

@await Html.PartialAsync("Components/_Button", buttonModel)
```

#### With Icon
```cshtml
@{
    var buttonModel = new ButtonViewModel
    {
        Text = "Add Item",
        Variant = "primary",
        Size = "md",
        Icon = "bi bi-plus-circle"
    };
}

@await Html.PartialAsync("Components/_Button", buttonModel)
```

#### Loading State
```cshtml
@{
    var buttonModel = new ButtonViewModel
    {
        Text = "Saving...",
        Variant = "primary",
        Size = "md",
        IsLoading = true
    };
}

@await Html.PartialAsync("Components/_Button", buttonModel)
```

#### As Link
```cshtml
@{
    var buttonModel = new ButtonViewModel
    {
        Text = "Learn More",
        Variant = "outline",
        Size = "md",
        Href = "/help"
    };
}

@await Html.PartialAsync("Components/_Button", buttonModel)
```

#### With Custom Attributes
```cshtml
@{
    var buttonModel = new ButtonViewModel
    {
        Text = "Submit",
        Variant = "primary",
        Size = "md",
        Type = "submit",
        Attributes = new Dictionary<string, string>
        {
            { "data-action", "submit-form" },
            { "aria-label", "Submit the form" }
        }
    };
}

@await Html.PartialAsync("Components/_Button", buttonModel)
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | string | - | The text displayed on the button (required) |
| `Variant` | string | "primary" | Button style: `primary`, `secondary`, `success`, `danger`, `outline`, `ghost` |
| `Size` | string | "md" | Button size: `sm`, `md`, `lg` |
| `Icon` | string | null | Bootstrap icon class (e.g., "bi bi-plus-circle") |
| `Type` | string | "button" | HTML button type: `button`, `submit`, `reset` |
| `IsLoading` | bool | false | Shows loading spinner and disables button |
| `IsDisabled` | bool | false | Disables the button |
| `OnClick` | string | null | JavaScript onclick handler |
| `Href` | string | null | URL for link-style buttons (renders as `<a>` tag) |
| `CssClass` | string | null | Additional CSS classes |
| `Attributes` | Dictionary<string, string> | null | Additional HTML attributes |

### Variants

#### Primary
Default button style with primary brand color.
```cshtml
Variant = "primary"
```

#### Secondary
Secondary button style with gray color.
```cshtml
Variant = "secondary"
```

#### Success
Success button style with green color.
```cshtml
Variant = "success"
```

#### Danger
Danger button style with red color (for destructive actions).
```cshtml
Variant = "danger"
```

#### Outline
Outlined button with transparent background.
```cshtml
Variant = "outline"
```

#### Ghost
Minimal button with no border or background.
```cshtml
Variant = "ghost"
```

### Sizes

- **Small (`sm`)**: Compact button for tight spaces (min-height: 32px, 44px on mobile)
- **Medium (`md`)**: Default button size (min-height: 40px, 44px on mobile)
- **Large (`lg`)**: Prominent button for primary actions (min-height: 48px)

### States

#### Default
Normal interactive state.

#### Hover
Elevated appearance with shadow and slight upward movement.

#### Active
Pressed state with reduced shadow.

#### Disabled
Reduced opacity, non-interactive (60% opacity).

#### Loading
Shows spinning icon, disabled state, prevents multiple submissions.

### Accessibility Features

- **Keyboard Navigation**: All buttons are keyboard accessible (Tab, Enter, Space)
- **Focus Indicators**: Visible focus ring with 2px outline
- **ARIA Attributes**: Loading state includes `role="status"` and `aria-label="Loading"`
- **Touch Targets**: Minimum 44x44px on mobile devices (WCAG 2.1 Level AA)
- **Screen Reader Support**: Icons marked with `aria-hidden="true"`, text always present
- **Disabled State**: Uses proper `disabled` attribute for buttons, `aria-disabled` for links

### Responsive Behavior

- **Desktop (≥768px)**: Standard button sizes as specified
- **Mobile (<768px)**: Minimum touch target size of 44x44 pixels enforced
- **Reduced Motion**: Animations disabled when user prefers reduced motion

### Theme Support

The button component fully supports both light and dark themes:
- Colors automatically adjust based on `data-theme` attribute
- Outline and ghost variants have theme-specific styles
- Focus indicators maintain proper contrast in both themes

### Examples

#### Form Submit Button
```cshtml
@{
    var submitButton = new ButtonViewModel
    {
        Text = "Save Changes",
        Variant = "primary",
        Size = "lg",
        Type = "submit",
        Icon = "bi bi-check-circle"
    };
}

@await Html.PartialAsync("Components/_Button", submitButton)
```

#### Delete Confirmation Button
```cshtml
@{
    var deleteButton = new ButtonViewModel
    {
        Text = "Delete",
        Variant = "danger",
        Size = "md",
        Icon = "bi bi-trash",
        OnClick = "confirmDelete()"
    };
}

@await Html.PartialAsync("Components/_Button", deleteButton)
```

#### Full Width Button
```cshtml
@{
    var fullWidthButton = new ButtonViewModel
    {
        Text = "Continue",
        Variant = "primary",
        Size = "lg",
        CssClass = "btn-custom-block"
    };
}

@await Html.PartialAsync("Components/_Button", fullWidthButton)
```

### CSS Classes Reference

- `.btn-custom` - Base button class
- `.btn-custom-{variant}` - Variant styles (primary, secondary, success, danger, outline, ghost)
- `.btn-custom-{size}` - Size styles (sm, md, lg)
- `.btn-custom-loading` - Loading state
- `.btn-custom-block` - Full width button
- `.btn-group-custom` - Button group container

### Demo

A visual demo of all button variants, sizes, and states is available at:
`wwwroot/css/components/button-demo.html`

Open this file in a browser to see all button styles in action and test theme switching.

