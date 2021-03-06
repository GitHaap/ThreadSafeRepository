# ThreadSafeRepository
`Repository` class is a thread-safe state manager for C#.

You can modify/undo/redo the managed state exclusively.


# Usage
```C#
var repos = new Repository<SampleClass>(obj);

// modify via StateModifier
var modifier = repos.GetModifier();
modifier.WorkingState = modifiedObj;

// reflect the modification
modifier.Commit();
// or revert
modifier.Revert();

// CurrentState is deep-copied object
var objX = repos.CurrentStateClone;

// change to previous revision
bool successUndo = repos.Undo();
// change to next revision
bool successRedo = repos.Redo();
```
