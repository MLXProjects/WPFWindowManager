# WPFWindowManager

This project is just a simple "window manager" concept made in C# using WPF.  
It targets .NET Framework 4.5.2 and was made using Visual Studio 2013.  
Tested only on Win7 x64, but compiled for any processor arch.  
It makes use of WinAPI and is related to my other project, WPF3DWindowViewer (it uses parts of this project as basis).  
  
<b>What's working?</b>  
Currently only the window displaying (with nearly-live update), movement (dragging the caption bar) and close/resize/minimize buttons (which work if you press on his location, even if they're not present) are working. Everything else isn't actually implemented.  
  
<b>How does this work?</b>  
This program basically lists all opened and visible windows into a ComboBox.  
When the user selects one of the CB's items, the program writes the window's HWND (unique window identifier) to a little textbox (which can be edited with any window's HWND) and then parses it to get an copy of the selected window. That copy is just a window's screenshot taken as a bitmap, which is displayed to the Image control associated to that window.  
The windows are updated each 200ms, so there cam be some lag between real window input and what's displayed here.  
  
<b>WARNING:</b>  
This project has a little memory usage bug, and so if you have 2GB RAM or less I don't recommend that you try to run this with more than 2 windows open. 
