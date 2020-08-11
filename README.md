# ThreadSafeRepository
Repository class is a thread-safe data manager for C#.

You can modify/undo/redo the managed data exclusively.


# Usage
```
var repos = new Repository<SampleClass>(obj);

// modify
var modifier = repos.GetModifier();
modifier.WorkingState = modifiedObj;
// commit
modifier.Commit();

// undo
bool successUndo = repos.Undo();
// redo
bool successRedo = repos.Redo();
```
