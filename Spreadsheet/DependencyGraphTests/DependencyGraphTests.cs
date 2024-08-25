// Modified by Phuong Anh Nguyen
// Date: September 5 2023

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace DevelopmentTests;

/// <summary>
///This is a test class for DependencyGraphTest and is intended
///to contain all DependencyGraphTest Unit Tests (once completed by the student)
///</summary>
[TestClass()]
public class DependencyGraphTest
{
    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyTest()
    {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependencies);
    }

    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyRemoveTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(1, t.NumDependencies);
        t.RemoveDependency("x", "y");
        Assert.AreEqual(0, t.NumDependencies);
    }

    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void EmptyEnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("x", e1.Current);
        IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
        Assert.IsTrue(e2.MoveNext());
        Assert.AreEqual("y", e2.Current);
        t.RemoveDependency("x", "y");
        Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
        Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
    }

    /// <summary>
    ///Replace on an empty DG shouldn't fail
    ///</summary>
    [TestMethod()]
    public void SimpleReplaceTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(t.NumDependencies, 1);
        t.RemoveDependency("x", "y");
        t.ReplaceDependents("x", new HashSet<string>());
        t.ReplaceDependees("y", new HashSet<string>());
    }

    ///<summary>
    ///It should be possibe to have more than one DG at a time.
    ///</summary>
    [TestMethod()]
    public void StaticTest()
    {
        DependencyGraph t1 = new DependencyGraph();
        DependencyGraph t2 = new DependencyGraph();
        t1.AddDependency("x", "y");
        Assert.AreEqual(1, t1.NumDependencies);
        Assert.AreEqual(0, t2.NumDependencies);
    }

    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void SizeTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");
        Assert.AreEqual(4, t.NumDependencies);
    }

    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void EnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        // This is one of several ways of testing whether your IEnumerable
        // contains the right values. This does not require any particular
        // ordering of the elements returned.
        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void ReplaceThenEnumerate()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "b");
        t.AddDependency("a", "z");
        t.ReplaceDependents("b", new HashSet<string>());
        t.AddDependency("y", "b");
        t.ReplaceDependents("a", new HashSet<string>() { "c" });
        t.AddDependency("w", "d");
        t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
        t.ReplaceDependees("d", new HashSet<string>() { "b" });

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    ///Using lots of data
    ///</summary>
    [TestMethod()]
    public void StressTest()
    {
        // Dependency graph
        DependencyGraph t = new DependencyGraph();

        // A bunch of strings to use
        const int SIZE = 200;
        string[] letters = new string[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            letters[i] = ("" + (char)('a' + i));
        }

        // The correct answers
        HashSet<string>[] dents = new HashSet<string>[SIZE];
        HashSet<string>[] dees = new HashSet<string>[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            dents[i] = new HashSet<string>();
            dees[i] = new HashSet<string>();
        }

        // Add a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j++)
            {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 4; j < SIZE; j += 4)
            {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Add some back
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j += 2)
            {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove some more
        for (int i = 0; i < SIZE; i += 2)
        {
            for (int j = i + 3; j < SIZE; j += 3)
            {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Make sure everything is right
        for (int i = 0; i < SIZE; i++)
        {
            Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
            Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
        }
    }

    //----------------------------------Student's tests----------------------------------------------
    /// <summary>
    /// An empty graphhas no dependencies and no node has dependents or dependees
    /// </summary>
    [TestMethod()]
    public void EmptyGraphTest()
    {
        DependencyGraph dg = new();
        Assert.AreEqual(0, dg.NumDependencies);
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsFalse(dg.HasDependees("a1"));
    }

    /// <summary>
    /// Check the number of dependees of nodes in a simple graph
    /// </summary>
    [TestMethod()]
    public void SimpleGraphTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a2");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a2", "b2");

        Assert.AreEqual(3, dg.NumDependencies);
        Assert.AreEqual(0, dg.NumDependees("a1"));
        Assert.AreEqual(1, dg.NumDependees("a2"));
        Assert.AreEqual(2, dg.NumDependees("b2"));

        Assert.IsTrue(dg.HasDependents("a1"));
        Assert.IsTrue(dg.HasDependents("a2"));
        Assert.IsFalse(dg.HasDependents("b2"));
        Assert.IsFalse(dg.HasDependees("a1"));
        Assert.IsTrue(dg.HasDependees("a2"));
        Assert.IsTrue(dg.HasDependees("b2"));
    }

    /// <summary>
    /// The same dependency should be added only once into the graph
    /// </summary>
    [TestMethod()]
    public void RepeatedAddTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "b");

        Assert.AreEqual(1, dg.NumDependencies);
        Assert.AreEqual(1, dg.NumDependees("b"));
    }

    /// <summary>
    /// Remove method should do nothing if the specified pair doesn't exist in the graph
    /// </summary>
    [TestMethod()]
    public void RemoveNonExistedTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        Assert.AreEqual(2, dg.NumDependencies);

        dg.RemoveDependency("d", "e");
        Assert.AreEqual(2, dg.NumDependencies);
    }

    /// <summary>
    /// Check if the set of dependents and dependees of a node is returned correctly
    /// </summary>
    [TestMethod()]
    public void EnumerableTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("a", "d");
        dg.AddDependency("b", "c");
        dg.AddDependency("b", "d");
        dg.AddDependency("c", "d");
        Assert.AreEqual(6, dg.NumDependencies);

        IEnumerable<string> dependentsOfA = dg.GetDependents("a");
        Assert.IsTrue(dependentsOfA.Contains("b"));
        Assert.IsTrue(dependentsOfA.Contains("c"));
        Assert.IsTrue(dependentsOfA.Contains("d"));

        IEnumerator<string> enumA = dg.GetDependees("a").GetEnumerator();
        Assert.IsFalse(enumA.MoveNext());

        IEnumerator<string> enumD = dg.GetDependents("d").GetEnumerator();
        Assert.IsFalse(enumD.MoveNext());

        IEnumerable<string> dependeesOfD = dg.GetDependees("d");
        Assert.IsTrue(dependeesOfD.Contains("a"));
        Assert.IsTrue(dependeesOfD.Contains("b"));
        Assert.IsTrue(dependeesOfD.Contains("c"));

        Assert.AreEqual(0, dg.NumDependees("a"));
        Assert.AreEqual(1, dg.NumDependees("b"));
        Assert.AreEqual(2, dg.NumDependees("c"));
        Assert.AreEqual(3, dg.NumDependees("d"));
    }

    /// <summary>
    /// After it's only dependent is removed, a node doesn't have dependents anymore
    /// </summary>
    [TestMethod()]
    public void AddThenRemoveTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("x", "y");
        dg.AddDependency("y", "z");
        dg.AddDependency("x", "z");
        dg.AddDependency("z", "w");

        dg.RemoveDependency("y", "z");
        dg.RemoveDependency("w", "y");

        Assert.AreEqual(3, dg.NumDependencies);
        Assert.AreEqual(1, dg.NumDependees("z"));
        Assert.IsFalse(dg.HasDependents("y"));
    }

    /// <summary>
    /// After replacing dependents of a node, that node should has dependents of the new nodes,
    /// not the old nodes.
    /// </summary>
    [TestMethod()]
    public void ReplaceDependentsTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");
        dg.AddDependency("b", "e");
        Assert.AreEqual(4, dg.NumDependencies);

        dg.ReplaceDependents("a", new List<string>() {"d", "e", "f"});
        Assert.AreEqual(5, dg.NumDependencies);
        IEnumerable<string> dependentsOfA = dg.GetDependents("a");
        Assert.IsFalse(dependentsOfA.Contains("b"));
        Assert.IsFalse(dependentsOfA.Contains("c"));
        Assert.IsTrue(dependentsOfA.Contains("d"));
        Assert.IsTrue(dependentsOfA.Contains("e"));
        Assert.IsTrue(dependentsOfA.Contains("f"));

        Assert.AreEqual(2, dg.NumDependees("d"));
        Assert.AreEqual(2, dg.NumDependees("e"));
        Assert.AreEqual(1, dg.NumDependees("f"));
        Assert.AreEqual(0, dg.NumDependees("b"));
        Assert.AreEqual(0, dg.NumDependees("c"));
    }

    /// <summary>
    /// Call replace dependents on a node without any dependents should simply
    /// add dependents to that node
    /// </summary>
    [TestMethod()]
    public void ReplaceNoDependentsTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("b", "c");
        dg.ReplaceDependents("a", new List<string>() { "b", "c", "d" });

        Assert.AreEqual(4, dg.NumDependencies);
        Assert.IsTrue(dg.HasDependees("b"));
    }

    /// <summary>
    /// Call replace dependents on a node and an empty list should
    /// remove all the dependents of that node
    /// </summary>
    [TestMethod()]
    public void ReplaceDependentsWithEmptyListTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");
        dg.AddDependency("b", "e");
        dg.ReplaceDependents("b", new List<string>());

        Assert.AreEqual(2, dg.NumDependencies);
        Assert.IsFalse(dg.HasDependents("b"));
    }

    /// <summary>
    /// After replacing dependees of a node, that node should has dependees of the new nodes,
    /// not the old nodes.
    /// </summary>
    [TestMethod()]
    public void ReplaceDependeesTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("b", "a");
        dg.AddDependency("c", "a");
        dg.AddDependency("d", "b");
        dg.AddDependency("e", "b");
        Assert.AreEqual(4, dg.NumDependencies);

        dg.ReplaceDependees("a", new List<string>() { "d", "e", "f" });
        Assert.AreEqual(5, dg.NumDependencies);
        IEnumerable<string> dependeesOfA = dg.GetDependees("a");
        Assert.IsFalse(dependeesOfA.Contains("b"));
        Assert.IsFalse(dependeesOfA.Contains("c"));
        Assert.IsTrue(dependeesOfA.Contains("d"));
        Assert.IsTrue(dependeesOfA.Contains("e"));
        Assert.IsTrue(dependeesOfA.Contains("f"));
    }

    /// <summary>
    /// Call replace dependees on a node without any dependees should simply
    /// add dependees to that node
    /// </summary>
    [TestMethod()]
    public void ReplaceNoDependeesTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("b", "c");
        dg.ReplaceDependees("a", new List<string>() { "b", "c", "d" });

        Assert.AreEqual(4, dg.NumDependencies);
        Assert.AreEqual(3, dg.NumDependees("a"));
    }

    /// <summary>
    /// Call replace dependees on a node and an empty list should
    /// remove all the dependees of that node
    /// </summary>
    [TestMethod()]
    public void ReplaceDependeesWithEmptyListTest()
    {
        DependencyGraph dg = new();
        dg.AddDependency("b", "a");
        dg.AddDependency("c", "a");
        dg.AddDependency("d", "b");
        dg.AddDependency("e", "b");
        dg.ReplaceDependees("b", new List<string>());

        Assert.AreEqual(2, dg.NumDependencies);
        Assert.IsFalse(dg.HasDependees("b"));
    }
}