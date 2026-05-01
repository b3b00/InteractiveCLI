using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace interactiveCLI;

public class VirtualConsole
{
    private readonly IConsole _console;
    private int _startTop;
    private int _maxVisibleLines;
    
    public List<StringBuilder> Lines { get; private set; }
    public int CursorX { get; private set; } // Column within current line
    public int CursorY { get; private set; } // Line index in the buffer
    public int ViewTopY { get; private set; } // Buffer line currently at top of view
    public bool IsInsertMode { get; set; } = true;
    public int MaxLines { get; }

    public VirtualConsole(IConsole console, int maxLines = 0)
    {
        _console = console;
        _startTop = _console.CursorTop;
        MaxLines = maxLines;
        
        // Ensure we have at least 5 lines of space if possible
        int availableLines = _console.WindowHeight - _startTop;
        if (availableLines < 5 && _console.WindowHeight >= 5)
        {
            int scrollAmount = 5 - availableLines;
            for(int i=0; i<scrollAmount; i++)
            {
                 _console.SetCursorPosition(0, _console.WindowHeight - 1);
                 _console.WriteLine();
            }
            _startTop -= scrollAmount;
            if (_startTop < 0) _startTop = 0;
        }

        _maxVisibleLines = _console.WindowHeight - _startTop;
        if (_maxVisibleLines < 1) _maxVisibleLines = 1;

        Lines = new List<StringBuilder> { new StringBuilder() };
        CursorX = 0;
        CursorY = 0;
        ViewTopY = 0;
        
        Redraw();
    }

    private void SetConsoleCursor()
    {
        int screenY = _startTop + (CursorY - ViewTopY);
        if (screenY < _startTop) screenY = _startTop;
        if (screenY >= _console.WindowHeight) screenY = _console.WindowHeight - 1;
        
        try
        {
            _console.SetCursorPosition(CursorX, screenY);
        }
        catch { }
    }

    public void Redraw()
    {
        for (int i = 0; i < _maxVisibleLines; i++)
        {
            int bufferY = ViewTopY + i;
            int screenY = _startTop + i;
            
            try
            {
                _console.SetCursorPosition(0, screenY);
            }
            catch { continue; }

            if (bufferY < Lines.Count)
            {
                string lineText = Lines[bufferY].ToString();
                int paddingLen = Math.Max(0, _console.WindowWidth - lineText.Length - 1);
                string padding = paddingLen > 0 ? new string(' ', paddingLen) : "";
                _console.Write(lineText + padding);
            }
            else
            {
                int paddingLen = Math.Max(0, _console.WindowWidth - 1);
                string padding = paddingLen > 0 ? new string(' ', paddingLen) : "";
                _console.Write(padding);
            }
        }
        SetConsoleCursor();
    }

    public void RedrawLine(int y)
    {
        if (y >= ViewTopY && y < ViewTopY + _maxVisibleLines)
        {
            int screenY = _startTop + (y - ViewTopY);
            try
            {
                _console.SetCursorPosition(0, screenY);
                string lineText = Lines[y].ToString();
                int paddingLen = Math.Max(0, _console.WindowWidth - lineText.Length - 1);
                string padding = paddingLen > 0 ? new string(' ', paddingLen) : "";
                _console.Write(lineText + padding);
            }
            catch { }
            SetConsoleCursor();
        }
    }

    public void MoveLeft()
    {
        if (CursorX > 0)
        {
            CursorX--;
            SetConsoleCursor();
        }
        else if (CursorY > 0)
        {
            CursorY--;
            CursorX = Lines[CursorY].Length;
            CheckScrollUp();
        }
    }

    public void MoveRight()
    {
        if (CursorX < Lines[CursorY].Length)
        {
            CursorX++;
            SetConsoleCursor();
        }
        else if (CursorY < Lines.Count - 1)
        {
            CursorY++;
            CursorX = 0;
            CheckScrollDown();
        }
    }

    public void MoveUp()
    {
        if (CursorY > 0)
        {
            CursorY--;
            if (CursorX > Lines[CursorY].Length)
            {
                CursorX = Lines[CursorY].Length;
            }
            CheckScrollUp();
        }
    }

    public void MoveDown()
    {
        if (CursorY < Lines.Count - 1)
        {
            CursorY++;
            if (CursorX > Lines[CursorY].Length)
            {
                CursorX = Lines[CursorY].Length;
            }
            CheckScrollDown();
        }
    }

