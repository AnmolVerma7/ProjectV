---
description: Update, clean, and consolidate documentation
---

# Documentation Maintenance Workflow

Use this workflow to keep `Assets/Project/Docs/` organized, accurate, and up-to-date.

---

## When to Use

- After major code changes
- When adding new systems
- Monthly maintenance
- When docs feel scattered or redundant

---

## Steps

### 1. Comprehend First

**CRITICAL**: Run `/comprehend-codebase` before touching docs!

You need to understand the current state before documenting it.

---

### 2. Check for Outdated Information

Review each doc and compare to actual code:

```bash
ls -la Assets/Project/Docs/**/*.md
```

For each guide:

- Does it match current code structure?
- Are examples still accurate?
- Are file paths correct?

---

### 3. Identify Redundancies

Look for duplicate content:

```bash
grep -r "Strategy Pattern" Assets/Project/Docs/
grep -r "Command Pattern" Assets/Project/Docs/
grep -r "Input System" Assets/Project/Docs/
```

**Ask**: Is this information duplicated across multiple docs?

---

### 4. Check Organizational Structure

Current structure should be:

```
Docs/
├── README.md (index)
├── Input/
├── Movement/
├── Design/
└── Development/
```

**Ask**: Are docs in the right folders?

---

### 5. Consolidate Duplicates

If you find overlapping content:

**Option A**: Merge into one comprehensive guide

- Keep the most complete version
- Add any unique info from others
- Delete redundant files

**Option B**: Create hierarchy with cross-links

- Keep separate but reference each other
- "See X for details on..."

---

### 6. Update Code Examples

For each code snippet in docs:

```
1. Copy the example
2. Check if it compiles in current codebase
3. Update namespaces, class names, method signatures
4. Test that it works
```

**Common issues**:

- Outdated namespace names
- Renamed methods
- Changed parameters
- Deprecated patterns

---

### 7. Update Change Logs

Each guide should have a changelog at the bottom:

```markdown
## Change Log

### [YYYY-MM-DD] Description

- Changed X
- Added Y
- Removed Z
```

Add an entry for today's changes.

---

### 8. Verify Cross-References

Check all internal links work:

```bash
grep -r "file://" Assets/Project/Docs/
grep -r "\](\.\./" Assets/Project/Docs/
```

Test each link leads to a valid location.

---

### 9. Update README Index

Ensure `Docs/README.md` lists all current guides:

```
View: Assets/Project/Docs/README.md
```

**Check**:

- All folders represented?
- Descriptions accurate?
- Links work?

---

### 10. Format for Consistency

All guides should have:

- ✅ Clear title with emoji
- ✅ "Last Updated" date
- ✅ Table of contents (if long)
- ✅ Code examples with syntax highlighting
- ✅ Sections with emoji headers
- ✅ Change log at bottom

---

### 11. Remove Legacy Content

Check for outdated docs:

```
Assets/Project/Docs/Development/LEGACY_*.md
```

**Ask**: Can this be deleted? If info is valuable, merge it elsewhere.

---

### 12. Run Format Check

// turbo

```bash
dotnet csharpier "Assets/Project/Docs" --check
```

Markdown should be clean and readable.

---

### 13. Commit Changes

Create a descriptive commit:

```bash
git add Assets/Project/Docs/
git commit -m "docs: [What you updated]

- Updated X guide with Y
- Consolidated A and B into C
- Removed outdated D
"
git push
```

---

## Checklist

Before finishing, verify:

- [ ] All guides reflect current code
- [ ] No duplicate information
- [ ] All code examples work
- [ ] Cross-references are valid
- [ ] README.md is updated
- [ ] Change logs are current
- [ ] Proper folder organization
- [ ] Changes committed to Git

---

## Common Maintenance Tasks

### Adding a New System

1. Create guide in appropriate folder
2. Add to `Docs/README.md`
3. Cross-reference from related guides

### Renaming a System

1. Update all references in docs
2. Update code examples
3. Update changelog
4. Commit with clear message

### Removing a System

1. Move guide to `Development/LEGACY_*.md`
2. Remove from `README.md`
3. Add deprecation note
4. Update dependent docs

---

**Keep docs as a living reference!** They're as important as the code. 📚
