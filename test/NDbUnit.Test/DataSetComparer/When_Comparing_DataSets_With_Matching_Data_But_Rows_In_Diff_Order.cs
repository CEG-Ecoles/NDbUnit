﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Gallio.Runtime.UtilityCommands;
using KellermanSoftware.CompareNetObjects;
using NDbUnit.Core;
using NUnit.Framework;

namespace NDbUnit.Test.DataSetComparer
{
    public class When_Comparing_DataSets_With_Matching_Data_But_Rows_In_Diff_Order : DataSetComparerTestBase
    {
        [Test]
        public void CanReportNoMatch()
        {
            var firstDataSet = BuildDataSet(@"Xml\DataSetComparer\FirstDataSetToCompare.xsd", @"Xml\DataSetComparer\FirstDataToCompare.xml");
            var secondDataSet = BuildDataSet(@"Xml\DataSetComparer\FirstDataSetToCompare.xsd", @"Xml\DataSetComparer\DifferingDataWithRowsInDiffOrderToCompare.xml");

            Assert.That(firstDataSet.HasTheSameDataAs(secondDataSet));
        }
    }
}