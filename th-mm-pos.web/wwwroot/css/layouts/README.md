# Responsive Layout System

This directory contains the responsive layout utilities for the TH-MM POS application redesign.

## Files

### responsive.css
Mobile-first responsive utilities including:
- **Fluid Typography Scaling**: Headings scale smoothly using `clamp()` functions
  - H1: 30px-36px
  - H2: 24px-30px
  - H3: 20px-24px
  - H4: 18px-20px
  - H5: 16px-18px
  - H6: 14px-16px
- **Display Utilities**: Show/hide elements at different breakpoints (d-xs-*, d-sm-*, d-md-*, d-lg-*, d-xl-*, d-xxl-*)
- **Touch Target Utilities**: Ensures minimum 44x44px touch targets on mobile devices
- **Responsive Spacing**: Container padding that adapts to screen size
- **Flex Utilities**: Responsive flex direction (flex-xs-*, flex-sm-*, flex-md-*, flex-lg-*)
- **Width Utilities**: Responsive width classes (w-xs-*, w-sm-*, w-md-*, w-lg-*)
- **Gap Utilities**: Responsive gap spacing for flexbox and grid
- **Text Alignment**: Responsive text alignment utilities
- **Print Styles**: Optimized styles for printing

### grid.css
Comprehensive CSS Grid system including:
- **Basic Grid Layouts**: .grid, .grid-auto-fit, .grid-auto-fill
- **Fixed Column Grids**: .grid-cols-2, .grid-cols-3, .grid-cols-4, .grid-cols-6, .grid-cols-12
- **Responsive Grid Columns**: .grid-sm-*, .grid-md-*, .grid-lg-*, .grid-xl-*
- **Column Spans**: .col-span-1 through .col-span-12, .col-span-full
- **Row Spans**: .row-span-1 through .row-span-6, .row-span-full
- **Grid Gap Utilities**: .grid-gap-*, .grid-row-gap-*, .grid-col-gap-*
- **Grid Alignment**: justify-items, align-items, justify-content, align-content, place-items
- **Grid Item Alignment**: justify-self, align-self, place-self
- **Common Layouts**: 
  - .grid-metrics (dashboard metrics)
  - .grid-products (product cards)
  - .grid-sidebar-layout (sidebar + main content)
  - .grid-two-column (equal columns)
  - .grid-three-column (three columns)
  - .grid-holy-grail (header, nav, main, aside, footer)
- **Grid Auto Flow**: .grid-flow-row, .grid-flow-col, .grid-flow-dense
- **Bootstrap Enhancements**: Gap support for Bootstrap rows, equal height columns

## Breakpoint System

The system uses Bootstrap 5's breakpoint system with a mobile-first approach:

| Breakpoint | Min Width | Description |
|------------|-----------|-------------|
| xs         | 0px       | Mobile phones (default) |
| sm         | 576px     | Large phones, small tablets |
| md         | 768px     | Tablets |
| lg         | 992px     | Desktops |
| xl         | 1200px    | Large desktops |
| xxl        | 1400px    | Extra large desktops |

## Usage Examples

### Responsive Grid Layout
```html
<!-- 1 column on mobile, 2 on tablet, 4 on desktop -->
<div class="grid-metrics">
  <div>Metric 1</div>
  <div>Metric 2</div>
  <div>Metric 3</div>
  <div>Metric 4</div>
</div>
```

### Auto-fit Grid (No Media Queries)
```html
<!-- Automatically fits columns based on available space -->
<div class="grid-auto-fit">
  <div>Item 1</div>
  <div>Item 2</div>
  <div>Item 3</div>
</div>
```

### Responsive Display
```html
<!-- Hidden on desktop, visible on mobile/tablet -->
<div class="d-lg-none">Mobile/Tablet Only</div>

<!-- Visible only on desktop -->
<div class="d-none d-lg-block">Desktop Only</div>
```

### Responsive Flex Direction
```html
<!-- Column on mobile, row on desktop -->
<div class="d-flex flex-xs-column flex-lg-row gap-4">
  <div>Item 1</div>
  <div>Item 2</div>
</div>
```

### Column Spans
```html
<div class="grid-cols-12">
  <!-- Full width on mobile, half on tablet, third on desktop -->
  <div class="col-span-12 col-md-span-6 col-lg-span-4">Content</div>
</div>
```

### Sidebar Layout
```html
<!-- Stacked on mobile, side-by-side on desktop -->
<div class="grid-sidebar-layout">
  <aside>Sidebar (250px on desktop)</aside>
  <main>Main Content (flexible)</main>
</div>
```

## Testing

A comprehensive test file is available at `test-responsive.html` that demonstrates:
1. Fluid typography scaling
2. Responsive grid layouts
3. Auto-fit grids
4. Display utilities
5. Flex direction changes
6. Column spans
7. Touch target sizing
8. Responsive spacing
9. Two-column layouts
10. Sidebar layouts

Open `test-responsive.html` in a browser and resize the window to see all responsive behaviors in action.

## Requirements Validation

This implementation satisfies the following requirements:

- **Requirement 2.1**: Mobile-first media queries with breakpoints at 576px, 768px, 992px, 1200px, 1400px ✅
- **Requirement 2.2**: Tablet-optimized layout (576-768px) with appropriate spacing ✅
- **Requirement 2.3**: Desktop layout with sidebar navigation (768-992px) ✅
- **Requirement 2.4**: Full desktop layout (>992px) with expanded navigation ✅
- **Requirement 2.5**: Fluid typography that scales proportionally with viewport size ✅
- **Requirement 2.6**: Touch targets minimum 44x44 pixels on mobile devices ✅

## Integration

These files are designed to work with the design system tokens:
- `../design-system/tokens.css` - Core design tokens
- `../design-system/colors.css` - Color palette
- `../design-system/typography.css` - Typography system
- `../design-system/spacing.css` - Spacing scale

Load order in your HTML:
```html
<link rel="stylesheet" href="/css/design-system/tokens.css">
<link rel="stylesheet" href="/css/design-system/colors.css">
<link rel="stylesheet" href="/css/design-system/typography.css">
<link rel="stylesheet" href="/css/design-system/spacing.css">
<link rel="stylesheet" href="/css/layouts/responsive.css">
<link rel="stylesheet" href="/css/layouts/grid.css">
```

## Browser Support

- Modern browsers (Chrome, Firefox, Safari, Edge)
- CSS Grid support required
- CSS Custom Properties (CSS Variables) support required
- Flexbox support required

## Notes

- All spacing uses CSS custom properties from the spacing scale
- Fluid typography uses `clamp()` for smooth scaling
- Touch targets automatically adjust on mobile viewports
- Print styles hide navigation and optimize for printing
- Reduced motion preferences are respected (see tokens.css)
