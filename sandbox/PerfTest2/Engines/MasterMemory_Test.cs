#pragma warning disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PerfTest2;

namespace TestPerfLiteDB
{
    public class MasterMemory_Test : ITest
    {
        private string _filename;
        private int _count;

        DatabaseSession _session;

        public int Count { get { return _count; } }
        public int FileLength { get { return (int)new FileInfo(_filename).Length; } }

        public MasterMemory_Test(int count)
        {
            _count = count;
            _filename = "mastermemorydatabase-" + Guid.NewGuid().ToString("n") + ".db";

        }

        public IEnumerable<TestDoc> MakeDoc()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                var v = new TestDoc
                {
                    id = doc["_id"].AsInt32,
                    name = doc["name"].AsString,
                    lorem = doc["lorem"].AsString
                };

                yield return v;
            }
        }

        public void Insert()
        {
            _session = new DatabaseSession();
            var transaction = _session.BeginTransaction();
            transaction.Diff(MakeDoc().ToArray());
            _session.Commit();
            var bytes = _session.Serialize();
            File.WriteAllBytes(_filename, bytes);
        }

        public void Bulk()
        {

        }

        public void CreateIndex()
        {

        }

        public void Dispose()
        {

        }

        public void Prepare()
        {

        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                //TestDoc d;
                _session.Tables.TestDocTable.FindByid(i);
            }
        }

        public void Update()
        {

        }
    }
}
