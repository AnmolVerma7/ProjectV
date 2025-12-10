---
description: Comprehend codebase architecture before making changes
---

# Codebase Comprehension Workflow

Use this workflow **before** updating docs or making architectural changes to ensure you understand the current state.

## When to Use

- Before updating documentation
- Before major refactors
- When resuming work after a break
- When a new agent takes over the conversation

---

## Steps

### 1. Review Project Structure

List the main directories:

```bash
find Assets/Project/Scripts -maxdepth 2 -type d | head -20
```

**Understand**: What are the main feature folders?

---

### 2. Check Documentation Index

Read the docs index to see what's documented:

```
View: Assets/Project/Docs/README.md
```

**Understand**: What systems exist? What's the organization?

---

### 3. Identify Core Systems

Find key architecture files:

```bash
find Assets/Project/Scripts -name "*System*.cs" -o -name "*Manager*.cs" -o -name "*Controller*.cs" | head -20
```

**Understand**: What are the main coordinators?

---

### 4. Review Interfaces

Check what contracts exist:

```bash
find Assets/Project/Scripts -name "I*.cs" | grep -v ".meta"
```

**Understand**: What are the system boundaries?

---

### 5. Read Architectural Docs

View the high-level guides:

```
View: Assets/Project/Docs/Movement/MOVEMENT_SYSTEM_GUIDE.md (first 100 lines)
View: Assets/Project/Docs/Input/INPUT_SYSTEM_GUIDE.md (first 100 lines)
```

**Understand**: What patterns are in use? What's the philosophy?

---

### 6. Check Recent Changes

See what was worked on recently:

```bash
git log --oneline --graph -10
```

**Understand**: What's the current development focus?

---

### 7. Identify Dependencies

Check what external packages are used:

```
View: Packages/manifest.json
```

**Understand**: What frameworks/libraries does the project depend on?

---

### 8. Review Task Artifacts (if available)

Check brain artifacts for context:

```
View: .gemini/antigravity/brain/*/task.md
View: .gemini/antigravity/brain/*/SCALABLE_ARCHITECTURE.md
```

**Understand**: What's the roadmap? What patterns are established?

---

## Output

After completing this workflow, you should be able to answer:

- ✅ What are the main systems? (Input, Movement, Time, etc.)
- ✅ What design patterns are used? (Strategy, Command, etc.)
- ✅ Where is documentation located?
- ✅ What's currently being developed?
- ✅ What are the architectural principles?

---

## Next Steps

Once you understand the codebase:

- Update documentation with `/update-docs`
- Make informed architectural changes
- Add features that align with existing patterns
