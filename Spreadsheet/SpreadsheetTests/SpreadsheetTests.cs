// Created by: Phuong Anh Nguyen
// Date: Sep 29 2023

using SpreadsheetUtilities;
using SS;
using System.IO;

namespace SpreadsheetTests
{
    /// <summary>
    /// Class containing testers for methods in Spreadsheet class
    /// </summary>
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// There must be only 1 non empty cell in the class
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "10");
            List<string> nonEmpty = ss.GetNamesOfAllNonemptyCells().ToList();
            Assert.AreEqual(1, nonEmpty.Count);
            Assert.IsTrue(nonEmpty.Contains("A1"));
        }

        /// <summary>
        /// Cells containing an empty string are considered empty
        /// </summary>
        [TestMethod]
        public void TestMethod2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("A2", "");
            ss.SetContentsOfCell("A3", "Hello");
            ss.SetContentsOfCell("A4", "=A1");
            List<string> nonEmpty = ss.GetNamesOfAllNonemptyCells().ToList();
            Assert.AreEqual(3, nonEmpty.Count);
            Assert.IsTrue(nonEmpty.Contains("A1"));
            Assert.IsFalse(nonEmpty.Contains("A2"));
            Assert.IsTrue(nonEmpty.Contains("A3"));
            Assert.IsTrue(nonEmpty.Contains("A4"));
        }

        /// <summary>
        /// GetCellContents returns an empty string for empty cell
        /// </summary>
        [TestMethod]
        public void TestMethod3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "1");
            ss.SetContentsOfCell("A2", "Hello");
            ss.SetContentsOfCell("A3", "= A1*5");
            Assert.AreEqual(1.0, ss.GetCellContents("A1"));
            Assert.AreEqual("Hello", ss.GetCellContents("A2"));
            Assert.AreEqual(new Formula("A1  * 5"), ss.GetCellContents("A3"));
            Assert.AreEqual("", ss.GetCellContents("A4"));
        }

        /// <summary>
        /// GetCellContents throws exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.GetCellContents("A$&");
        }

        /// <summary>
        /// SetContentsOfCell throws exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A$&", "0");
        }

        /// <summary>
        /// SetContentsOfCell throws exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod6()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("_1a*", "Hello");
        }

        /// <summary>
        /// SetContentsOfCell throws exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod7()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("1A", "=1");
        }

        /// <summary>
        /// SetContentsOfCell on a cell without any dependees with return a Enumerable containing
        /// only itself
        /// </summary>
        [TestMethod]
        public void TestMethod8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "Hello");
            IEnumerable<string> dependents = ss.SetContentsOfCell("A1", "Hola");
            Assert.AreEqual(1, dependents.Count());
            Assert.IsTrue(dependents.Contains("A1"));
        }

        /// <summary>
        /// SetContentsOfCell should return a list of all direct and indirect dependents of
        /// the cell whose value is changed, including itself
        /// </summary>
        [TestMethod]
        public void TestMethod9()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "1");
            ss.SetContentsOfCell("A2", "= A1 + 1");
            ss.SetContentsOfCell("B1", "= A2 + A0");
            ss.SetContentsOfCell("B2", "= B1 + 5");
            IEnumerable<string> dependents = ss.SetContentsOfCell("A1", "3");
            Assert.AreEqual(4, dependents.Count());
            Assert.IsTrue(dependents.Contains("A1"));
            Assert.IsTrue(dependents.Contains("A2"));
            Assert.IsTrue(dependents.Contains("B1"));
            Assert.IsTrue(dependents.Contains("B2"));
        }

        /// <summary>
        /// SetContentsOfCell on a cell that has the original content of a Formula
        /// will remove the dependency of that cell and its dependees
        /// </summary>
        [TestMethod]
        public void TestMethod10()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("B1", "= A1 + 1");
            ss.SetContentsOfCell("B1", "10");
            IEnumerable<string> dependents = ss.SetContentsOfCell("A1", "3");
            Assert.AreEqual(1, dependents.Count());
            Assert.IsTrue(dependents.Contains("A1"));
            Assert.IsFalse(dependents.Contains("B1"));
        }

        /// <summary>
        /// SetContentsOfCell a cell to a formula containing itself will throw circular exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod11()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "= A1");
        }

        /// <summary>
        /// SetContentsOfCell will throw exception if the new Formula content makes the graph circular
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod12()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("B1", "= A1 + 5");
            ss.SetContentsOfCell("A1", "= B1");
        }

        /// <summary>
        /// SetContentsOfCell will throw exception if the new Formula content makes the graph circular
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod13()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("B1", "= A1 + 5");
            ss.SetContentsOfCell("C1", "= B1 + 5");
            ss.SetContentsOfCell("D1", "= C1 + 5");
            ss.SetContentsOfCell("A1", "= D1");
        }

        /// <summary>
        /// SetCellContains remove the old dependees of the cell
        /// </summary>
        [TestMethod]
        public void TestMethod14()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "= A2");
            ss.SetContentsOfCell("A2", "= A3");
            ss.SetContentsOfCell("A1", "0");
            Assert.IsFalse(ss.SetContentsOfCell("A2", "0").Contains("A1"));
            Assert.IsFalse(ss.SetContentsOfCell("A3", "0").Contains("A1"));
        }

        /// <summary>
        /// An empty spreadsheet should have all cells empty
        /// </summary>
        [TestMethod]
        public void TestMethod15()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = 0; i < 100; i++)
                Assert.AreEqual("", ss.GetCellContents("A" + i));
        }

        /// <summary>
        /// If new content causes circular graph then the cell's content should be set back to original
        /// </summary>
        [TestMethod]
        public void TestMethod16()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "Hello");
            try
            {
                ss.SetContentsOfCell("A1", "= A1 + 1");
                Assert.Fail();
            }
            catch (CircularException)
            {
                Assert.AreEqual("Hello", ss.GetCellContents("A1"));
            }
        }

        /// <summary>
        /// If new content causes circular graph then the cell's content should be set back to original
        /// </summary>
        [TestMethod]
        public void TestMethod17()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("A2", "= A1 + 5");
            ss.SetContentsOfCell("A3", "= A2 + 10");
            try
            {
                ss.SetContentsOfCell("A1", "= A3");
                Assert.Fail();
            }
            catch (CircularException)
            {
                Assert.AreEqual(5.0, ss.GetCellContents("A1"));
            }
        }

        /// <summary>
        /// If setting content to an empty cell throws circular exception then the cell
        /// should be set back to empty
        /// </summary>
        [TestMethod]
        public void TestMethod18()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            try
            {
                ss.SetContentsOfCell("A1", "= A1 + 1");
                Assert.Fail();
            }
            catch (CircularException)
            {
                Assert.AreEqual("", ss.GetCellContents("A1"));
            }
        }

        /// <summary>
        /// Test all aspects of our spreadsheet
        /// </summary>
        [TestMethod]
        public void StressTest1()
        {
            // Set up out spreadsheet
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "2");
            ss.SetContentsOfCell("A3", "3");
            ss.SetContentsOfCell("A4", "4");
            ss.SetContentsOfCell("A1", "= A2 + A3");
            ss.SetContentsOfCell("B1", "= A1 + 1");
            ss.SetContentsOfCell("C1", "= A1 + B1");
            ss.SetContentsOfCell("D1", "= A1 + B1 + C1");

            // The change in A2 will affect A2, A1, B1, C1, D1 but not A3
            IEnumerable<string> dependentsA2 = ss.SetContentsOfCell("A2", "10");
            Assert.IsTrue(dependentsA2.Contains("A2") &&
                          dependentsA2.Contains("A1") &&
                          dependentsA2.Contains("B1") &&
                          dependentsA2.Contains("C1") &&
                          dependentsA2.Contains("D1"));
            Assert.IsFalse(dependentsA2.Contains("A3"));
            Assert.AreEqual(5, dependentsA2.Count());

            // The change in D1 will affect nothing but itself
            IEnumerable<string> dependentsD1 = ss.SetContentsOfCell("D1", "0");
            Assert.IsFalse(dependentsD1.Contains("A1") ||
                           dependentsD1.Contains("B1") ||
                           dependentsD1.Contains("C1"));
            Assert.IsTrue(dependentsD1.Contains("D1"));
            Assert.AreEqual(1, dependentsD1.Count());

            // If the change in A1 throws circular exception then the content of A1
            // will be set back to original
            try
            {
                ss.SetContentsOfCell("A1", "= C1*2");
                Assert.Fail();
            }
            catch
            {
                Assert.AreEqual(new Formula("A2 + A3"), ss.GetCellContents("A1"));
            }

            // If the content of A1 is set to another formula that doesn't contain 
            // A2 and A3, then A1 is not a dependent of A2 and A3 anymore
            ss.SetContentsOfCell("A1", "= A4 / 10");
            Assert.IsFalse(ss.SetContentsOfCell("A2", "12").Contains("A1"));
            Assert.IsFalse(ss.SetContentsOfCell("A3", "13").Contains("A1"));

            // The change in A4 should now affect A4, A1, B1, C1
            IEnumerable<string> dependentsA4 = ss.SetContentsOfCell("A4", "70");
            Assert.IsTrue(dependentsA4.Contains("A4") &&
                          dependentsA4.Contains("A1") &&
                          dependentsA4.Contains("B1") &&
                          dependentsA4.Contains("C1"));
            Assert.AreEqual(4, dependentsA4.Count());
        }

        /// <summary>
        /// Normalizer turns the first character of name into lowercase, then validator which requires
        /// cell name to be capitalized will return false and throw exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod19()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => Char.IsUpper(s, 0), s => s.ToLower(), "first");
            ss.SetContentsOfCell("A1", "10");
        }

        /// <summary>
        /// Normalizer should normalize varible names, not string
        /// </summary>
        [TestMethod]
        public void TestMethod20()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => Char.IsUpper(s, 0), s => s.ToUpper(), "first");
            ss.SetContentsOfCell("a1", "a1");
            ss.SetContentsOfCell("a2", "= a1");
            Assert.AreEqual("a1", ss.GetCellValue("A1"));
            Assert.AreEqual(new Formula("A1"), ss.GetCellContents("a2"));
        }

        /// <summary>
        /// Constructor from a file with path as an empty string should throw exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod21()
        {
            AbstractSpreadsheet ss = new Spreadsheet("", s => true, s => s, "first");
        }

        /// <summary>
        /// A file with invalid content will cause exception when passed to constructor
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod22()
        {
            string invalidJson = "Happy New Year";
            File.WriteAllText("text.txt", invalidJson);
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => true, s => s, "first");
        }

        /// <summary>
        /// If deserialize returns null then throw exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod23()
        {
            File.WriteAllText("text.txt", "null");
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => true, s => s, "first");
        }

        /// <summary>
        /// Expect exception for version mismatch
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod24()
        {
            string emptySS = "{\"Cells\":{},\"Version\":\"first\"}";
            File.WriteAllText("text.txt", emptySS);
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => true, s => s, "second");
        }

        /// <summary>
        /// Expect exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod25()
        {
            string emptySS = "{\"Cells\":{\"a1\":{\"StringForm\":\"= A2 + 1\"}},\"Version\":\"first\"}";
            File.WriteAllText("text.txt", emptySS);
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => Char.IsLower(s, 0), s => s, "first");
        }

        /// <summary>
        /// Expect exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod26()
        {
            string emptySS = "{\"Cells\":{\"A1\":{\"StringForm\":\"= 5\"}},\"Version\":\"first\"}";
            File.WriteAllText("text.txt", emptySS);
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => Char.IsLower(s, 0), s => s, "first");
        }

        /// <summary>
        /// Expect exception for circular exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod27()
        {
            string emptySS = "{\"Cells\":{\"a1\":{\"StringForm\":\"=a2\"},\"a2\":{\"StringForm\":\"=a1\"}},\"Version\":\"first\"}";
            File.WriteAllText("text.txt", emptySS);
            AbstractSpreadsheet ss = new Spreadsheet("text.txt", s => true, s => s, "first");
        }

        /// <summary>
        /// GetCellValue throws exception for invalid cell name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod28()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.GetCellValue("6c");
        }

        /// <summary>
        /// GetCellValue returns an empty string for an empty cell
        /// </summary>
        [TestMethod]
        public void TestMethod29()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.AreEqual("", ss.GetCellValue("A1"));
        }

        /// <summary>
        /// All direct and indirect dependents will have to recalculate
        /// if cell's content changes
        /// </summary>
        [TestMethod]
        public void TestMethod30()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("A2", "= A1 + 2");
            ss.SetContentsOfCell("A3", "= A2 + 3");

            Assert.AreEqual(5.0, ss.GetCellValue("A1"));
            Assert.AreEqual(7.0, ss.GetCellValue("A2"));
            Assert.AreEqual(10.0, ss.GetCellValue("A3"));

            ss.SetContentsOfCell("A1", "15");

            Assert.AreEqual(15.0, ss.GetCellValue("A1"));
            Assert.AreEqual(17.0, ss.GetCellValue("A2"));
            Assert.AreEqual(20.0, ss.GetCellValue("A3"));
        }

        /// <summary>
        /// If Lookup fail to find the value of the variable then the value of the formula
        /// should be FormulaError
        /// </summary>
        [TestMethod]
        public void TestMethod31()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "5");
            ss.SetContentsOfCell("A2", "= A1*2");

            ss.SetContentsOfCell("A1", "Hello");
            Assert.IsTrue(ss.GetCellValue("A2") is FormulaError);
        }

        /// <summary>
        /// The spreadsheet created from a saved spreadsheet must be equal to the original spreadsheet
        /// </summary>
        [TestMethod]
        public void TestMethod32()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "Hello");
            ss.SetContentsOfCell("A2", "1");
            ss.SetContentsOfCell("A3", "= A2 + 5");
            ss.Save("ss.txt");

            Console.WriteLine(File.ReadAllText("ss.txt"));

            AbstractSpreadsheet fromJson = new Spreadsheet("ss.txt", s => true, s => s, "default");

            IEnumerable<string> cells = fromJson.GetNamesOfAllNonemptyCells();
            Assert.AreEqual(3, cells.Count());
            Assert.IsTrue(cells.Contains("A1"));
            Assert.IsTrue(cells.Contains("A2"));
            Assert.IsTrue(cells.Contains("A3"));

            Assert.AreEqual(ss.GetCellContents("A1"), fromJson.GetCellContents("A1"));
            Assert.AreEqual(ss.GetCellContents("A2"), fromJson.GetCellContents("A2"));
            Assert.AreEqual(ss.GetCellContents("A3"), fromJson.GetCellContents("A3"));
        }

        /// <summary>
        /// Save method should throw exception if the input path is an empty string
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod33()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("");
        }

        /// <summary>
        /// Saving an empty spreadsheet and then deserialize it should also result in an empty spreadsheet
        /// </summary>
        [TestMethod]
        public void TestMethod34()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("emptySS.txt");

            AbstractSpreadsheet fromJson = new Spreadsheet("emptySS.txt", s => true, s => s, "default");
            IEnumerable<string> nonEmpty = fromJson.GetNamesOfAllNonemptyCells();
            Assert.AreEqual(0, nonEmpty.Count());
        }

        /// <summary>
        /// Test all aspects of our spreadsheet
        /// </summary>
        [TestMethod]
        public void StressTest2()
        {
            // Set up a spreadsheet manually
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "5");
            ss.SetContentsOfCell("a2", "= a1 + 2");
            ss.SetContentsOfCell("a3", "= a2 + 3");

            // If a spreadsheet is saved then its Changed property is false
            ss.Save("ss.txt");
            Assert.IsFalse(ss.Changed);

            // Duplicate the spreadsheet
            AbstractSpreadsheet fromJson = new Spreadsheet("ss.txt", s => Char.IsUpper(s, 0), s => s.ToUpper(), "default");
            Assert.AreEqual(5.0, fromJson.GetCellValue("A1"));
            Assert.AreEqual(7.0, fromJson.GetCellValue("A2"));
            Assert.AreEqual(10.0, fromJson.GetCellValue("A3"));

            // Change the duplicated spreadsheet
            fromJson.SetContentsOfCell("A1", "Hello");
            Assert.IsTrue(fromJson.GetCellValue("A2") is FormulaError);
            Assert.IsTrue(fromJson.GetCellValue("A2") is FormulaError);
        }
    }
}