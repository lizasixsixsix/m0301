// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using SampleSupport;
using System;
using System.Collections.Generic;
using System.Linq;

using Task.Data;

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {
        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Where - Task 1")]
        [Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
        public void Linq1()
        {
            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

            var lowNums =
                from num in numbers
                where num < 5
                select num;

            Console.WriteLine("Numbers < 5:");

            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 2")]
        [Description("This sample return return all presented in market products")]
        public void Linq2()
        {
            var products =
                from p in dataSource.Products
                where p.UnitsInStock > 0
                select p;

            foreach (var p in products)
            {
                ObjectDumper.Write(p);
            }
        }

        ///                         .            .          .
        ///  ,-,-. . .    ,-. ,-. ,-| ,-.    ,-. |- ,-. ,-. |- ,-.
        ///  | | | | |    |   | | | | |-'    `-. |  ,-| |   |  `-.
        ///  ' ' ' `-|    `-' `-' `-^ `-'    `-' `' `-^ '   `' `-'
        ///         /|
        ///        `-'

        [Category("My Tasks")]
        [Title("Task 01")]
        [Description("Reusable Query")]
        public void Linq01()
        {
            var query = new Func<DataSource, int, IEnumerable<Customer>>(
                (source, x) =>
                    source.Customers.Where(
                        c => c.Orders.Select(o => o.Total).Sum() > x));

            var customers = dataSource.Execute(
                source => query(source, 500));

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }
    }

    public static class SourceExtensions
    {
        public static TResult Execute<TSource, TResult>(this TSource source,
                                                        Func<TSource, TResult> stuff)
        {
            return stuff(source);
        }
    }

    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source,
                                      Action<T> stuff)
        {
            foreach (var element in source)
            {
                stuff(element);
            }
        }
    }

    ///  ,-,-. . .    ,-. ,-. ,-| ,-.    ,-. ,-. ,-| ,-.
    ///  | | | | |    |   | | | | |-'    |-' | | | | `-.
    ///  ' ' ' `-|    `-' `-' `-^ `-'    `-' ' ' `-^ `-'
    ///         /|
    ///        `-'
}
