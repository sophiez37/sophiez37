Code completed by: 
Phuong Anh Nguyen and Alimkhan Zhanaladin
30/11/2023


INTRODUCTION:
This project is a spreadsheet app. It attempts to manage data 
in a user-friendly and efficient manner

FEATURES:
The app can evaluate a function that uses variables, integers 
and operations.
Find value feature allows a user to find a cells value.
Preview for cell location
Display for cell value
Entry for editing the call contents.
Responsive design.

USAGE:
To create a new spreadsheet click on "File" and select "New".
To load an existing spreadsheet click on "File" and select "Load".
To save a spreadsheet click on "File" and select "Save".

In order to change a selected cell, simply hover the cursor over
a target cell and left click to select it.
In order to change the contents of a cell, click on the top 
middle entry where it says "f(x)", enter a desired value, and press Enter.

The extra feature allows user to find a specific value within 
the spreadsheet. To use the find feature, select "Find" on the 
menu bar. Select "Find Value". You will be presented
a new window where it asks you to input a value to be found. 
Once the user has inputed the value to be found and hit find 
button, the feature will highlight the cells that contain the 
desired value. In order to clear the highlight, simply click
on "Find" on the menu bar and click on "Clear Find".

IMPLEMENTATION NOTES & DESIGN DECISIONS:

 - The tool bar that displays cell location uses relative width
	to keep the window responsive. This allows user to 
	comfortly change the window size without loosing the bar.
 - In order to get the filename to save, there is a pop up window
	that appears when the user click "Save" to ask for the filename.
	That prompt will keep appearing until the user input some string
	that is not an empty string or white spaces.



