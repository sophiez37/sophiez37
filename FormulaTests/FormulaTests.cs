// Created by: Phuong Anh Nguyen
// Date: Sep 15 2023
using SpreadsheetUtilities;
using System.Reflection.Metadata.Ecma335;

namespace FormulaTests
{
    /// <summary>
    /// This class contains testers for Formula class.
    /// </summary>
    [TestClass]
    public class FormulaTests
    {
        /// <summary>
        /// Constructor should throw exception for invalid token
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod1()
        {
            Formula f = new Formula(" 1 + @ + $ + 1");
        }

        /// <summary>
        /// Constructor should throw exception for invalid variable for a formula that has a variable
        /// starting with a lowercase letter while isValid asks for a variable starting with uppercase letter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod2()
        {
            Formula f = new Formula("1 + a1", s => s, s => Char.IsUpper(s, 0));
        }

        /// <summary>
        /// Constructor should throw exception for invalid formula that has the first token of an operator
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod3()
        {
            Formula f = new Formula("+(1 - a1)");
        }

        /// <summary>
        /// Constructor should throw exception for invalid formula that has the first token of a closing parenthesis
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod4()
        {
            Formula f = new Formula(")(1 - a1)");
        }

        /// <summary>
        /// Constructor should throw exception for invalid formula that has the last token of an operator
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod5()
        {
            Formula f = new Formula("(1 - a1)*");
        }

        /// <summary>
        /// Constructor should throw exception for invalid formula that has the last token of a opening parenthesis
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod6()
        {
            Formula f = new Formula("(1 - a1)(");
        }

