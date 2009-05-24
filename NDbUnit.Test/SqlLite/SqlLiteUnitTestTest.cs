/*
 *
 * NDbUnit
 * Copyright (C)2005
 * http://code.google.com/p/ndbunit
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using NDbUnit.Core;
using NDbUnit.Core.SqlLite;
using MbUnit.Framework;

namespace NDbUnit.Test.SqlLite
{

    [TestFixture]
    public class SqlLiteUnitTestTest
    {
        private SqlLiteUnitTest _sqlLiteTest;

        [SetUp]
        public void SetUp()
        {
            _sqlLiteTest = new SqlLiteUnitTest(DbConnection.SqlLiteConnectionString);
            _sqlLiteTest.ReadXmlSchema(XmlTestFiles.Sqlite.XmlSchemaFile);
            _sqlLiteTest.ReadXml(XmlTestFiles.Sqlite.XmlFile);
            _sqlLiteTest.PerformDbOperation(DbOperationFlag.DeleteAll);
        }

        [Test]
        public void Test()
        {
        }

        [TearDown]
        public void tearDown()
        {
        }
    }
}