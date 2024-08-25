// Created by: Phuong Anh Nguyen
// Date: Sep 29 2023

using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS
{
    /// <summary>
    /// Class representing a Spreadsheet. Inherit from AbstractSpreadsheet.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        // A map from cell name to Cell object
        public Dictionary<string, Cell> Cells { get; private set; }
        // A dependency graph between cells
        private DependencyGraph graph;
        // Validator and Normalizer functions for cell names
        private Func<string, bool> IsValid;
        private Func<string, string> Normalize;
        
        /// <summary>
        /// Constructs an empty spreadsheet that imposes no extra validity condition, 
        /// normalizes every cell name to itself, and has version "default"
        /// </summary>
        public Spreadsheet() : base("default")
        {
            Cells = new();
            graph = new();
            this.IsValid = s => true;
            this.Normalize = s => s;
            Changed = false;
        }

        /// <summary>
        /// Constructs an empty spreadsheet with determined validator, normalizer, and version
        /// </summary>
        /// <param name="IsValid">Validator</param>
        /// <param name="Normalize">Normalizer</param>
        /// <param name="version">Version</param>
        public Spreadsheet(Func<string, bool> IsValid, Func<string, string> Normalize, string version) : base(version)
        {
            Cells = new();
            graph = new();
            this.IsValid = IsValid;
            this.Normalize = Normalize;
            Changed = false;
        }

        /// <summary>
        /// Constructs a spreadsheet that contains only strings. Used by Json Deserializer
        /// </summary>
        /// <param name="cells">Map from cell name to Cell object</param>
        /// <param name="version">Version</param>
        [JsonConstructor]
        public Spreadsheet(Dictionary<string, Cell> cells, string version) : base(version)
        {
            Cells = cells;
            graph = new();
            this.IsValid = s => true;
            this.Normalize = s => s;
        }

        /// <summary>
        /// Contructs a spreadsheet from data in the file which has the address of path. The spreadsheet
        /// also has determined validator, normalizer, and version.
        /// </summary>
        /// <param name="path">Path to the file containing data of the spreadsheet</param>
        /// <param name="IsValid">Validator</param>
        /// <param name="Normalize">Normalizer</param>
        /// <param name="version">Version</param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public Spreadsheet(string path, Func<string, bool> IsValid, Func<string, string> Normalize, string version) : base(version)
        {
            // Read all data from the file into a string
            string fileContent = "";
            try
            {
                fileContent = File.ReadAllText(path);
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Problems opening, reading, or closing the file");
            }

            // Constructs a spreadsheet which contains only strings
            Spreadsheet? stringOnly;
            try
            {
               stringOnly  = JsonSerializer.Deserialize<Spreadsheet>(fileContent);
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Problems deserializing.");
            }
            
            // Constructs this spreadsheet using the string only spreadsheet
            if (stringOnly != null)
            {
                this.Cells = new();
                graph = new();
                this.IsValid = IsValid;
                this.Normalize = Normalize;
                Changed = false;

                // If the version from the file doesn't match the input version
                if (!stringOnly.Version.Equals(version))
                    throw new SpreadsheetReadWriteException("Version mismatch.");

                // Set the contents of this spreadsheet's cells
                foreach (string name in stringOnly.Cells.Keys)
                {
                    string stringContent = stringOnly.Cells[name].StringForm;
                    try
                    {
                            SetContentsOfCell(name, stringContent);
                    }
                    catch(Exception ex)
                    {
                        if (ex is InvalidNameException)
                            throw new SpreadsheetReadWriteException("Cell's name is invalid.");
                        else if (ex is CircularException)
                            throw new SpreadsheetReadWriteException("Spreadsheet contains circular dependency.");
                        else
                            throw new SpreadsheetReadWriteException("Exception thrown: " + ex.Message);
                    }
                }
            }
            else
            {
                throw new SpreadsheetReadWriteException("New spreadsheet is null.");
            }
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "Cells" - a data structure containing 0 or more cell entries
        ///           Each cell entry has a field (or key) named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "StringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of StringForm is that string
        ///               - If the contents is a double d, the value of StringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of StringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "Cells": {
        ///     "A1": {
        ///       "StringForm": "5"
        ///     },
        ///     "B3": {
        ///       "StringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            // Set up indented option
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            string savedString = JsonSerializer.Serialize(this, options);

            // Write the Json string to the file
            try
            {
                File.WriteAllText(filename, savedString);
            }
            catch (Exception ex)
            {
                throw new SpreadsheetReadWriteException("Exception thrown: " + ex.Message);
            }

            Changed = false;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            // Throw exception if cell name is invalid
            if (!IsValidCell(name))
                throw new InvalidNameException();
            else
            {
                name = Normalize(name);
                // If the Cell is in the map then return the value
                if (Cells.ContainsKey(name))
                    return Cells[name].Value;
                // If the Cell is not in the map then return an empty string
                else
                    return "";
            }
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> nonEmptyCells = new();

            foreach (string name in Cells.Keys)
                // Cells containing empty string is considered empty
                if (!Cells[name].Content.Equals(""))
                    nonEmptyCells.Add(name);

            return nonEmptyCells;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.
        /// The return value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            // Throw exception if cell name is invalid
            if (!IsValidCell(name))
                throw new InvalidNameException();
            else
            {
                name = Normalize(name);
                // If the Cell is in the map then return the content
                if (Cells.ContainsKey(name))
                    return Cells[name].Content;
                // If the Cell is not in the map then return an empty string
                else
                    return "";
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            // Throw exception if cell name is invalid
            if (!IsValidCell(name))
                throw new InvalidNameException();

            name = Normalize(name);
            IList<string> result = new List<string>();

            // If content is a double
            if (double.TryParse(content, out double value))
                result = SetCellContents(name, value);

            // If content is a Formula
            else if (content.StartsWith('='))
                result = SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid));

            // Else content is a string
            else
                result = SetCellContents(name, content);

            // Recalculate all the direct and indirect dependent cells
            foreach (string dependent in result)
            {
                if (dependent.Equals(name))
                    continue;
                else
                {
                    Formula dependentContent = (Formula)Cells[dependent].Content;
                    Cells[dependent] = new Cell(dependentContent, Lookup);
                }
            }

            return result;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {
            object originalContent = GetCellContents(name);

            // Set a new content for the cell
            if (Cells.ContainsKey(name))
                Cells[name] = new Cell(number);
            else
                Cells.Add(name, new Cell(number));

            // Remove old dependees of the cell
            graph.ReplaceDependees(name, new List<string>());

            if (!number.Equals(originalContent))
                Changed = true;

            // Return the list of direct and indirect dependents of the cell
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {
            object originalContent = GetCellContents(name);

            // Set a new content for the cell
            if (Cells.ContainsKey(name))
                Cells[name] = new Cell(text);
            else
                Cells.Add(name, new Cell(text));

            // Remove old dependees of the cell
            graph.ReplaceDependees(name, new List<string>());

            if (!text.Equals(originalContent))
                Changed = true;

            // Return the list of direct and indirect dependents of the cell
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            // Keep track of the original cell in case circular exception is thrown
            Cell originalCell;
            if (Cells.ContainsKey(name))
                originalCell = Cells[name];
            else
                originalCell = new Cell("");

            // Set a new content for the cell
            if (Cells.ContainsKey(name))
                Cells[name] = new Cell(formula, Lookup);
            else
                Cells.Add(name, new Cell(formula, Lookup));

            // Replace old dependees of the cell with new dependees
            graph.ReplaceDependees(name, formula.GetVariables());

            // Add the dependencies between the cell and the variables in the formula
            foreach (string var in formula.GetVariables())
                graph.AddDependency(var, name);

            // If the new formula causes a circular graph then set the cell's content back to original
            // and the throw exception
            try
            {
                GetCellsToRecalculate(name);
                if (!formula.Equals(originalCell.Content))
                    Changed = true;
            }
            catch (CircularException)
            {
                Cells[name] = originalCell;
                throw new CircularException();
            }

            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependents(Normalize(name));
        }

        /// <summary>
        /// Checks if a string contains valid cell name or not
        /// </summary>
        /// <param name="name">The string that needs to be checked</param>
        /// <returns>True if the string contains valid cell name, false otherwise</returns>
        private bool IsValidCell(string name)
        {
            string pattern = @"^[a-zA-Z_][a-zA-Z_0-9]*$";
            return Regex.IsMatch(name, pattern) && IsValid(Normalize(name));
        }

        /// <summary>
        /// Return the double value of the cell named name. If the value of the name cell is not a double,
        /// throw Argument Exception
        /// </summary>
        /// <param name="name">Name of the cell needed looking up</param>
        /// <returns>Double value of the cell</returns>
        /// <exception cref="ArgumentException"></exception>
        private double Lookup(string name)
        {
            if (!Cells.ContainsKey(name))
                throw new ArgumentException();
            else if (GetCellValue(name) is double)
                return (double)GetCellValue(name);
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Class representing the Cell object
        /// </summary>
        public class Cell
        {
            // Field containing the content of the cell
            // Content can be of type string, double, or Formula
            [JsonIgnore]
            public object Content { get; private set; }
            // Field containing the value of the cell
            // Value can be of type strin, double, or FormulaError
            [JsonIgnore]
            public object Value { get; private set; }
            // Field containing the string representation of the cell's content
            public string StringForm { get; private set; }

            /// <summary>
            /// Constructs a Cell that has the content of type string
            /// </summary>
            /// <param name="stringContent">Content of the new Cell</param>
            [JsonConstructor]
            public Cell(string stringForm)
            {
                Content = stringForm;
                Value = stringForm;
                StringForm = stringForm;
            }

            /// <summary>
            /// Constructs a Cell that has the content of type double
            /// </summary>
            /// <param name="doubleContent">Content of the new Cell</param>
            public Cell(double doubleContent)
            {
                Content = doubleContent;
                Value = doubleContent;
                StringForm = doubleContent.ToString();
            }

            /// <summary>
            /// Constructs a Cell that has the content of type string
            /// </summary>
            /// <param name="formulaContent">Content of the new Cell</param>
            /// <param name="lookup">Function to look up the value of variables in the Formula</param>
            public Cell(Formula formulaContent, Func<string, double> lookup)
            {
                Content = formulaContent;
                Value = formulaContent.Evaluate(lookup);
                StringForm = "=" + formulaContent.ToString();
            }
        }
    }
}
