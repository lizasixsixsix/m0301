// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using SampleSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        ///  %%     %% %%    %%     %%%%%%   %%%%%%%  %%%%%%%%  %%%%%%%%  %
        ///  %%%   %%%  %%  %%     %%    %% %%     %% %%     %% %%       %%%
        ///  %%%% %%%%   %%%%      %%       %%     %% %%     %% %%        %
        ///  %% %%% %%    %%       %%       %%     %% %%     %% %%%%%%
        ///  %%     %%    %%       %%       %%     %% %%     %% %%        %
        ///  %%     %%    %%       %%    %% %%     %% %%     %% %%       %%%
        ///  %%     %%    %%        %%%%%%   %%%%%%%  %%%%%%%%  %%%%%%%%  %

        [Category("My Tasks")]
        [Title("Task 01")]
        [Description("Reusable Query")]
        public void Linq01()
        {
            var query = new Func<DataSource, decimal, IDictionary<string, decimal>>(
                (source, x) =>
                    source.Customers.Select(
                        c => new
                        {
                            cust = c.CompanyName,
                            sum = c.Orders.Select(o => o.Total).Sum()
                        })
                        .Where(c => c.sum > x)
                        .ToDictionary(c => c.cust, c => c.sum));

            var customers = dataSource.Execute(
                source => query(source, 500));

            foreach (var c in customers)
            {
                ObjectDumper.Write($"{c.Key,-40} : {c.Value}");
            }
        }

        [Category("My Tasks")]
        [Title("Task 02.1")]
        [Description("With Grouping")]
        public void Linq021()
        {
            var groupedSuppliers = dataSource.Suppliers.GroupBy(
                s => new
                {
                    country = s.Country,
                    city = s.City
                },
                s => s.SupplierName);

            var customersSuppliers = dataSource.Customers.Select(
                c => new
                {
                    cust = c.CompanyName,
                    supps = groupedSuppliers.Where(
                        g => g.Key.country == c.Country
                             && g.Key.city == c.City
                        )
                })
                .Where(c => c.supps.Any());

            foreach (var c in customersSuppliers)
            {
                ObjectDumper.Write($"{c.cust} :");

                foreach (var s in c.supps)
                {
                    foreach (var ss in s)
                    {
                        ObjectDumper.Write($"    * {ss}");
                    }
                }
            }
        }

        [Category("My Tasks")]
        [Title("Task 02.2")]
        [Description("With Join")]
        public void Linq022()
        {
            var customersSuppliers = dataSource.Customers.GroupJoin(
                dataSource.Suppliers,
                c => new
                {
                    country = c.Country,
                    city = c.City
                },
                s => new
                {
                    country = s.Country,
                    city = s.City
                },
                (c, s) => new
                {
                    cust = c.CompanyName,
                    supps = s.Select(ss => ss.SupplierName)
                }
            )
            .Where(cc => cc.supps.Any());

            foreach (var c in customersSuppliers)
            {
                ObjectDumper.Write($"{c.cust} :");

                foreach (var s in c.supps)
                {
                    ObjectDumper.Write($"    * {s}");
                }
            }
        }

        [Category("My Tasks")]
        [Title("Task 03")]
        [Description("Any Greater than X")]
        public void Linq03()
        {
            var query = new Func<DataSource, decimal, IEnumerable<Customer>>(
                (source, x) =>
                    source.Customers.Where(
                        c => c.Orders.Any(o => o.Total > x)));

            var customers = dataSource.Execute(
                source => query(source, 500));

            foreach (var c in customers)
            {
                ObjectDumper.Write(c.CompanyName);
            }
        }

        [Category("My Tasks")]
        [Title("Task 04")]
        [Description("Earliest Order")]
        public void Linq04()
        {
            var customers = dataSource.Customers.Select(
                c => c.CompanyName
                )
                .Zip(
                    dataSource.Customers.Where(c => c.Orders.Any())
                        .Select(
                        d => d.Orders.Min(o => o.OrderDate)
                        ),
                    (c, d) => new
                    {
                        cust = c,
                        date = d
                    }
                );

            foreach (var c in customers)
            {
                ObjectDumper.Write($"{c.cust,-40} : {c.date:MM/dd/yyyy HH:mm:ss tt}");
            }
        }

        [Category("My Tasks")]
        [Title("Task 05")]
        [Description("Multiple Order By")]
        public void Linq05()
        {
            var customers = dataSource.Customers
                .Zip(
                    dataSource.Customers.Where(c => c.Orders.Any())
                        .Select(
                        d => d.Orders.Min(o => o.OrderDate)
                        ),
                    (c, d) => new
                    {
                        cust = c.CompanyName,
                        sum = c.Orders.Sum(o => o.Total),
                        date = d
                    }
                )
                .OrderBy(c => c.date.Year)
                .ThenBy(c => c.date.Month)
                .ThenByDescending(c => c.sum)
                .ThenBy(c => c.cust);

            foreach (var c in customers)
            {
                ObjectDumper.Write($"{c.cust,-40} : " +
                                   $"{c.date:yyyy}, {c.date:MM}, : " +
                                   $"{c.sum,10}");
            }
        }

        [Category("My Tasks")]
        [Title("Task 06")]
        [Description("Wrong Address Fields")]
        public void Linq06()
        {
            var customers = dataSource.Customers
                .Where(c => !String.IsNullOrEmpty(c.PostalCode) && !Regex.IsMatch(c.PostalCode, @"^[\d-]+$")
                            || String.IsNullOrEmpty(c.Region)
                            || !String.IsNullOrEmpty(c.Phone) && !Regex.IsMatch(c.Phone, @"^\(\d+\)"));

            foreach (var c in customers)
            {
                ObjectDumper.Write($"{c.CompanyName,-36} : " +
                                   $"{c.PostalCode,10} : " +
                                   $"{c.Region,13} : " +
                                   $"{c.Phone,10}");
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
}