    public void MoveHome(bool ctrl)
    {
        if (ctrl)
        {
            CursorY = 0;
            CursorX = 0;
            if (ViewTopY != 0)
            {
                ViewTopY = 0;
                Redraw();
            }
            else
            {
                SetConsoleCursor();
            }
        }
        else
        {
            CursorX = 0;
            SetConsoleCursor();
        }
    }

    public void MoveEnd(bool ctrl)
    {
        if (ctrl)
        {
            CursorY = Lines.Count - 1;
            CursorX = Lines[CursorY].Length;
            CheckScrollDown();
        }
        else
        {
            CursorX = Lines[CursorY].Length;
            SetConsoleCursor();
        }
    }

    private void CheckScrollUp()
    {
        if (CursorY < ViewTopY)
        {
            // when a top of screen and currently display line at line #0 is not the first line of the buffer: redraw starting form line -1 at top of screen.
            ViewTopY = CursorY;
            Redraw();
        }
        else
        {
            SetConsoleCursor();
        }
    }

    private void CheckScrollDown()
    {
        if (CursorY >= ViewTopY + _maxVisibleLines)
        {
            // if last line is on bottom screen, do nothing. else redraw starting from line +1 at top.
            ViewTopY = CursorY - _maxVisibleLines + 1;
            Redraw();
        }
        else
        {
            SetConsoleCursor();
        }
    }

    public void Enter()
    {
        if (MaxLines > 0 && Lines.Count >= MaxLines)
            return;

        string remainder = Lines[CursorY].ToString().Substring(CursorX);
        Lines[CursorY].Length = CursorX;
        Lines.Insert(CursorY + 1, new StringBuilder(remainder));
        
        CursorY++;
        CursorX = 0;

        // "on enter key : if a position (height,0) insert an empty line after current line. and move global display up using a writeline. Tht is also to say that starting textarea display move one line up in the display"
        int screenY_new = _startTop + (CursorY - ViewTopY);
        if (screenY_new >= _console.WindowHeight)
        {
            if (_startTop > 0)
            {
                try
                {
                    _console.SetCursorPosition(0, _console.WindowHeight - 1);
                    _console.WriteLine();
                }
                catch { }
                _startTop--;
                _maxVisibleLines = _console.WindowHeight - _startTop;
            }
            else
            {
                ViewTopY++;
            }
        }
        Redraw();
    }

    public void Backspace()
    {
        if (CursorX > 0)
        {
            CursorX--;
            Lines[CursorY].Remove(CursorX, 1);
            RedrawLine(CursorY);
        }
        else if (CursorY > 0)
        {
            string currentLineText = Lines[CursorY].ToString();
            Lines.RemoveAt(CursorY);
            CursorY--;
            CursorX = Lines[CursorY].Length;
            Lines[CursorY].Append(currentLineText);
            
            if (CursorY < ViewTopY)
            {
                ViewTopY = CursorY;
            }
            Redraw();
        }
    }

    public void Delete()
    {
        if (CursorX < Lines[CursorY].Length)
        {
            Lines[CursorY].Remove(CursorX, 1);
            RedrawLine(CursorY);
        }
        else if (CursorY < Lines.Count - 1)
        {
            string nextLineText = Lines[CursorY + 1].ToString();
            Lines.RemoveAt(CursorY + 1);
            Lines[CursorY].Append(nextLineText);
            Redraw();
        }
    }

    public void InsertChar(char c)
    {
        if (IsInsertMode)
        {
            Lines[CursorY].Insert(CursorX, c);
            CursorX++;
            RedrawLine(CursorY);
        }
        else
        {
            if (CursorX < Lines[CursorY].Length)
            {
                Lines[CursorY][CursorX] = c;
                CursorX++;
                RedrawLine(CursorY);
            }
            else
            {
                Lines[CursorY].Append(c);
                CursorX++;
                RedrawLine(CursorY);
            }
        }
    }

    public void Finish()
    {
        try
        {
            int bottomY = _startTop + (Lines.Count - ViewTopY);
            if (bottomY > _console.WindowHeight - 1)
            {
                bottomY = _console.WindowHeight - 1;
            }
            _console.SetCursorPosition(0, bottomY);
            _console.WriteLine();
        }
        catch { }
    }
}
