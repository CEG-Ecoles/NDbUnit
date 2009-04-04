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

using System;
using System.Data;
using System.Collections;
using System.Data.Common;

namespace NDbUnit.Core
{
    public abstract class DbOperation : IDbOperation
    {
        public virtual string QuotePrefix { get { return "";} }
        public virtual string QuoteSuffix { get { return ""; } }

        protected DataRow CloneDataRow(DataTable dataTable, DataRow dataRow)
        {
            DataRow dataRowClone = dataTable.NewRow();
            for (int i = 0; i < dataRow.ItemArray.Length; ++i)
            {
                dataRowClone[i] = dataRow[i];
            }

            return dataRowClone;
        }

        protected bool IsPrimaryKeyValueEqual(DataRow dataRow1, DataRow dataRow2, DataColumn[] primaryKey)
        {
            if (primaryKey.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < primaryKey.Length; ++i)
            {
                DataColumn dataColumn = primaryKey[i];
                // Primary key column value is not equal.
                if (!dataRow1[dataColumn.ColumnName].Equals(dataRow2[dataColumn.ColumnName]))
                {
                    return false;
                }
            }

            return true;
        }

        protected abstract DbDataAdapter CreateDbDataAdapter();
        protected abstract DbCommand CreateDbCommand(string cmdText);

        private void deleteCommon(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction,
                                  bool deleteAll)
        {
            Hashtable deletedTableColl = new Hashtable();

            DataSet dsSchema = dbCommandBuilder.GetSchema();

            DataSetTableIterator iterator = new DataSetTableIterator(dsSchema, true);

            foreach (DataTable dataTable in iterator)
            {
                deleteRecursive(ds, dataTable, dbCommandBuilder, dbTransaction, deletedTableColl, deleteAll);
            }
        }

        private void deleteRecursive(DataSet ds, DataTable dataTableSchema, IDbCommandBuilder dbCommandBuilder,
                                     IDbTransaction dbTransaction, Hashtable deletedTableColl, bool deleteAll)
        {
            // Table has already been deleted from.
            if (deletedTableColl.ContainsKey(dataTableSchema.TableName))
            {
                return;
            }

            // [20060724 - sdh] Move here (from end of method) to avoid infinite-loop when package has relation to itself
            // Table was deleted from in the database.
            deletedTableColl[dataTableSchema.TableName] = null;

            DataRelationCollection childRelations = dataTableSchema.ChildRelations;
            // The table has children.
            if (null != childRelations)
            {
                foreach (DataRelation childRelation in childRelations)
                {
                    // Must delete the child table first.
                    deleteRecursive(ds, childRelation.ChildTable, dbCommandBuilder, dbTransaction, deletedTableColl,
                                    deleteAll);
                }
            }

            if (deleteAll)
            {
                IDbCommand dbCommand = dbCommandBuilder.GetDeleteAllCommand(dataTableSchema.TableName);

                try
                {
                    OnDeleteAll(dbCommand, dbTransaction);
                }
                catch (DBConcurrencyException)
                {
                    // Swallow deletion of zero records.
                }
            }
            else
            {
                DataTable dataTable = ds.Tables[dataTableSchema.TableName];
                DataTable dataTableCopy = dataTable.Copy();
                dataTableCopy.AcceptChanges();

                foreach (DataRow dataRow in dataTableCopy.Rows)
                {
                    // Delete the row.
                    dataRow.Delete();
                }

                IDbCommand dbCommand = dbCommandBuilder.GetDeleteCommand(dataTableSchema.TableName);

                try
                {
                    OnDelete(dataTableCopy, dbCommand, dbTransaction);
                }
                catch (DBConcurrencyException)
                {
                    // Swallow deletion of zero records.
                }
            }
        }

