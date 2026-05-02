# Multiline Text Input Overhaul Walkthrough

I have successfully updated the multiline text input component in your `InteractiveCLI` application to support a robust text editing experience.

## What Was Added

1. **Arrow Navigation:**
   You can now use `[UP]`, `[DOWN]`, `[LEFT]`, and `[RIGHT]` arrows to navigate around the text area. The logic correctly handles wrapping around the beginning and end of lines and bounding the cursor horizontally when jumping between lines of different lengths.

2. **Insert and Overwrite Modes:**
   The `[INSERT]` key toggles between insert and overwrite modes.
   - We updated `IConsole` and `SystemConsole` to expose the `CursorSize` property.
   - When in **Insert mode**, the cursor size is set to `25` (usually an underscore).
   - When in **Overwrite mode**, the cursor size is set to `100` (usually a solid block).
   - A `try-catch` block ensures that if a console terminal doesn't support changing cursor sizes, the application will fallback gracefully.

3. **Backspace and Delete:**
   - **`[BACKSPACE]`** removes the character *behind* the cursor. If pressed at the beginning of a line, it brings the current line up to the previous line (joining them).
   - **`[DELETE]`** removes the character *under* the cursor. If pressed at the end of a line, it pulls the next line up to the current line (joining them).

4. **Improved Editing:**
   The input logic was completely refactored to use a horizontal and vertical cursor position (`cursorX` and `cursorY`) instead of simply appending characters to the end. Lines are represented as a list of `StringBuilder` objects and rendered using coordinate-based rendering.

## Files Modified

- [Prompt.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/Prompt.cs)
- [IConsole.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/IConsole.cs)
- [SystemConsole.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/SystemConsole.cs)

> [!TIP]
> You can now test the behavior in your console by running the application!
