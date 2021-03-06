﻿using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.EntityFrameworkCore.StreamNodes;
using System;
using System.Linq;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using System.Linq.Expressions;

namespace Paillave.Etl.EntityFrameworkCore.Extensions
{
    public static class EntityFrameworkCoreEx
    {
        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TIn, TResource, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, Func<TIn, TResource, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TResource : DbContext
        {
            return stream.CrossApply(name, resourceStream, (TIn inputValue, TResource valueToApply, Action<TOut> push) =>
            {
                foreach (var item in getQuery(inputValue, valueToApply).ToList())
                    push(item);
            }, noParallelisation);
        }
        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode,
                GetEntity = i => i,
                GetOutput = (i, j) => j,
            }).Output;
        }

        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource, TEntityKey>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Expression<Func<TIn, TEntityKey>> getBusinessKey, SaveByKeyMode bulkInsertMode = SaveByKeyMode.BulkUpsert, int chunkSize = 100)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TEntityKey, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TEntityKey, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey,
                BulkLoadMode = bulkInsertMode,
                GetEntity = i => i,
                GetOutput = (i, j) => i,
            }).Output;
        }

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Func<TIn, TInEf> getEntity, Func<TIn, TInEf, TOut> getResult, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TInEf : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TIn, TOut>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TIn, TOut>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode,
                GetEntity = getEntity,
                GetOutput = getResult,
            }).Output;
        }

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TEntityKey, TOut, TInEf>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Func<TIn, TInEf> getEntity, Expression<Func<TInEf, TEntityKey>> getBusinessKey, Func<TIn, TInEf, TOut> getResult, SaveByKeyMode bulkInsertMode = SaveByKeyMode.BulkUpsert, int chunkSize = 100)
            where TResource : DbContext
            where TInEf : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TEntityKey, TIn, TOut>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TEntityKey, TIn, TOut>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey,
                BulkLoadMode = bulkInsertMode,
                GetEntity = getEntity,
                GetOutput = getResult,
            }).Output;
        }
        public static IStream<TOut> Lookup<TIn, TEntity, TCtx, TOut>(this IStream<TIn> inputStream, string name, ISingleStream<TCtx> dbContextStream, Expression<Func<TIn, TEntity, bool>> match, Func<TIn, TEntity, TOut> resultSelector, int cacheSize = 1000)
            where TCtx : DbContext
            where TEntity : class
        {
            return new LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut>(name, new LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut>
            {
                CacheSize = cacheSize,
                DbContextStream = dbContextStream,
                InputStream = inputStream,
                Match = match,
                ResultSelector = resultSelector
            }).Output;
        }
    }
}
