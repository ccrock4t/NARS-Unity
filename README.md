# NARS-Unity
A C# NARS for Unity game engine

Originally ported from *NARS-Python v0.4* (https://github.com/ccrock4t/NARS-Python)

## How to use in Unity

Simply attach `NARSAgent.cs` to a Unity gameobject.

Use `SendInput()` in `NARSAgent.cs` to send inputs to NARS.

Create a C# function to define NARS operations. For example:
```
    /// <summary>
    /// my motor operation
    /// </summary>
    public void MyMotorOp()
    {
        // motor operation code here
    }
```
