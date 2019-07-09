﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class BasicEvaluatorTests : TestBase
    {
        [TestMethod]
        public void SimpleVNextTest()
        {
            var query = 
                "table DummyTable {" +
                "   Name 'System.String'" +
                "};" +
                "couple #A.Entities with table DummyTable as SourceOfDummyRows;" +
                "select Name from SourceOfDummyRows();";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABCAACBA"),
                        new BasicEntity("AAeqwgQEW"),
                        new BasicEntity("XXX"),
                        new BasicEntity("dadsqqAA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("ABCAACBA", table[0].Values[0]);
            Assert.AreEqual("AAeqwgQEW", table[1].Values[0]);
            Assert.AreEqual("XXX", table[2].Values[0]);
            Assert.AreEqual("dadsqqAA", table[3].Values[0]);
        }

        [TestMethod]
        public void LikeOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABCAACBA"), new BasicEntity("AAeqwgQEW"), new BasicEntity("XXX"),
                        new BasicEntity("dadsqqAA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABCAACBA", table[0].Values[0]);
            Assert.AreEqual("AAeqwgQEW", table[1].Values[0]);
            Assert.AreEqual("dadsqqAA", table[2].Values[0]);
        }

        [TestMethod]
        public void NotLikeOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name not like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABCAACBA"), new BasicEntity("AAeqwgQEW"), new BasicEntity("XXX"),
                        new BasicEntity("dadsqqAA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("XXX", table[0].Values[0]);
        }

        [TestMethod]
        public void RLikeOperatorTest()
        {
            var query = @"select Name from #A.Entities() where Name rlike '^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("12@hostname.com"),
                        new BasicEntity("ma@hostname.comcom"),
                        new BasicEntity("david.jones@proseware.com"),
                        new BasicEntity("ma@hostname.com")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("12@hostname.com", table[0].Values[0]);
            Assert.AreEqual("david.jones@proseware.com", table[1].Values[0]);
            Assert.AreEqual("ma@hostname.com", table[2].Values[0]);
        }

        [TestMethod]
        public void NotRLikeOperatorTest()
        {
            var query = @"select Name from #A.Entities() where Name not rlike '^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("12@hostname.com"),
                        new BasicEntity("ma@hostname.comcom"),
                        new BasicEntity("david.jones@proseware.com"),
                        new BasicEntity("ma@hostname.com")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("ma@hostname.comcom", table[0].Values[0]);
        }

        [TestMethod]
        public void ComplexWhere1Test()
        {
            var query =
                $"select Population from #A.Entities() where Population > 0 and Population - 100 > -1.5d and Population - 100 < 1.5d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 99),
                        new BasicEntity("KATOWICE", "POLAND", 101),
                        new BasicEntity("BERLIN", "GERMANY", 50)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Population", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(99m, table[0].Values[0]);
            Assert.AreEqual(101m, table[1].Values[0]);
        }

        [TestMethod]
        public void MultipleAndOperatorTest()
        {
            var query =
                "select Name from #A.Entities() where IndexOf(Name, 'A') = 0 and IndexOf(Name, 'B') = 1 and IndexOf(Name, 'C') = 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[] {new BasicEntity("A"), new BasicEntity("AB"), new BasicEntity("ABC"), new BasicEntity("ABCD")}
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABC", table[0].Values[0]);
            Assert.AreEqual("ABCD", table[1].Values[0]);
        }

        [TestMethod]
        public void MultipleOrOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name = 'ABC' or Name = 'ABCD' or Name = 'A'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[] {new BasicEntity("A"), new BasicEntity("AB"), new BasicEntity("ABC"), new BasicEntity("ABCD")}
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("A", table[0].Values[0]);
            Assert.AreEqual("ABC", table[1].Values[0]);
            Assert.AreEqual("ABCD", table[2].Values[0]);
        }

        [TestMethod]
        public void AddOperatorWithStringsTurnsIntoConcatTest()
        {
            var query = "select 'abc' + 'cda' from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("'abc' + 'cda'", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("abccda", table[0].Values[0]);
        }

        [TestMethod]
        public void ContainsStringsTest()
        {
            var query = "select Name from #A.Entities() where Name contains ('ABC', 'CdA', 'CDA', 'DDABC')";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABC"),
                        new BasicEntity("XXX"),
                        new BasicEntity("CDA"),
                        new BasicEntity("DDABC")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABC", table[0].Values[0]);
            Assert.AreEqual("CDA", table[1].Values[0]);
            Assert.AreEqual("DDABC", table[2].Values[0]);
        }

        [TestMethod]
        public void CanPassComplexArgumentToFunctionTest()
        {
            var query = "select NothingToDo(Self) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001")
                        {
                            Name = "ABBA",
                            Country = "POLAND",
                            City = "CRACOV",
                            Money = 1.23m,
                            Month = "JANUARY",
                            Population = 250
                        }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("NothingToDo(Self)", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void TableShouldReturnComplexTypeTest()
        {
            var query = "select Self from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001")
                        {
                            Name = "ABBA",
                            Country = "POLAND",
                            City = "CRACOV",
                            Money = 1.23m,
                            Month = "JANUARY",
                            Population = 250
                        }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Self", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void SimpleShowAllColumnsTest()
        {
            var entity = new BasicEntity("001")
            {
                Name = "ABBA",
                Country = "POLAND",
                City = "CRACOV",
                Money = 1.23m,
                Month = "JANUARY",
                Population = 250,
                Time = DateTime.MaxValue,
                Id = 5,
                NullableValue = null
            };
            var query = "select 1, *, Name as Name2, ToString(Self) as SelfString from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {entity}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();
            Assert.AreEqual("1", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Name", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("City", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("Country", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual("Population", table.Columns.ElementAt(4).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(4).ColumnType);

            Assert.AreEqual("Self", table.Columns.ElementAt(5).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(5).ColumnType);

            Assert.AreEqual("Money", table.Columns.ElementAt(6).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(6).ColumnType);

            Assert.AreEqual("Month", table.Columns.ElementAt(7).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(7).ColumnType);

            Assert.AreEqual("Time", table.Columns.ElementAt(8).ColumnName);
            Assert.AreEqual(typeof(DateTime), table.Columns.ElementAt(8).ColumnType);

            Assert.AreEqual("Id", table.Columns.ElementAt(9).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(9).ColumnType);

            Assert.AreEqual("NullableValue", table.Columns.ElementAt(10).ColumnName);
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(10).ColumnType);

            Assert.AreEqual("Name2", table.Columns.ElementAt(11).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(11).ColumnType);

            Assert.AreEqual("SelfString", table.Columns.ElementAt(12).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(12).ColumnType);

            Assert.AreEqual(1, table.Count);

            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[0]);
            Assert.AreEqual("ABBA", table[0].Values[1]);
            Assert.AreEqual("CRACOV", table[0].Values[2]);
            Assert.AreEqual("POLAND", table[0].Values[3]);
            Assert.AreEqual(250m, table[0].Values[4]);
            Assert.AreEqual(entity, table[0].Values[5]);
            Assert.AreEqual(1.23m, table[0].Values[6]);
            Assert.AreEqual("JANUARY", table[0].Values[7]);
            Assert.AreEqual(DateTime.MaxValue, table[0].Values[8]);
            Assert.AreEqual(5, table[0].Values[9]);
            Assert.AreEqual(null, table[0].Values[10]);
            Assert.AreEqual("ABBA", table[0].Values[11]);
            Assert.AreEqual("TEST STRING", table[0].Values[12]);
        }

        [TestMethod]
        public void SimpleAccessArrayTest()
        {
            var query = @"select Self.Array[2] from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Array[2]", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(2, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleAccessObjectTest()
        {
            var query = @"select Self.Array from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Array", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int[]), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void AccessObjectTest()
        {
            var query = @"select Self.Self.Array from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Self.Array", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int[]), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void SimpleAccessObjectIncrementTest()
        {
            var query = @"select Inc(Self.Array[2]) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Inc(Self.Array[2])", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(long), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(Convert.ToInt64(3), table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt64(3), table[1].Values[0]);
        }

        [TestMethod]
        public void WhereWithOrTest()
        {
            var query = @"select Name from #A.Entities() where Name = '001' or Name = '005'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleQueryTest()
        {
            var query = @"select Name as 'x1' from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void IdentifiersInBracketTest()
        {
            var query = @"select [Name] as 'x1' from [#A].Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTest()
        {
            var query = @"select Name from #A.Entities() skip 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("003", table[0].Values[0]);
            Assert.AreEqual("004", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
            Assert.AreEqual("006", table[3].Values[0]);
        }

        [TestMethod]
        public void SimpleTakeTest()
        {
            var query = @"select Name from #A.Entities() take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"),
                        new BasicEntity("002"),
                        new BasicEntity("003"),
                        new BasicEntity("004"),
                        new BasicEntity("005"),
                        new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTakeTest()
        {
            var query = @"select Name from #A.Entities() skip 1 take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("003", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void SimpleTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
            Assert.AreEqual("005", table[4].Values[0]);
            Assert.AreEqual("006", table[5].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100 take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void ColumnNamesSimpleTest()
        {
            var query =
                @"select Name as TestName, GetOne(), GetOne() as TestColumn, GetTwo(4d, 'test') from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new BasicEntity[] { }}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("TestName", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("GetOne()", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("TestColumn", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("GetTwo(4, 'test')", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);
        }

        [TestMethod]
        public void CallMethodWithTwoParametersTest()
        {
            var query = @"select Concat(Country, ToString(Population)) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA", 200)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Concat(Country, ToString(Population))", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("ABBA200", table[0].Values[0]);
        }

        [TestMethod]
        public void ColumnTypeDateTimeTest()
        {
            var query = "select Time from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity(DateTime.MinValue)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Time", table.Columns.ElementAt(0).ColumnName);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(DateTime.MinValue, table[0].Values[0]);
        }

        [TestMethod]
        public void SimpleRowNumberStatTest()
        {
            var query = @"select RowNumber() from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("001"),
                        new BasicEntity("002"),
                        new BasicEntity("003"),
                        new BasicEntity("004"),
                        new BasicEntity("005"),
                        new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual(1, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
            Assert.AreEqual(3, table[2].Values[0]);
            Assert.AreEqual(4, table[3].Values[0]);
            Assert.AreEqual(5, table[4].Values[0]);
            Assert.AreEqual(6, table[5].Values[0]);
        }

        [TestMethod]
        public void SelectDecimalWithoutMarkingNumberExplicitlyTest()
        {
            var query = "select 1.0, -1.0 from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("1.0", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("-1.0", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(1.0m, table[0].Values[0]);
            Assert.AreEqual(-1.0m, table[0].Values[1]);
        }

        [TestMethod]
        public void DescEntityTest()
        {
            var query = "desc #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Index", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("Type", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.IsTrue(table.Any(row => (string) row[0] == "Name" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "City" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Country" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Self" && (string) row[2] == "Traficante.TSQL.Evaluator.Tests.Core.Schema.BasicEntity"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Money" && (string) row[2] == "System.Decimal"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Month" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Time" && (string) row[2] == "System.DateTime"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Id" && (string) row[2] == "System.Int32"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "NullableValue" && (string) row[2] == "System.Nullable`1[System.Int32]"));
        }

        [TestMethod]
        public void DescMethodTest()
        {
            var query = "desc #A.entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("entities", table[0][0]);
        }

        [TestMethod]
        public void DescSchemaTest()
        {
            var query = "desc #A";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual(2, table.Count);

            Assert.AreEqual("empty", table[0][0]);
            Assert.AreEqual("entities", table[1][0]);
        }

        [TestMethod]
        public void AggregateValuesTest()
        {
            var query = @"select AggregateValues(Name) from #A.entities() a group by Name";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A"),
                        new BasicEntity("B")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();


            Assert.AreEqual("A", table[0][0]);
            Assert.AreEqual("B", table[1][0]);
        }

        [TestMethod]
        public void AggregateValuesParentTest()
        {
            var query = @"select AggregateValues(Name, 1) from #A.entities() a group by Name";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A"),
                        new BasicEntity("B")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("A,B", table[0][0]);
        }

        [TestMethod]
        public void CoalesceTest()
        {
            var query = @"select Coalesce('a', 'b', 'c', 'e', 'f') from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("a", table[0][0]);
        }

        [TestMethod]
        public void ChooseTest()
        {
            var query = @"select Choose(2, 'a', 'b', 'c', 'e', 'f') from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("c", table[0][0]);
        }

        [TestMethod]
        public void MatchWithRegexTest()
        {
            var query = @"select Match('\d{7}', Name) from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("3213213")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(true, table[0][0]);
        }

        [TestMethod]
        public void SubtractTwoAliasedValuesTest()
        {
            var query = "select a.Money - a.Money from #A.entities() a";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 2512m)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0m, table[0][0]);
        }

        [TestMethod]
        public void SubtractThreeAliasedValuesTest()
        {
            var query = "select (a.Money - a.Population) / a.Money from #A.entities() a";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 10 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0.9m, table[0][0]);
        }

        [TestMethod]
        public void FilterByComplexObjectAccessInWhereTest()
        {
            var query = "select Population from #A.entities() where Self.Money > 100";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 10 },
                        new BasicEntity("june", 200m) { Population = 20 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(20m, table[0][0]);
        }
        
        [TestMethod]
        public void CaseWhenSimpleTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then true" +
                "       else false" +
                "   end)" +
                "from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(false, table[0][0]);
            Assert.AreEqual(true, table[1][0]);
        }

        [TestMethod]
        public void CaseWhenWithLibraryMethodCallTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then entities.GetOne()" +
                "       else entities.Inc(entities.GetOne())" +
                "   end)" +
                "from #A.entities() entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(2m, table[0][0]);
            Assert.AreEqual(1m, table[1][0]);
        }

        [TestMethod]
        public void MultipleCaseWhenWithLibraryMethodCallTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then entities.GetOne()" +
                "       else entities.Inc(entities.GetOne())" +
                "   end)," +
                "   (case " +
                "       when Population <= 100d" +
                "       then entities.GetOne()" +
                "       else entities.Inc(entities.GetOne())" +
                "   end)" +
                "from #A.entities() entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(2m, table[0][0]);
            Assert.AreEqual(1m, table[0][1]);
            Assert.AreEqual(1m, table[1][0]);
            Assert.AreEqual(2m, table[1][1]);
        }

        [TestMethod]
        public void SelectWithAbsFunction()
        {
            var query = "select abs(Population) from #A.entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = -100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(100m, table[0][0]);
            Assert.AreEqual(200m, table[1][0]);;
        }

        [TestMethod]
        public void SelectGetDateFromTable()
        {
            var query = "select GetDate() from #A.entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = -100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.IsTrue(table[0][0] is DateTimeOffset);
            Assert.IsTrue(table[1][0] is DateTimeOffset);
        }

        [TestMethod]
        public void SelectGetDate()
        {
            var query = "select GetDate()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);

            Assert.IsTrue(table[0][0] is DateTimeOffset);
        }
    }
}