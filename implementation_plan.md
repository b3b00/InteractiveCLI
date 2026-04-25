# Arrow Navigation in AskMultiLineText

Currently, `AskMultiLineText` in `interactiveCLI.Prompt` only supports appending characters and deleting the last character via `Backspace`. We need to implement proper cursor navigation to make it behave more like a standard text area.

## User Review Required

> [!IMPORTANT]
> Please review the answers to your questions below and confirm if you are happy with the proposed approach.

### Answers to your questions:

**1. Would the carret move in insert or overwrite mode (maybe let user choose with [INSER] key)**
* **Proposed Approach:** Let's default to **Insert mode**, as that's the standard for modern text editors. We will implement support for the `Insert` key to toggle between Insert and Overwrite modes. 

**2. What about changing the carret look to reflect insert/overwrite mode?**
* **Proposed Approach:** Yes! We can change the cursor size to visually indicate the mode. A standard underscore cursor (e.g. `CursorSize = 25`) indicates Insert mode, while a block cursor (e.g. `CursorSize = 100`) indicates Overwrite mode. We will add a `CursorSize` property to `IConsole` and implement it in `SystemConsole` (wrapping it in a try-catch, as it can sometimes throw `PlatformNotSupportedException` on non-Windows platforms, ensuring it gracefully falls back).

**3. What arrows are allowed [LEFT] and [RIGHT] are obvious but should user be able to navigate accross line with [UP] and [DOWN]**
* **Proposed Approach:** Yes, `[UP]` and `[DOWN]` should absolutely be allowed for navigating across lines. When pressing `[UP]`, the cursor will move to the same character index on the line above (or to the end of the line above if it's shorter). Same logic applies for `[DOWN]`.

## Proposed Changes

### `interactiveCLI`

#### [MODIFY] [IConsole.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/IConsole.cs)
- Add `int CursorSize { get; set; }` to the interface.

#### [MODIFY] [SystemConsole.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/SystemConsole.cs)
- Implement `CursorSize` wrapping `Console.CursorSize`. If `Console.CursorSize` throws `PlatformNotSupportedException`, catch it and do nothing to ensure cross-platform safety.

#### [MODIFY] [Prompt.cs](file:///c:/Users/olduh/dev/InteractiveCLI/src/interactiveCLI/Prompt.cs)
- Refactor the inner loop of `AskMultiLineText` to maintain:
  - `int cursorX`: The current column position within the current line.
  - `int cursorY`: The current row index in the `lines` list.
  - `bool isInsertMode`: Toggled by the `Insert` key.
- Update the rendering logic:
  - We will need a way to redraw lines when characters are inserted in the middle of a line.
  - Since standard console input reads line by line, modifying previous lines requires moving the cursor with `_console.SetCursorPosition()` and re-rendering the updated line.
- Key bindings to add:
  - `LeftArrow`: Move `cursorX` left. If at the beginning of the line, move to the end of the previous line.
  - `RightArrow`: Move `cursorX` right. If at the end of the line, move to the beginning of the next line.
  - `UpArrow`: Move `cursorY` up by 1. Keep `cursorX` unless the new line is shorter, then clamp it.
  - `DownArrow`: Move `cursorY` down by 1. Keep `cursorX` unless the new line is shorter, then clamp it.
  - `Insert`: Toggle `isInsertMode`.
  - `Delete`: Remove the character under the cursor (different from `Backspace` which removes the character before the cursor).
  - Modify `Backspace`, `Enter`, and regular character input to account for `cursorX` and `cursorY` (inserting instead of just appending).

## Verification Plan

### Manual Verification
1. Run a sample application using `AskMultiLineText`.
2. Type multiple lines of text.
3. Use Arrow keys to navigate up, down, left, and right.
4. Use `Insert` to toggle mode and type in the middle of a word to verify insert/overwrite behavior.
5. Use `Delete` and `Backspace` in the middle of lines and across line boundaries to verify text shifting.
