Property Changed Generator
==============

Development tool for code generation.

This tool generates properties and automatically raise the `PropertyChanged` or `PropertyChanging` events.

## How to setup:

1. Add a package as a reference.
2. Adjust your editor config if needed.
3. It's ready to use, implement specific `interface` and declare your `backing fields`, generator will do all the dirty work.

## Available editor config settings:

- `indent_style` - solution identation style.

    > Available values is: `tab`, `space`.
    >
    > Default is `space`.
 
- `indent_size` - used to define indentation spaces amount when using `space` indent style.

    > Default is `4`.

- `property_changed_backing_fields_convention` - identify the backing fields naming convention.

    > Available values is: `CamelCase`, `PascalCase`.
    >
    > Default is `CamelCase`.

- `property_changed_backing_fields_underscore` - should use underscore during processing backing fields.

    > Default is `false`.

- `property_changed_raise_method_names` - methods, that will be used by generator to raise `PropertyChanged` event.

    > Used names by default is: `OnPropertyChanged`, `RaisePropertyChanged`.

- `property_changing_raise_method_names` - methods, that will be used by generator to raise `PropertyChanging` event.

    > Used names by default is: `OnPropertyChanging`, `RaisePropertyChanging`.
