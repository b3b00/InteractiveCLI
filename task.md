# Tasks for AskMultiLineText Navigation

- [x] Update `IConsole.cs` and `SystemConsole.cs` to include `CursorSize` property.
- [x] Refactor `AskMultiLineText` in `Prompt.cs` to maintain `cursorX` and `cursorY`.
- [x] Implement arrow key navigation (`LeftArrow`, `RightArrow`, `UpArrow`, `DownArrow`).
- [x] Implement `Insert` key to toggle insert/overwrite mode and update `CursorSize`.
- [x] Update `Backspace` to handle deleting behind the cursor across lines.
- [x] Implement `Delete` key to remove the character under the cursor.
- [x] Update regular character input to respect `cursorX`, `cursorY`, and the insert mode.
- [x] Verify functionality manually or with a test app.
