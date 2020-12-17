# Hotkeys
A helper program that can invoke global hotkeys and chords

Chords, basically, you press a global hotkey, and can then press a second key combination to invoke a chord.

Because hotkeys are global to the computer, you can't take anything that's already taken. But chords are local to the Hotkeys program, so they only need to be unique for their parent hotkey.

You also can't define a hotkey to use F12.

# Configuration File
Hotkeys are defined by a hotkeys.cfg file which should be passed to Hotkeys.exe when it is run. There's an example in this directory.

## type
Can be any of the below
- process: Executes "path" as an executable directly.
- shell: Uses the operating system shell to open "path".
- chord: Shows a window which allows a second hotkey to be pressed. This type can only have "key" and "mods" as it doesn't execute anything by itself.

## path
A path to the file that is executed. When type is process, it should be an executable. When type is shell, it can be any file.

## args
The arguments to pass when executing. The string {clipboard} will be replaced with the text that's currently on the clipboard.

## key
The key to press to invoke the hotkey. Must be one of the keys in keycode.cfg.

## mods
Any modifier keys held while invoking the hotkey. Has to be a string, and contain any of...
- Ctrl
- Shift
- Alt
- Win