        /// <summary>
        /// Constructor should throw exception for formula not having at least one token
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod7()
        {
            Formula f = new Formula("");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates parenthesis following rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod8()
        {
            Formula f = new Formula("(-6 + a1)/4");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates operator following rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod9()
        {
            Formula f = new Formula("3 + () / 4");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates extra following rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod10()
        {
            Formula f = new Formula("(6 + a1)4");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates extra following rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod11()
        {
            Formula f = new Formula("3 + 4 a1");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates right parentheses rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod12()
        {
            Formula f = new Formula("(a1 - 4)) * 2)");
        }

        /// <summary>
        /// Constructor should throw exception because formula violates balanced parentheses rule
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMethod13()
        {
            Formula f = new Formula("((a2 - 3 + 4)");
        }

        /// <summary>
        /// Constructor should normalize scientific notation and variable
        /// </summary>
        [TestMethod]
        public void TestMethod14()
        {
            Formula f = new Formula("(  1e2 +   a1)    /4", s => s.ToUpper(), s => true);
            Assert.AreEqual("(100+A1)/4", f.ToString());
        }

        /// <summary>
        /// Constructor should normalize scientific notation and variable
        /// </summary>
        [TestMethod]
        public void TestMethod15()
        {
            Formula f = new Formula("_A_1 + 4.0", s => s.ToLower(), s => true);
            Assert.AreEqual("_a_1+4", f.ToString());
        }

        /// <summary>
        /// Evaluate a formula containing only one number should return that number
        /// </summary>
        [TestMethod]
        public void TestMethod16()
        {
            Formula f = new Formula("3");
            Assert.AreEqual(3.0, f.Evaluate(s => 0));
        }

        /// <summary>
        /// Evaluate a formula containing only one variable should return that variable's value
        /// </summary>
        [TestMethod]
        public void TestMethod17()
        {
            Formula f = new Formula("b2", s => s, s => true);
            Assert.AreEqual(6.0, f.Evaluate(s => 6));
        }

        /// <summary>
        /// Evaluate a formula containing only numbers
        /// </summary>
        [TestMethod]
        public void TestMethod18()
        {
            Formula f = new Formula("3/(  1  + 1) *(9  -6   )");
            Assert.AreEqual(4.5, f.Evaluate(s => 0));
        }

        /// <summary>
        /// Evaluate a formula containing only variables
        /// </summary>
        [TestMethod]
        public void TestMethod19()
        {
            Formula f = new Formula("a1 + a3 / (A1 * a2)", s => s.ToUpper(), s => true);
            Assert.AreEqual(4.25, f.Evaluate(s => { if (s.Equals("A1")) return 4; else return 1; }));
        }

        /// <summary>
        /// Evaluate a formula containing both numbers and variables
        /// </summary>
        [TestMethod]
        public void TestMethod20()
        {
            Formula f = new Formula("a1 * (3 + a2) / 4 - 12");
            Assert.AreEqual(-8.0, f.Evaluate(s => { if (s.Equals("a1")) return 2; else return 5; }));
        }

        /// <summary>
        /// Evaluate a formula containing both numbers and variables
        /// </summary>
        [TestMethod]
        public void TestMethod21()
        {
            Formula f = new Formula("a1 * 5 - a2");
            Assert.AreEqual(4.0, f.Evaluate(s => { if (s.Equals("a1")) return 1.8; else return 5; }));
        }

        /// <summary>
        /// Evaluate a formula containing both numbers and variables
        /// </summary>
        [TestMethod]
        public void TestMethod22()
        {
            Formula f = new Formula("a4 + b1 - 3");
            Assert.AreEqual(7.0, f.Evaluate(s => 5));
        }

        /// <summary>
        /// Evaluate a formula containing both numbers and variables
        /// </summary>
        [TestMethod]
        public void TestMethod23()
        {
            Formula f = new Formula("(3 + 4) / a1");
            Assert.AreEqual(1.0, f.Evaluate(s => 7));
        }

        /// <summary>
        /// Division by 0 should return FormulaError
        /// </summary>
        [TestMethod]

        public void TestMethod24()
        {
            Formula f = new Formula("3/b1", s => s.ToUpper(), s => Char.IsUpper(s, 0));
            Assert.IsTrue(f.Evaluate(s => 0) is FormulaError);
        }

        /// <summary>
        /// Division by 0 should return FormulaError
        /// </summary>
        [TestMethod]

        public void TestMethod25()
        {
            Formula f = new Formula("3/(5-b1)", s => s.ToUpper(), s => Char.IsUpper(s, 0));
            Assert.IsTrue(f.Evaluate(s => 5) is FormulaError);
        }

        /// <summary>
        /// Division by 0 should return FormulaError
        /// </summary>
        [TestMethod]
        public void TestMethod26()
        {
            Formula f = new Formula("(3 - c1) / 0");
            Assert.IsTrue(f.Evaluate(s => 3) is FormulaError);
        }

        /// <summary>
        /// Return FormulaError if the variable's value is unidentified (lookup throws ArgumentException)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        [TestMethod]
        public void TestMethod27()
        {
            Formula f = new Formula("3 + c3");
            Assert.IsTrue(f.Evaluate(s => throw new ArgumentException()) is FormulaError);
        }

        /// <summary>
        /// GetVariables should return all the variables in the formula
        /// </summary>
        [TestMethod]
        public void TestMethod28()
        {
            Formula f = new Formula("a1 + A2 + a3");
            IEnumerable<string> variables = f.GetVariables();
            Assert.IsTrue(variables.Contains("a1"));
            Assert.IsTrue(variables.Contains("A2"));
            Assert.IsTrue(variables.Contains("a3"));
        }

        /// <summary>
        /// GetVariables should return the normalized version of the variables
        /// </summary>
        [TestMethod]
        public void TestMethod29()
        {
            Formula f = new Formula("a1*A2 - A1*a3", s => s.ToUpper(), s => true);
            IEnumerable<string> variables = f.GetVariables();
            Assert.IsTrue(variables.Contains("A1"));
            Assert.IsTrue(variables.Contains("A2"));
            Assert.IsTrue(variables.Contains("A3"));
        }

        /// <summary>
        /// GetVariables should contain only one copy of a variable
        /// </summary>
        [TestMethod]
        public void TestMethod30()
        {
            Formula f = new Formula("A1 + A1 + A4 ", s => s.ToLower(), s => Char.IsLower(s, 0));
            IEnumerable<string> variables = f.GetVariables();
            Assert.AreEqual(2, variables.Count());
            Assert.IsTrue(variables.Contains("a1"));
            Assert.IsTrue(variables.Contains("a4"));
        }

        /// <summary>
        /// Equals should return false if input is null
        /// </summary>
        [TestMethod]
        public void TestMethod31()
        {
            Formula f = new Formula("3 + _a1");
            Assert.IsFalse(f.Equals(null));
        }

        /// <summary>
        /// Equals should return false if input is not of type Formula
        /// </summary>
        [TestMethod]
        public void TestMethod32()
        {
            Formula f = new Formula("3 + _a1");
            Assert.IsFalse(f.Equals("3 + _a1"));
        }

        /// <summary>
        /// Two formulae with the same tokens in the same order must be equal
        /// </summary>
        [TestMethod]
        public void TestMethod33()
        {
            Formula f1 = new Formula("(3+a1)*a2");
            Formula f2 = new Formula("(   3  + a1)  *     a2");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1 == f2);
            Assert.IsFalse(f1 != f2);
        }

        /// <summary>
        /// After normalizing variables and parsing numbers, the two formulae must be equal
        /// </summary>
        [TestMethod]
        public void TestMethod34()
        {
            Formula f1 = new Formula("1000.0 + a1", s => s.ToUpper(), s => true);
            Formula f2 = new Formula("1e3    +     A1", s => s.ToUpper(), s => true);
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1 == f2);
            Assert.IsFalse(f1 != f2);
        }

        /// <summary>
        /// Two formulae with the different tokens shouldn't be equal
        /// </summary>
        [TestMethod]
        public void TestMethod35()
        {
            Formula f1 = new Formula("((3+a1)*a2)");
            Formula f2 = new Formula("(   3  + a1)  *     (a2)");
            Assert.IsFalse(f1.Equals(f2));
            Assert.IsFalse(f1 == f2);
            Assert.IsTrue(f1 != f2);
        }

        /// <summary>
        /// Without normalizing variables, the two formulae with different variable format shouldn't be equal
        /// </summary>
        [TestMethod]
        public void TestMethod36()
        {
            Formula f1 = new Formula("1 + a1");
            Formula f2 = new Formula("1 + A1");
            Assert.IsFalse(f1.Equals(f2));
            Assert.IsFalse(f1 == f2);
            Assert.IsTrue(f1 != f2);
        }

        /// <summary>
        /// Numbers of the same value after normalizing should represent the same token
        /// </summary>
        [TestMethod]
        public void TestMethod37()
        {
            Formula f1 = new Formula("10");
            Formula f2 = new Formula("10.000");
            Formula f3 = new Formula("1e1");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1.Equals(f3));
            Assert.IsTrue(f2.Equals(f3));
        }

        /// <summary>
        /// Equal formulae should have the same hash code
        /// </summary>
        [TestMethod]
        public void TestMethod38()
        {
            Formula f1 = new Formula("1.0 + a1", s => s.ToLower(), s => true);
            Formula f2 = new Formula("1.000 + A1", s => s.ToLower(), s => true);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        /// <summary>
        /// Non-equal formulae shouldn't have the same hash code
        /// </summary>
        [TestMethod]
        public void TestMethod39()
        {
            Formula f1 = new Formula("3 + a1");
            Formula f2 = new Formula("(3 + a1)");
            Assert.IsFalse(f1.GetHashCode() == f2.GetHashCode());
        }

        /// <summary>
        /// Equal formulae should have the same hash code
        /// </summary>
        [TestMethod]
        public void TestMethod40()
        {
            Formula f1 = new Formula("(1-    AA11) *   1e2", s => s.ToLower(), s => true);
            Formula f2 = new Formula("(1.00    - aa11)*100", s => s.ToLower(), s => true);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        /// <summary>
        /// The result of ToString if being passed to constructor will produce
        /// a Formula f such that this.Equals(f)
        /// </summary>
        [TestMethod]
        public void TestMethod41()
        {
            Formula f1 = new Formula("(4     - a1  ) *(3  +a2   )/ A3 ");
            string s = f1.ToString();
            Formula f2 = new Formula(s);
            Assert.AreEqual(f1, f2);
        }

        /// <summary>
        /// The result of ToString if being passed to constructor will produce
        /// a Formula f such that this.Equals(f)
        /// </summary>
        [TestMethod]
        public void TestMethod42()
        {
            Formula f1 = new Formula("a1 + 1.000", s => s.ToUpper(), s => true);
            string s = f1.ToString();
            Formula f2 = new Formula(s);
            Assert.AreEqual(f1, f2);
        }
    }
}