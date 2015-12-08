/*
 *
 * NDbUnit
 * Copyright (C)2005 - 2011
 * http://code.google.com/p/ndbunit
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System.Data;
using System.IO;
using NDbUnit.Core;
using NUnit.Framework;

namespace NDbUnit.Test
{
    [TestFixture]
    public abstract class IntegationTestBase
    {

        protected abstract INDbUnitTest GetNDbUnitTest();

        protected abstract string GetXmlFilename();

        protected abstract string GetXmlModFilename();

        protected abstract string GetXmlRefreshFilename();

        protected abstract string GetXmlSchemaFilename();

        private FileStream ReadOnlyStreamFromFilename(string filename)
        {
            return new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        private DataSet BuildDataSet(string dataFilename = null)
        {
            var dataSet = new DataSet();
            dataSet.ReadXmlSchema(ReadOnlyStreamFromFilename(GetXmlSchemaFilename()));

            if (dataFilename != null)
            {
                dataSet.ReadXml(ReadOnlyStreamFromFilename(dataFilename));
            }

            return dataSet;
        }
    }
}
