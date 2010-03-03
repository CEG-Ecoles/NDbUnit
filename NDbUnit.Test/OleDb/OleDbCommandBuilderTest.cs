﻿/*
 *
 * NDbUnit
 * Copyright (C)2005 - 2010
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

using System;
using System.Collections.Generic;
using System.Text;
using NDbUnit.Core.SqlClient;
using MbUnit.Framework;
using NDbUnit.Core;
using NDbUnit.Core.OleDb;

namespace NDbUnit.Test.SqlClient
{
    [TestFixture]
    class OleDbCommandBuilderTest : NDbUnit.Test.Common.DbCommandBuilderTestBase
    {
        public override IList<string> ExpectedDataSetTableNames
        {
            get
            {
                return new List<string>()
                {
                    "Role", "dbo.User", "UserRole" 
                };
            }
        }

        public override IList<string> ExpectedDeleteAllCommands
        {
            get
            {
                return new List<string>()
                {
                    "DELETE FROM [Role]",
                    "DELETE FROM [dbo].[User]",
                    "DELETE FROM [UserRole]"
                };
            }
        }

        public override IList<string> ExpectedDeleteCommands
        {
            get
            {
                return new List<string>()
                {
                    "DELETE FROM [Role] WHERE [ID]=?",
                    "DELETE FROM [dbo].[User] WHERE [ID]=?",
                    "DELETE FROM [UserRole] WHERE [UserID]=? AND [RoleID]=?"
                };
            }
        }

        public override IList<string> ExpectedInsertCommands
        {
            get
            {
                return new List<string>()
                {
                    "INSERT INTO [Role]([Name], [Description]) VALUES(?, ?)",
                    "INSERT INTO [dbo].[User]([FirstName], [LastName], [Age], [SupervisorID]) VALUES(?, ?, ?, ?)",
                    "INSERT INTO [UserRole]([UserID], [RoleID]) VALUES(?, ?)"
                };

            }
        }

        public override IList<string> ExpectedInsertIdentityCommands
        {
            get
            {
                return new List<string>()
                {
                    "INSERT INTO [Role]([ID], [Name], [Description]) VALUES(?, ?, ?)",
                    "INSERT INTO [dbo].[User]([ID], [FirstName], [LastName], [Age], [SupervisorID]) VALUES(?, ?, ?, ?, ?)",
                    "INSERT INTO [UserRole]([UserID], [RoleID]) VALUES(?, ?)"
                };
            }
        }

        public override IList<string> ExpectedSelectCommands
        {
            get
            {
                return new List<string>()
                {
                    "SELECT [ID], [Name], [Description] FROM [Role]",
                    "SELECT [ID], [FirstName], [LastName], [Age], [SupervisorID] FROM [dbo].[User]",
                    "SELECT [UserID], [RoleID] FROM [UserRole]"
                };
            }
        }

        public override IList<string> ExpectedUpdateCommands
        {
            get
            {
                return new List<string>()
                {
                    "UPDATE [Role] SET [Name]=?, [Description]=? WHERE [ID]=?",
                    "UPDATE [dbo].[User] SET [FirstName]=?, [LastName]=?, [Age]=?, [SupervisorID]=? WHERE [ID]=?",
                    "UPDATE [UserRole] SET [UserID]=?, [RoleID]=? WHERE [UserID]=? AND [RoleID]=?"
                };
            }
        }

        protected override IDbCommandBuilder GetDbCommandBuilder()
        {
            return new OleDbCommandBuilder(DbConnection.OleDbConnectionString);
        }

        protected override string GetXmlSchemaFilename()
        {
            return XmlTestFiles.OleDb.XmlSchemaFile;
        }

    }
}