        public void Delete(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {
            deleteCommon(ds, dbCommandBuilder, dbTransaction, false);
        }

        public void DeleteAll(IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {
            deleteCommon(null, dbCommandBuilder, dbTransaction, true);
        }

        public void Insert(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {
            insertCommon(ds, dbCommandBuilder, dbTransaction, false);
        }

        public void InsertIdentity(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {
            insertCommon(ds, dbCommandBuilder, dbTransaction, true);
        }

        public void Refresh(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {

            DataSetTableIterator iterator = new DataSetTableIterator(ds, false);

            foreach (DataTable dataTable in iterator)
            {
                OnRefresh(ds, dbCommandBuilder, dbTransaction, dataTable.TableName);
            }
        }

        public void Update(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction)
        {
            DataSet dsCopy = ds.Copy();
            dsCopy.AcceptChanges();

            DataSetTableIterator iterator = new DataSetTableIterator(dsCopy, true);

            foreach (DataTable dataTable in iterator)
            {
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    // Modify every table row.
                    dataRow.BeginEdit();
                    dataRow.EndEdit();
                }

                OnUpdate(dsCopy, dbCommandBuilder, dbTransaction, dataTable.TableName);
            }
        }

        private void insertCommon(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction,
                                  bool insertIdentity)
        {
            Hashtable insertedTableColl = new Hashtable();

            DataSet dsSchema = dbCommandBuilder.GetSchema();

            DataSetTableIterator iterator = new DataSetTableIterator(dsSchema, true);

            foreach (DataTable dataTable in iterator)
            {
                insertRecursive(ds, dataTable, dbCommandBuilder, dbTransaction, insertedTableColl, insertIdentity);
            }
        }

        private void insertRecursive(DataSet ds, DataTable dataTableSchema, IDbCommandBuilder dbCommandBuilder,
                                     IDbTransaction dbTransaction, Hashtable insertedTableColl, bool insertIdentity)
        {
            // Table has already been inserted into.
            if (insertedTableColl.ContainsKey(dataTableSchema.TableName))
            {
                return;
            }
            // [20060724 - sdh] Move here (from end of method) to avoid infinite-loop when package has relation to itself
            // Table was inserted into in the database.
            insertedTableColl[dataTableSchema.TableName] = null;

            ConstraintCollection constraints = dataTableSchema.Constraints;
            if (null != constraints)
            {
                foreach (Constraint constraint in constraints)
                {
                    // The table has a foreign key constraint.
                    if (constraint.GetType() == typeof(ForeignKeyConstraint))
                    {
                        ForeignKeyConstraint fkConstraint = (ForeignKeyConstraint)constraint;
                        // Must insert parent table first.
                        insertRecursive(ds, fkConstraint.RelatedTable, dbCommandBuilder, dbTransaction,
                                        insertedTableColl, insertIdentity);
                    }
                }
            }
            // process parent tables first!
            DataRelationCollection parentRelations = dataTableSchema.ParentRelations;
            if (null != parentRelations)
            {
                foreach (DataRelation parentRelation in parentRelations)
                {
                    // Must insert parent table first.
                    insertRecursive(ds, parentRelation.ParentTable, dbCommandBuilder, dbTransaction, insertedTableColl,
                                    insertIdentity);
                }
            }

            DataRow dataRowClone = null;
            DataTable dataTable = ds.Tables[dataTableSchema.TableName];
            DataTable dataTableClone = dataTableSchema.Clone();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                // Insert as a new row.
                dataRowClone = CloneDataRow(dataTableClone, dataRow);
                dataTableClone.Rows.Add(dataRowClone);
            }

            if (insertIdentity)
            {
                IDbCommand dbCommand = dbCommandBuilder.GetInsertIdentityCommand(dataTableSchema.TableName);
                OnInsertIdentity(dataTableClone, dbCommand, dbTransaction);
            }
            else
            {
                IDbCommand dbCommand = dbCommandBuilder.GetInsertCommand(dataTableSchema.TableName);
                OnInsert(dataTableClone, dbCommand, dbTransaction);
            }
        }

        protected virtual void OnDelete(DataTable dataTable, IDbCommand dbCommand, IDbTransaction dbTransaction)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            DbDataAdapter sqlDataAdapter = CreateDbDataAdapter();
            sqlDataAdapter.DeleteCommand = (DbCommand) dbCommand;
            sqlDataAdapter.DeleteCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.DeleteCommand.Transaction = sqlTransaction;

            sqlDataAdapter.Update(dataTable);
        }

        protected virtual void OnDeleteAll(IDbCommand dbCommand, IDbTransaction dbTransaction)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            DbCommand sqlCommand = (DbCommand) dbCommand;
            sqlCommand.Connection = sqlTransaction.Connection;
            sqlCommand.Transaction = sqlTransaction;

            sqlCommand.ExecuteNonQuery();
        }

        protected virtual void OnInsert(DataTable dataTable, IDbCommand dbCommand, IDbTransaction dbTransaction)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            DbDataAdapter sqlDataAdapter = CreateDbDataAdapter();
            sqlDataAdapter.InsertCommand = (DbCommand) dbCommand;
            sqlDataAdapter.InsertCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.InsertCommand.Transaction = sqlTransaction;

            sqlDataAdapter.Update(dataTable);
        }

