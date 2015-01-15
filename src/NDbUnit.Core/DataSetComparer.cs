﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace NDbUnit.Core
{
    public static class DataSetComparer
    {
        //useful to increase this during debug/testing, default to 1 so that the first non-matching value will abort comparison and return a FALSE
        public static int MAX_COMPARE_ERRORS = 1;

        public static bool HasTheSameDataAs(this DataSet left, DataSet right)
        {
            //if schemas don't match, no point in proceeding to test any data/content so just bail out early...
            if (!left.HasTheSameSchemaAs(right))
                return false;

            foreach (var table in left.Tables.Cast<DataTable>())
            {
                if (!right.Tables.Contains(table.TableName))
                    return false;

                if (!HaveTheSameData(table, right.Tables[table.TableName]))
                    return false;
            }

            return true;
        }

        public static bool HasTheSameSchemaAs(this DataSet left, DataSet right)
        {
            //if the count of tables fails to match, no point in proceeding
            if (left.Tables.Count != right.Tables.Count)
                return false;

            //consider tables
            foreach (var table in left.Tables.Cast<DataTable>())
            {
                if (!right.Tables.Contains(table.TableName))
                    return false;

                if (!HaveTheSameSchema(table, right.Tables[table.TableName]))
                    return false;
            }

            //consider relatioships
            foreach (var relationship in left.Relations.Cast<DataRelation>())
            {
                if (!HaveTheSameSchema(relationship, right.Relations[relationship.RelationName]))
                    return false;
            }

            return true;
        }

        private static bool HaveTheSameSchema(DataTable left, DataTable right)
        {
            //for some reason the CompareNETObjects comparer refuses to respect the config directive to ignore the Rows property
            // so we have to clone the DataTable(s) and then clear the .Rows collection on both before comparing them
            var leftClone = left.Clone();
            var rightClone = right.Clone();

            leftClone.Rows.Clear();
            rightClone.Rows.Clear();

            var config = new ComparisonConfig { IgnoreCollectionOrder = true, CompareChildren = false, MaxDifferences = MAX_COMPARE_ERRORS };

            //this line *should* make the comparer ignore the Rows collection, but it doesn't appear to work
            // so we'll leave it in here just in case this functionality should be resolved at some future point
            config.MembersToIgnore.Add("Rows");

            var comparer = new CompareLogic(config);

            var result = comparer.Compare(leftClone, rightClone);

            if (!result.AreEqual)
            {
                Log(result.DifferencesString);
                return false;
            }

            //if the count of columns fails to match, no point in proceeding
            if (left.Columns.Count != right.Columns.Count)
                return false;

            foreach (var column in left.Columns.Cast<DataColumn>())
            {
                if (!right.Columns.Contains(column.ColumnName))
                    return false;

                if (!HaveTheSameSchema(column, right.Columns[column.ColumnName]))
                    return false;
            }

            return true;
        }

        private static bool HaveTheSameData(DataTable left, DataTable right)
        {
            //clone the tables so we don't inadvertently modify the ACTUAL datatables as part of the compare process...
            var leftTable = left.Clone();
            var rightTable = right.Clone();

            //if the count of rows fails to match, no point in proceeding
            if (leftTable.Rows.Count != rightTable.Rows.Count)
                return false;

            //get the rows
            var leftRows = leftTable.Rows.Cast<DataRow>();
            var rightRows = rightTable.Rows.Cast<DataRow>();

            //now clear the rows since CompareNETObjects cannot ignore Rows property...
            leftTable.Rows.Clear();
            rightTable.Rows.Clear();

            var config = new ComparisonConfig { IgnoreCollectionOrder = true, MaxDifferences = MAX_COMPARE_ERRORS };

            //this doens't actually work, but leaving it here for now in case this issue is resolved in CompareNETObjects later...
            config.MembersToIgnore.Add("Rows");

            var comparer = new CompareLogic(config);

            var result = comparer.Compare(leftTable, rightTable);

            if (!result.AreEqual)
            {
                Log(string.Format("Expected DataTable: {0}, Actual DataTable: {1}\n{2}", leftTable.TableName,
                    rightTable.TableName, result.DifferencesString));

                return false;
            }

            //if the tables match, proceed to compare the rows...
            return HaveTheSameData(leftRows, rightRows);

        }

        private static bool HaveTheSameData(IEnumerable<DataRow> leftRows, IEnumerable<DataRow> rightRows)
        {
            //protect against multiple iterations of IEnumberables...
            leftRows = leftRows.ToList();
            rightRows = rightRows.ToList();

            //this test was already performed at the table level, but let's do it again just in case this method gets called from elsewhere at some pt
            if (leftRows.Count() != rightRows.Count())
                return false;

            foreach (var leftRow in leftRows)
            {
                var matchingRowFound = rightRows.Any(rightRow => HaveTheSameData(leftRow, rightRow));

                if (!matchingRowFound)
                    return false;
            }

            return true;
        }

        public static bool HaveTheSameData(DataRow left, DataRow right)
        {
            var config = new ComparisonConfig { IgnoreCollectionOrder = true, CompareChildren = false, MaxDifferences = MAX_COMPARE_ERRORS };
            var comparer = new CompareLogic(config);

            var result = comparer.Compare(left, right);

            if (!result.AreEqual)
                Log(string.Format("Expected DataRow: {0}, Actual DataRow: {1}\n{2}", left,
                    right, result.DifferencesString));

            return result.AreEqual;
        }

        private static bool HaveTheSameSchema(DataColumn left, DataColumn right)
        {
            var config = new ComparisonConfig { IgnoreCollectionOrder = true, CompareChildren = false, MaxDifferences = MAX_COMPARE_ERRORS };
            var comparer = new CompareLogic(config);

            var result = comparer.Compare(left, right);

            if (!result.AreEqual)
                Log(string.Format("Expected DataColumn: {0}, Actual DataColumn: {1}\n{2}", left.ColumnName,
                    right.ColumnName, result.DifferencesString));

            return result.AreEqual;
        }

        private static bool HaveTheSameSchema(DataRelation left, DataRelation right)
        {
            var config = new ComparisonConfig { IgnoreCollectionOrder = true, CompareChildren = false, MaxDifferences = MAX_COMPARE_ERRORS };
            var comparer = new CompareLogic(config);

            var result = comparer.Compare(left, right);

            if (!result.AreEqual)
                Log(string.Format("Expected DataRelation: {0}, Actual DataRelation: {1}\n{2}", left.RelationName,
                    right.RelationName, result.DifferencesString));

            return result.AreEqual;
        }

        //TODO: wire up Common.Logging here...
        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// Compare all tables and all rows in all tables
    /// </summary>
    internal class NDbUnitDatasetComparer : BaseTypeComparer
    {
        private readonly DataTableComparer _compareDataTable;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public NDbUnitDatasetComparer(RootComparer rootComparer)
            : base(rootComparer)
        {
            _compareDataTable = new DataTableComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if both objects are data sets
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsDataset(type1) && TypeHelper.IsDataset(type2);
        }

        /// <summary>
        /// Compare two data sets
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            DataSet dataSet1 = parms.Object1 as DataSet;
            DataSet dataSet2 = parms.Object2 as DataSet;

            //This should never happen, null check happens one level up
            if (dataSet1 == null || dataSet2 == null)
                return;

            if (TableCountsDifferent(parms, dataSet2, dataSet1)) return;

            CompareEachTable(parms, dataSet1, dataSet2);
        }

        private bool TableCountsDifferent(CompareParms parms, DataSet dataSet2, DataSet dataSet1)
        {
            if (dataSet1.Tables.Count != dataSet2.Tables.Count)
            {
                Difference difference = new Difference
                                            {
                                                ParentObject1 = new WeakReference(parms.ParentObject1),
                                                ParentObject2 = new WeakReference(parms.ParentObject2),
                                                PropertyName = parms.BreadCrumb,
                                                Object1Value = dataSet1.Tables.Count.ToString(CultureInfo.InvariantCulture),
                                                Object2Value = dataSet2.Tables.Count.ToString(CultureInfo.InvariantCulture),
                                                ChildPropertyName = "Tables.Count",
                                                Object1 = new WeakReference(parms.Object1),
                                                Object2 = new WeakReference(parms.Object2)
                                            };

                AddDifference(parms.Result, difference);

                if (parms.Result.ExceededDifferences)
                    return true;
            }
            return false;
        }

        private void CompareEachTable(CompareParms parms, DataSet dataSet1, DataSet dataSet2)
        {
            for (int i = 0; i < Math.Min(dataSet1.Tables.Count, dataSet2.Tables.Count); i++)
            {
                string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Tables", string.Empty,
                                                         dataSet1.Tables[i].TableName);

                CompareParms childParms = new CompareParms();
                childParms.Result = parms.Result;
                childParms.Config = parms.Config;
                childParms.BreadCrumb = currentBreadCrumb;
                childParms.ParentObject1 = dataSet1;
                childParms.ParentObject2 = dataSet2;
                childParms.Object1 = dataSet1.Tables[i];
                childParms.Object2 = dataSet2.Tables[i];

                _compareDataTable.CompareType(childParms);

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }
    }
/// <summary>
    /// Compare all rows in a data table
    /// </summary>
    public class NDbUnitDataTableComparer : BaseTypeComparer
    {
        private readonly DataRowComparer _compareDataRow;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public NDbUnitDataTableComparer(RootComparer rootComparer)
            : base(rootComparer)
        {
            _compareDataRow = new DataRowComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if both objects are of type DataTable
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsDataTable(type1) && TypeHelper.IsDataTable(type2);
        }

        /// <summary>
        /// Compare two datatables
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            DataTable dataTable1 = parms.Object1 as DataTable;
            DataTable dataTable2 = parms.Object2 as DataTable;

            //This should never happen, null check happens one level up
            if (dataTable1 == null || dataTable2 == null)
                return;

            //Only compare specific table names
            if (parms.Config.MembersToInclude.Count > 0 && !parms.Config.MembersToInclude.Contains(dataTable1.TableName))
                return;

            //If we should ignore it, skip it
            if (parms.Config.MembersToInclude.Count == 0 && parms.Config.MembersToIgnore.Contains(dataTable1.TableName))
                return;

            //There must be the same amount of rows in the datatable
            if (dataTable1.Rows.Count != dataTable2.Rows.Count)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = new WeakReference(parms.ParentObject1),
                    ParentObject2 = new WeakReference(parms.ParentObject2),
                    PropertyName = parms.BreadCrumb,
                    Object1Value = dataTable1.Rows.Count.ToString(CultureInfo.InvariantCulture),
                    Object2Value = dataTable2.Rows.Count.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Rows.Count",
                    Object1 = new WeakReference(parms.Object1),
                    Object2 = new WeakReference(parms.Object2)
                };

                AddDifference(parms.Result, difference);

                if (parms.Result.ExceededDifferences)
                    return;
            }

            if (ColumnCountsDifferent(parms)) return;

            CompareEachRow(parms);
        }

        private bool ColumnCountsDifferent(CompareParms parms)
        {
            DataTable dataTable1 = parms.Object1 as DataTable;
            DataTable dataTable2 = parms.Object2 as DataTable;

            if (dataTable1.Columns.Count != dataTable2.Columns.Count)
            {
                Difference difference = new Difference
                {
                    ParentObject1 = new WeakReference(parms.ParentObject1),
                    ParentObject2 = new WeakReference(parms.ParentObject2),
                    PropertyName = parms.BreadCrumb,
                    Object1Value = dataTable1.Columns.Count.ToString(CultureInfo.InvariantCulture),
                    Object2Value = dataTable2.Columns.Count.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Columns.Count",
                    Object1 = new WeakReference(parms.Object1),
                    Object2 = new WeakReference(parms.Object2)
                };

                AddDifference(parms.Result, difference);

                if (parms.Result.ExceededDifferences)
                    return true;
            }
            return false;
        }

        private void CompareEachRow(CompareParms parms)
        {
            DataTable dataTable1 = parms.Object1 as DataTable;
            DataTable dataTable2 = parms.Object2 as DataTable;

            for (int i = 0; i < Math.Min(dataTable1.Rows.Count, dataTable2.Rows.Count); i++)
            {
                string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Rows", string.Empty, i);

                CompareParms childParms = new CompareParms
                {
                    Result = parms.Result,
                    Config = parms.Config,
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    Object1 = dataTable1.Rows[i],
                    Object2 = dataTable2.Rows[i],
                    BreadCrumb = currentBreadCrumb
                };

                _compareDataRow.CompareType(childParms);

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }


    }
}