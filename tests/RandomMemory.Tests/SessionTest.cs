using System;
using RandomMemory.Tests.TestStructures;

namespace RandomMemory.Tests;

public class SessionTest
{
    Sample[] CreateData()
    {
        var data = new[]
        {
            new Sample { Id = 5, Age = 19, FirstName = "aaa", LastName = "foo" },
            new Sample { Id = 6, Age = 29, FirstName = "bbb", LastName = "foo" },
            new Sample { Id = 7, Age = 39, FirstName = "ccc", LastName = "foo" },
            new Sample { Id = 8, Age = 49, FirstName = "ddd", LastName = "foo" },
            new Sample { Id = 1, Age = 59, FirstName = "eee", LastName = "foo" },
            new Sample { Id = 2, Age = 89, FirstName = "aaa", LastName = "bar" },
            new Sample { Id = 3, Age = 79, FirstName = "be", LastName = "de" },
            new Sample { Id = 4, Age = 89, FirstName = "aaa", LastName = "tako" },
            new Sample { Id = 9, Age = 99, FirstName = "aaa", LastName = "ika" },
            new Sample { Id = 10, Age = 9, FirstName = "eee", LastName = "baz" },
        };
        return data;
    }

    byte[] CreateSerializedData()
    {
        var data = CreateData();
        return new DatabaseBuilder().Append(data).Build();
    }

    [Fact]
    public void CreateSession()
    {
        var filledSession = new DatabaseSession(CreateSerializedData());
        var emptySession = new DatabaseSession();
    }

    [Fact]
    public void ValidTransaction()
    {
        var session = new DatabaseSession(CreateSerializedData());
        var transaction = session.BeginTransaction();
        transaction.Diff(new Sample(Id: 100, Age: 100, FirstName: "FirstName", LastName: "LastName"));
    }

    [Fact]
    public void TransactionStarted()
    {
        var session = new DatabaseSession(CreateSerializedData());
        var transaction = session.BeginTransaction();
        transaction.Diff(new Sample(Id: 100, Age: 100, FirstName: "FirstName", LastName: "LastName"));
        Assert.True(session.IsTransactionStarted);
    }

    [Fact]
    public void InvalidTransaction()
    {
        var session = new DatabaseSession(CreateSerializedData());
        var transaction = session.BeginTransaction();
        transaction.Diff(new Sample(Id: 100, Age: 100, FirstName: "FirstName", LastName: "LastName"));

        try
        {
            //Must cause an exception!
            session.BeginTransaction();
            Assert.True(false);
        }
        catch (InvalidOperationException)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void Commit()
    {
        var session = new DatabaseSession(CreateSerializedData());
        var transaction = session.BeginTransaction();
        transaction.Diff(new Sample(Id: 100, Age: 100, FirstName: "FirstName", LastName: "LastName"));
        session.Commit();
    }
}
