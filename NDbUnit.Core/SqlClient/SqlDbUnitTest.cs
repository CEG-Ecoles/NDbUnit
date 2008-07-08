/*
 *
 * NDbUnit
 * Copyright (C)2005
 * http://www.ndbunit.org
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

using System;
using System.Data;
using System.Data.SqlClient;

namespace NDbUnit.Core.SqlClient
{
	/// <summary>
	/// The Sql Server unit test data adapter.
	/// </summary>
	/// <example>
	/// <code>
	/// string connectionString = "Persist Security Info=False;Integrated Security=SSPI;database=testdb;server=V-AL-DIMEOLA\NETSDK";
	/// SqlDbUnitTest sqlDbUnitTest = new SqlDbUnitTest(connectionString);
	/// string xmlSchemaFile = "User.xsd";
	/// string xmlFile = "User.xml";
	/// sqlDbUnitTest.ReadXmlSchema(xmlSchemaFile);
	/// sqlDbUnitTest.ReadXml(xmlFile);
	/// sqlDbUnitTest.PerformDbOperation(DbOperation.CleanInsertIdentity);
	/// </code>
	/// <seealso cref="INDbUnitTest"/>
	/// </example>
	public class SqlDbUnitTest : NDbUnitTest
	{
		#region Private Fields

		SqlDbCommandBuilder _sqlDbCommandBuilder = null;
		SqlDbOperation _sqlDbOperation = null;

		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlDbUnitTest"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string 
		/// used to open the database.
		/// <seealso cref="System.Data.IDbConnection.ConnectionString"/></param>
		public SqlDbUnitTest(string connectionString)
		{
			_sqlDbCommandBuilder = new SqlDbCommandBuilder(connectionString);
			_sqlDbOperation = new SqlDbOperation();
		}

		#endregion

		#region Protected Overrides

		protected override IDbCommandBuilder GetDbCommandBuilder()
		{
			return _sqlDbCommandBuilder;
		}

		protected override IDbOperation GetDbOperation()
		{
			return _sqlDbOperation;
		}

		protected override void OnGetDataSetFromDb(string tableName, ref System.Data.DataSet dsToFill, System.Data.IDbConnection dbConnection)
		{
			SqlCommand selectCommand = _sqlDbCommandBuilder.GetSelectCommand(tableName);
			selectCommand.Connection = dbConnection as SqlConnection;
			SqlDataAdapter adapter = new SqlDataAdapter(selectCommand);
			adapter.Fill(dsToFill, tableName);
		}

		#endregion
	}
}
