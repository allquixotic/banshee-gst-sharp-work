using System;
using System.Reflection;
using NUnit.Framework;
using Hyena.Data.Sqlite;

[TestFixture]
public class SqliteCommandTests
{
    [Test]
    public void TestIdentifiesParameters ()
    {
        HyenaSqliteCommand cmd = null;
        try {
            cmd = new HyenaSqliteCommand ("select foo from bar where baz = ?, bbz = ?, this = ?",
                "a", 32);
            Assert.Fail ("Should not have been able to pass 2 values to ApplyValues without exception");
        } catch {}

        try {
            cmd = new HyenaSqliteCommand ("select foo from bar where baz = ?, bbz = ?, this = ?",
                "a", 32, "22");
        } catch {
            Assert.Fail ("Should have been able to pass 3 values to ApplyValues without exception");
        }

        Assert.AreEqual ("select foo from bar where baz = 'a', bbz = 32, this = '22'", GetGeneratedSql (cmd));
    }

    [Test]
    public void TestConstructor ()
    {
        HyenaSqliteCommand cmd = new HyenaSqliteCommand ("select foo from bar where baz = ?, bbz = ?, this = ?", "a", 32, "22");
        Assert.AreEqual ("select foo from bar where baz = 'a', bbz = 32, this = '22'", GetGeneratedSql (cmd));
    }

    [Test]
    public void TestCultureInvariant ()
    {
        HyenaSqliteCommand cmd = new HyenaSqliteCommand ("select foo from bar where baz = ?", 32.2);
        Assert.AreEqual ("select foo from bar where baz = 32.2", GetGeneratedSql (cmd));
    }

    [Test]
    public void TestParameterSerialization ()
    {
        HyenaSqliteCommand cmd = new HyenaSqliteCommand ("select foo from bar where baz = ?");

        Assert.AreEqual ("select foo from bar where baz = NULL", GetGeneratedSql (cmd, null));
        Assert.AreEqual ("select foo from bar where baz = 'It''s complicated, \"but\" ''''why not''''?'", GetGeneratedSql (cmd, "It's complicated, \"but\" ''why not''?"));
        Assert.AreEqual ("select foo from bar where baz = 0", GetGeneratedSql (cmd, new DateTime (1970, 1, 1).ToLocalTime ()));
        Assert.AreEqual ("select foo from bar where baz = 931309200", GetGeneratedSql (cmd, new DateTime (1999, 7, 7).ToLocalTime ()));
        Assert.AreEqual ("select foo from bar where baz = 555.55", GetGeneratedSql (cmd, 555.55f));
        Assert.AreEqual ("select foo from bar where baz = 555.55", GetGeneratedSql (cmd, 555.55));
        Assert.AreEqual ("select foo from bar where baz = 555", GetGeneratedSql (cmd, 555));
        Assert.AreEqual ("select foo from bar where baz = 1", GetGeneratedSql (cmd, true));
        Assert.AreEqual ("select foo from bar where baz = 0", GetGeneratedSql (cmd, false));

        HyenaSqliteCommand cmd2 = new HyenaSqliteCommand ("select foo from bar where baz = ?, bar = ?, boo = ?");
        Assert.AreEqual ("select foo from bar where baz = NULL, bar = NULL, boo = 22", GetGeneratedSql (cmd2, null, null, 22));

        HyenaSqliteCommand cmd3 = new HyenaSqliteCommand ("select foo from bar where id in (?) and foo not in (?)");
        Assert.AreEqual ("select foo from bar where id in (1,2,4) and foo not in ('foo','baz')",
                GetGeneratedSql (cmd3, new int [] {1, 2, 4}, new string [] {"foo", "baz"}));
    }

    static PropertyInfo tf = typeof(HyenaSqliteCommand).GetProperty ("CurrentSqlText", BindingFlags.Instance | BindingFlags.NonPublic);
    private static string GetGeneratedSql (HyenaSqliteCommand cmd, params object [] p)
    {
        return tf.GetValue ((new HyenaSqliteCommand (cmd.Text, p)), null) as string;
    }

    private static string GetGeneratedSql (HyenaSqliteCommand cmd)
    {
        return tf.GetValue (cmd, null) as string;
    }
}
