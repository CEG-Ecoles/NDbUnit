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

namespace NDbUnit.Test
{
    public class DbConnection
    {
        public static string SqlConnectionString
        {
            get { return @"Data Source=localhost\SQLServer2005;Database=testdb;Trusted_Connection=True"; }
        }

        public static string OleDbConnectionString
        {
            get { return @"Provider=SQLOLEDB;Data Source=localhost\SQLServer2005;Database=testdb;Integrated Security=SSPI"; }
        }

        public static string SqlLiteConnectionString
        {
            get { return @"Data Source=SqlLite\test.sqlite.db;Version=3;New=True"; }
        }

        public static string SqlCeConnectionString
        {
            get { return @"Data Source=SqlServerCe\testdb.sdf;Persist Security Info=False;"; }
        }
    }
}
