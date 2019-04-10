# Hotkeys
A helper program that can invoke global hotkeys and chords

Chords are just like what Visual Studio does. Basically, you press a global hotkey, and can then press a second key combination to invoke a chord.

Because hotkeys are global to the computer, you can't take anything that's already taken. But chords are local to the Hotkeys program, so they only need to be unique for their parent hotkey.

Hotkeys are defined by a hotkeys.xml file which should be passed to Hotkeys.exe when it is run. There's an example in this directory.
"type" attribute can be either process, shell, or chord. Use process for executables, shell for files, or chord for chords. Chord types can only be either process or shell.

"mod" attribute can contain "ctrl", "shift", "win", and "alt" to define modifiers. The Key is the same as the Windows.Forms.Keys enumeration.

The only special key that's replaced in Args is {clipboard}. All other keys are as defined in the Prompts. They don't need braces around them, they can be anything you like.

This targets .NET Framework 4.7.2.