        protected virtual void OnInsertIdentity(DataTable dataTable, IDbCommand dbCommand, IDbTransaction dbTransaction)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.AutoIncrement)
                {
                    // Set identity insert on.
                    DbCommand sqlCommand =
                        CreateDbCommand("SET IDENTITY_INSERT " +
                                        TableNameHelper.FormatTableName(dataTable.TableName, QuotePrefix, QuoteSuffix) +
                                        " ON");
                    sqlCommand.Connection = sqlTransaction.Connection;
                    sqlCommand.Transaction = sqlTransaction;
                    sqlCommand.ExecuteNonQuery();

                    break;
                }
            }

            try
            {
                DbDataAdapter sqlDataAdapter = CreateDbDataAdapter();
                sqlDataAdapter.InsertCommand = (DbCommand) dbCommand;
                sqlDataAdapter.InsertCommand.Connection = sqlTransaction.Connection;
                sqlDataAdapter.InsertCommand.Transaction = sqlTransaction;

                sqlDataAdapter.Update(dataTable);
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.AutoIncrement)
                    {
                        // Set identity insert off.
                        DbCommand sqlCommand =
                            CreateDbCommand("SET IDENTITY_INSERT " +
                                            TableNameHelper.FormatTableName(dataTable.TableName, QuotePrefix,
                                                                            QuoteSuffix) + " OFF");
                        sqlCommand.Connection = sqlTransaction.Connection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.ExecuteNonQuery();

                        break;
                    }
                }
            }
        }

        protected virtual void OnRefresh(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction,
                                         string tableName)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            DbDataAdapter sqlDataAdapter = CreateDbDataAdapter();
            sqlDataAdapter.SelectCommand = (DbCommand) dbCommandBuilder.GetSelectCommand(tableName);
            sqlDataAdapter.SelectCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.SelectCommand.Transaction = sqlTransaction;

            DataSet dsDb = new DataSet();
            // Query all records in the database table.
            sqlDataAdapter.Fill(dsDb, tableName);

            DataSet dsUpdate = dbCommandBuilder.GetSchema().Clone();

            DataTable dataTable = ds.Tables[tableName];
            DataTable dataTableDb = dsDb.Tables[tableName];
            // Iterate all rows in the table.
            foreach (DataRow dataRow in dataTable.Rows)
            {
                bool rowDoesNotExist = true;
                // Iterate all rows in the database table.
                foreach (DataRow dataRowDb in dataTableDb.Rows)
                {
                    // The row exists in the database.
                    if (IsPrimaryKeyValueEqual(dataRow, dataRowDb, dsUpdate.Tables[tableName].PrimaryKey))
                    {
                        rowDoesNotExist = false;
                        DataRow dataRowNew = CloneDataRow(dsUpdate.Tables[tableName], dataRow);
                        dsUpdate.Tables[tableName].Rows.Add(dataRowNew);
                        dataRowNew.AcceptChanges();
                        MarkRowAsModified(dataRowNew);
                        break;
                    }
                }

                // The row does not exist in the database.
                if (rowDoesNotExist)
                {
                    DataRow dataRowNew = CloneDataRow(dsUpdate.Tables[tableName], dataRow);
                    dsUpdate.Tables[tableName].Rows.Add(dataRowNew);
                }
            }

            // Does not insert identity.
            sqlDataAdapter.InsertCommand = (DbCommand) dbCommandBuilder.GetInsertCommand(tableName);
            sqlDataAdapter.InsertCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.InsertCommand.Transaction = sqlTransaction;

            sqlDataAdapter.UpdateCommand = (DbCommand) dbCommandBuilder.GetUpdateCommand(tableName);
            sqlDataAdapter.UpdateCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.UpdateCommand.Transaction = sqlTransaction;

            sqlDataAdapter.Update(dsUpdate, tableName);
        }

        private void MarkRowAsModified(DataRow dataRowNew)
        {
            dataRowNew.BeginEdit();
            dataRowNew.EndEdit();
        }

        protected virtual void OnUpdate(DataSet ds, IDbCommandBuilder dbCommandBuilder, IDbTransaction dbTransaction,
                                        string tableName)
        {
            DbTransaction sqlTransaction = (DbTransaction) dbTransaction;

            DbDataAdapter sqlDataAdapter = CreateDbDataAdapter();
            sqlDataAdapter.UpdateCommand = (DbCommand) dbCommandBuilder.GetUpdateCommand(tableName);
            sqlDataAdapter.UpdateCommand.Connection = sqlTransaction.Connection;
            sqlDataAdapter.UpdateCommand.Transaction = sqlTransaction;

            sqlDataAdapter.Update(ds, tableName);
        }
    }
}
