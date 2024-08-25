using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using SS;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    private Spreadsheet ss;
    private string currentPath;

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();
        ss = new Spreadsheet(s => Char.IsUpper(s, 0), s => s.ToUpper(), "ps6");
        currentPath = Directory.GetCurrentDirectory();

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);
    }

    /// <summary>
    /// Displays when selected cell changes
    /// </summary>
    /// <param name="grid"></param>
    private void displaySelection(ISpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        Address.Text = GetCellName(col, row);
        Content.Text = ss.GetCellContents(GetCellName(col, row)).ToString();
        Value.Text = ss.GetCellValue(GetCellName(col, row)).ToString();
    }

    /// <summary>
    /// Opens an empty spreadsheet grid
    /// </summary>
    private async void NewClicked(Object sender, EventArgs e)
    {
        // Display warning if the current grid is not saved
        bool answer = false;
        if (ss.Changed)
            answer = await DisplayAlert("Unsaved Data", "Do you want to save your changes?", "Yes", "No");
        if (answer)
            SaveClicked(sender, e);

        spreadsheetGrid.Clear();
        ss = new Spreadsheet(s => Char.IsUpper(s, 0), s => s.ToUpper(), "ps6");
        currentPath = Directory.GetCurrentDirectory();
        spreadsheetGrid.SetSelection(0, 0);
        Address.Text = "A1";
        Content.Text = "";
        Value.Text = "";
    }

    /// <summary>
    /// Opens any file as text and creates a spreadsheet grid correspond to the data in the file
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        // Display warning if the current grid is not saved
        bool answer = false;
        if (ss.Changed)
            answer = await DisplayAlert("Unsaved Data", "Do you want to save your changes?", "Yes", "No");
        if (answer)
            SaveClicked(sender, e);

        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                System.Diagnostics.Debug.WriteLine("Successfully chose file: " + fileResult.FileName);

                ss = new Spreadsheet(fileResult.FullPath, s => Char.IsUpper(s, 0), s => s.ToUpper(), "ps6");
                currentPath = fileResult.FullPath;
                spreadsheetGrid.Clear();
                foreach(string name in ss.GetNamesOfAllNonemptyCells())
                {
                    GetPosition(name, out int col, out int row);
                    spreadsheetGrid.SetValue(col, row, ss.GetCellValue(name).ToString());
                }
                spreadsheetGrid.SetSelection(0, 0);
                Address.Text = "A1";
                Content.Text = ss.GetCellContents("A1").ToString();
                Value.Text = ss.GetCellValue("A1").ToString();
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Saves the current spreadsheet grid in a file
    /// </summary>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        string folderName = "";
        while (string.IsNullOrEmpty(folderName) || string.IsNullOrWhiteSpace(folderName))
            folderName = await DisplayPromptAsync("File name", "Enter a file name to save", "OK", "Cancel");

        if (!folderName.EndsWith(".sqrd"))
            folderName += ".sprd";
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = path + "\\" + folderName;

        // If saving the file will modify an existed file then display warning
        bool overwrite = false;
        if (File.Exists(path) && !path.Equals(currentPath))
        {
            overwrite = await DisplayAlert("Overwritting file", "Do you want to overwrite this file " + path, "Yes", "No");
            if (!overwrite)
                return;
        }

        // Save the spreadsheet into the file in path
        try
        {
            ss.Save(path);
            currentPath = path;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving file:");
            Console.WriteLine(ex);
        }  
    }

    /// <summary>
    /// Finds an user input value in the spreadsheet grid and highlights the cells that contain it
    /// </summary>
    private async void FindClicked(Object sender, EventArgs e)
    {
        // Get the target value from user
        string value = await DisplayPromptAsync("Find", "What value do you want to find?", "OK", "Cancel");
        List<string> foundCells = new List<string>();
        IEnumerable<string> nonEmptyCells = ss.GetNamesOfAllNonemptyCells();

        // Add the found cells to the found list to highlight
        foreach (string name in nonEmptyCells)
        {
            if (ss.GetCellValue(name).ToString().Equals(value))
            {
                GetPosition(name, out int col, out int row);
                spreadsheetGrid.AddToFound(col, row);
            }
        }
    }

    /// <summary>
    /// Sets the content and value of a cell and its dependents
    /// </summary>
    private void ContentChanged(Object sender, EventArgs e)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        string name = GetCellName(col, row);

        // Recalculate all dependents
        IList<string> recalculate = new List<string>();
        try
        {
            recalculate = ss.SetContentsOfCell(name, Content.Text);
        }
        catch (Exception)
        {
            DisplayAlert("Exception thrown", "Check your formula", "OK");
        }

        Value.Text = ss.GetCellValue(name).ToString();

        // Change dependents' values in spreadsheet grid
        foreach (string cellName in recalculate)
        {
            GetPosition(cellName, out int cellCol, out int cellRow);
            string cellValue = ss.GetCellValue(cellName).ToString();
            if (cellValue.Equals("SpreadsheetUtilities.FormulaError"))
                spreadsheetGrid.SetValue(cellCol, cellRow, "ERROR");
            else
                spreadsheetGrid.SetValue(cellCol, cellRow, cellValue);
        }
    }

    /// <summary>
    /// Unhighlight the cells found by Find value
    /// </summary>
    public void ClearFindClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.ClearFind();
    }

    /// <summary>
    /// Displays help document
    /// </summary>
    public void HelpClicked(Object sender, EventArgs e)
    {
        DisplayAlert("How to use spreadsheet",
            "To create a new spreadsheet click on \"File\" and select \"New\".\r\n" +
            "To load an existing spreadsheet click on \"File\" and select \"Load\".\r\n" +
            "To save a spreadsheet click on \"File\" and select \"Save\".\r\n\r\n" +
            "In order to change a selected cell, simply hover the cursor over\r\n" +
            "a target cell and left click to select it.\r\n" +
            "In order to change the contents of a cell, click on the top \r\n" +
            "middle entry where it says \"f(x)\", enter a desired value, and press Enter.\r\n\r\n" +
            "The extra feature allows user to find a specific value within \r\n" +
            "the spreadsheet. To use the find feature, select \"Find\" on the \r\n" +
            "menu bar. Select \"Find Value\". You will be presented\r\n" +
            "a new window where it asks you to input a value to be found. \r\n" +
            "Once the user has inputed the value to be found and hit find \r\n" +
            "button, the feature will highlight the cells that contain the \r\n" +
            "desired value. In order to clear the highlight, simply click\r\n" +
            "on \"Find\" on the menu bar and click on \"Clear Find\".", "OK");
    }

    /// <summary>
    /// Gets the cell name from its position (col, row)
    /// </summary>
    /// <param name="col">Column number</param>
    /// <param name="row">Row number</param>
    /// <returns>Cell name</returns>
    private string GetCellName(int col, int row)
    {
        char letter = (char)(col + 65);
        int number = row + 1;
        return "" + letter + number;
    }

    /// <summary>
    /// Gets the cell position (col, row) from its name
    /// </summary>
    /// <param name="name">Cell name</param>
    /// <param name="col">Column number</param>
    /// <param name="row">Row number</param>
    private void GetPosition(string name, out int col, out int row)
    {   
        col = name[0] - 65;
        row = int.Parse(name.Substring(1)) - 1;
    }
}
