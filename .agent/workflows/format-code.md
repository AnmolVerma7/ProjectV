---
description: How to format and clean up C# code following project conventions
---

# Code Formatting Workflow

This workflow helps you quickly format and clean C# code to follow project standards.

## Quick Formatting Options

### Option 1: VS Code Extensions (Recommended)

Install these extensions for automatic formatting:

1. **C# Dev Kit** (Microsoft) - Provides IntelliSense and basic formatting
2. **CSharpier** - Opinionated code formatter (like Prettier for C#)
   - Install: `dotnet tool install csharpier -g`
   - Run: `dotnet csharpier .` in project root
3. **EditorConfig** - Respects `.editorconfig` settings

### Option 2: Rider/JetBrains

If you have Rider:

- `Cmd+Option+L` (Mac) or `Ctrl+Alt+L` (Windows) to reformat file
- `Cmd+Shift+Option+L` to reformat with dialog

### Option 3: Ask Claude to Format

Simply say:

```
Format @[path/to/file.cs] following our C# conventions
```

---

## Project Naming Conventions

| Element         | Convention    | Example                           |
| --------------- | ------------- | --------------------------------- |
| Private fields  | `_camelCase`  | `private int _health;`            |
| Public fields   | `PascalCase`  | `public int Health;`              |
| Properties      | `PascalCase`  | `public int Health { get; set; }` |
| Methods         | `PascalCase`  | `public void TakeDamage()`        |
| Parameters      | `camelCase`   | `void Heal(int amount)`           |
| Local variables | `camelCase`   | `var player = GetPlayer();`       |
| Constants       | `PascalCase`  | `const float Gravity = 9.81f;`    |
| Interfaces      | `IPascalCase` | `interface IInputUser`            |

---

## Code Structure Template

```csharp
using UnityEngine;

namespace Antigravity.Feature
{
    /// <summary>
    /// Brief description of the class.
    /// </summary>
    public class MyClass : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Transform _target;

        [Header("Settings")]
        [SerializeField] private float _speed = 5f;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current speed.
        /// </summary>
        public float Speed => _speed;

        #endregion

        #region Private Fields

        private bool _isActive;

        #endregion

        #region Unity Lifecycle

        private void Awake() { }
        private void Start() { }
        private void Update() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activates the component.
        /// </summary>
        public void Activate()
        {
            _isActive = true;
        }

        #endregion

        #region Private Methods

        private void DoSomething() { }

        #endregion
    }
}
```

---

## EditorConfig (Add to Project Root)

Create `.editorconfig` in your project root for automatic settings:

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming conventions
dotnet_naming_rule.private_fields_with_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_with_underscore.style = prefix_underscore
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private
dotnet_naming_style.prefix_underscore.required_prefix = _
dotnet_naming_style.prefix_underscore.capitalization = camel_case
```

---

## Quick Commands

// turbo-all

```bash
# Format entire project with CSharpier (if installed)
dotnet csharpier "Assets/Project/Scripts"
```

---

## Checklist Before Committing

- [ ] All public members have XML documentation
- [ ] Private fields use `_camelCase`
- [ ] No magic numbers (use constants)
- [ ] Regions organize large files
- [ ] No unused `using` statements
