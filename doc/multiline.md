#ask multi line . aka textarea . 

too many bugs rework it managing edge cases :

## global architecture consideration

avoid local methods. you'de better extract then in a decicated class called VirtualConsole.

This class should handle:
- cursor movement
- line management
- buffer management
- scrolling




## edge cases

### scrolling

at top of screen

**on arrow up:**

  - when a top of screen and currently displya line at line #0 is not the first line of the buffer: redraw starting fomr line -1 at top of screen.
  - when a top of buffer dot nothhing


  

**on enter key :**
if a position (0,0) on screen and in insert mode -> insert a line at current line in buffer and redraw


at bottom of screen

**on arrow down :**
 - if last line is on bottom screen, do nothing.
 - else redraw starting from line +1 at top.

** on enter key :**
  - if position (height,0) insert an empty line after current line. and move global display up using a writeline. Tht is also to say that starting textarea display move one line up in the display
  


## bugs 

  - if at (0,0) on screen AND buffer : moving up is making disapear the cursor but input char is still possible even if thy re not visible.

# unit tests

Add unit tests for all edge cases.
Add unit tests for all bugs.

