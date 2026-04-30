# RandomMemory

Source Generator based embedded typed readonly in-memory document database for .NET.

Built on top of [MasterMemory](https://github.com/Cysharp/MasterMemory) by Cysharp — adds a session/transaction API for managing database state and mutations.

---

## Overview

The database is built from typed table definitions and stored as a compact binary blob. Queries run as binary search on sorted arrays — no heap allocation per lookup. Mutations go through a transaction that is committed atomically; the database reference stays immutable between commits.

```csharp
var session = new DatabaseSession(builder.Build());

var transaction = session.BeginTransaction();
transaction.Diff(new TestDoc() { id = 1, name = "Updated" });
session.Commit();

var doc = session.Tables.TestDocTable.FindByid(1);
```

---

## Core Concepts

### Tables

Each table is defined by a plain C# class marked with `[MemoryTable]`. The Source Generator detects it and emits a fully typed table class with all query methods. Tables are read-only at runtime — all writes go through transactions.

### Session

`DatabaseSession` is the single entry point. It wraps the underlying `MemoryDatabase` and owns the transaction API. The session can be serialized to `byte[]` and restored at any point.

### Transactions

Mutations are batched in a transaction and applied on `Commit()`. Before commit, `session.Tables` still reflects the previous state. After commit, it reflects the new one. Any reference held before commit remains valid as an immutable snapshot.

---

## Usage

### Defining a table

```csharp
[MemoryTable("TestDoc"), MessagePackObject(true)]
public class TestDoc
{
    [PrimaryKey]
    public int id { get; set; }
    public string name { get; set; }
    public string lorem { get; set; }
}
```

Secondary indexes and non-unique keys:

```csharp
[SecondaryKey(0), NonUnique]
public string name { get; set; }
```

Computed properties that shouldn't be serialized:

```csharp
[IgnoreMember]
public string DisplayName => $"{id}: {name}";
```

### Building and loading

```csharp
var builder = new DatabaseBuilder();
builder.Append(new[]
{
    new TestDoc() { id = 0, name = "Doc Name", lorem = "Hello World!" },
});

var session = new DatabaseSession(builder.Build());
```

Load from file:

```csharp
var session = new DatabaseSession(File.ReadAllBytes(_filename));
```

`DatabaseSession` constructor:

```csharp
public DatabaseSession(
    byte[]? databaseBinary = null,
    bool internString = true,
    MessagePack.IFormatterResolver? formatterResolver = null,
    int maxDegreeOfParallelism = 1
)
```

### Querying

```csharp
// Exact match — throws KeyNotFoundException if not found
TestDoc doc = session.Tables.TestDocTable.FindByid(1);

// Safe lookup
if (session.Tables.TestDocTable.TryFindByid(1, out var doc)) { ... }

// Nearest value
TestDoc? closest = session.Tables.TestDocTable.FindClosestByid(5);
TestDoc? higher  = session.Tables.TestDocTable.FindClosestByid(5, selectLower: false);

// Range (inclusive)
RangeView<TestDoc> range = session.Tables.TestDocTable.FindRangeByid(1, 10);
```

`RangeView<T>` is a struct — no allocation, supports `foreach`, `Count`, `First`, `Last`, and index access.

### Full example

```csharp
var builder = new DatabaseBuilder();
builder.Append(new[]
{
    new TestDoc() { id = 0, name = "Doc Name", lorem = "Hello World!" },
});

var session = new DatabaseSession(builder.Build());

var transaction = session.BeginTransaction();
transaction.Diff(new TestDoc() { id = 999, name = "New Doc", lorem = "..." });
transaction.Diff(docs.ToArray());
transaction.RemoveTestDoc(4);
transaction.RemoveTestDoc(new[] { 1, 2, 3 });
transaction.ReplaceAll(new List<TestDoc> { new() { id = 0, name = "Replaced", lorem = "..." } });
session.Commit();

var doc = session.Tables.TestDocTable.FindByid(999);

File.WriteAllBytes(_filename, session.Serialize());
```

---

## Transactions

| Method | Description |
|---|---|
| `transaction.Diff(entity)` | Add or replace a single entity by primary key |
| `transaction.Diff(collection)` | Add or replace a collection of entities |
| `transaction.RemoveTestDoc(id)` | Remove a single entity by primary key |
| `transaction.RemoveTestDoc(ids)` | Remove multiple entities by primary key |
| `transaction.ReplaceAll(collection)` | Replace the entire table |
| `session.Commit()` | Apply all pending changes atomically |

---

## Extend Table

Generated table classes are `partial` — add methods in a separate file:

```csharp
public sealed partial class TestDocTable
{
    partial void OnAfterConstruct()
    {
        // called after the table is fully built
        // good for precomputing derived values
    }

    public IEnumerable<TestDoc> Search(string keyword)
    {
        return All.Where(x => x.lorem.Contains(keyword));
    }
}
```

---

## Validator

Implement `IValidatable<T>` to define custom validation rules:

```csharp
[MemoryTable("quest_master"), MessagePackObject(true)]
public class Quest : IValidatable<Quest>
{
    [PrimaryKey]
    public int Id { get; }
    public int RewardId { get; }
    public int Cost { get; }

    void IValidatable<Quest>.Validate(IValidator<Quest> validator)
    {
        var items = validator.GetReferenceSet<Item>();
        if (this.RewardId > 0)
            items.Exists(x => x.RewardId, x => x.ItemId);

        validator.Validate(x => x.Cost >= 10);
        validator.Validate(x => x.Cost <= 20);

        if (validator.CallOnce())
            validator.GetTableSet().Where(x => x.RewardId != 0).Unique(x => x.RewardId);
    }
}
```

```csharp
var result = db.Validate();
if (result.IsValidationFailed)
    Console.WriteLine(result.FormatFailedResults());
```

---

## Optimization

By default tables are deserialized sequentially on load. For large databases, pass `maxDegreeOfParallelism` to deserialize tables in parallel:

```csharp
var session = new DatabaseSession(bytes, maxDegreeOfParallelism: Environment.ProcessorCount);
```

Strip validator and metadata from release builds by defining:

```
DISABLE_RANDOMMEMORY_VALIDATOR
DISABLE_RANDOMMEMORY_METADATABASE
```
