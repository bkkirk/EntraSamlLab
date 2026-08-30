---
name: Razor component naming
description: A Blazor/Razor naming constraint that can produce confusing compiler failures.
---

In a `.razor` component, do not name a field or property the same as the component's generated type name. For example, a `Claims` property inside `Claims.razor` produces a CS0542 member-name collision.

**Why:** Razor generates a component class from the filename, so a seemingly unrelated member declaration can collide at compile time.

**How to apply:** Prefer a descriptive alternate such as `ClaimRows`, `Items`, or `Records` for component state collections.