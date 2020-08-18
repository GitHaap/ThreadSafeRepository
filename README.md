# ThreadSafeRepository
Repository class is a thread-safe data manager for C#.

You can modify/undo/redo the managed data exclusively.


# Usage
```C#
var repos = new Repository<SampleClass>(obj);

// modify via StateModifier
var modifier = repos.GetModifier();
modifier.WorkingState = modifiedObj;

// reflect the modification
modifier.Commit();

// CurrentState is deep-copied object
var objX = repos.CurrentStateClone;

// change to previous revision
bool successUndo = repos.Undo();
// change to next revision
bool successRedo = repos.Redo();
```
