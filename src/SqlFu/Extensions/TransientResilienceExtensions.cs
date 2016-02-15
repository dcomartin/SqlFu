﻿using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Model.Persistence;

namespace SqlFu
{
    public static class TransientResilienceExtensions
    {
        /// <summary>
        /// Wraps db access action and retries it for 10 times with a slight delay between retries
        /// if the db is temporary unavailable (timeout or connection limit reached)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbFactory"></param>
        /// <param name="action"></param>
        /// <param name="tryCount"></param>
        /// <param name="wait">How much to wait in miliseconds before retrying</param>
        /// <exception cref="PersistenceException"></exception>
        /// <returns></returns>
        public static T HandleTransientErrors<T>(this IDbFactory dbFactory,Func<DbConnection,T> action,int tryCount=10,int wait=100)
        {
            T result = default(T);
            ModelTools.RetryOnException<DbException>(() =>
            {
                using (var db = dbFactory.Create())
                {
                    result=action(db);
                }
            }, x =>
            {
                if (dbFactory.Provider.IsDbBusy(x)) return true;
                throw new PersistenceException(x);

            },tryCount,wait);
            return result;
        }

        /// <summary>
        /// Wraps db access action and retries it for 10 times with a slight delay between retries
        /// if the db is temporary unavailable (timeout or connection limit reached)
        /// </summary>
        /// <param name="dbFactory"></param>
        /// <param name="tryCount"></param>
        /// <param name="wait">How much to wait in miliseconds before retrying</param>
        /// <exception cref="PersistenceException"></exception>
        /// <param name="action"></param>
        public static void HandleTransientErrors(this IDbFactory dbFactory, Action<DbConnection> action, int tryCount = 10, int wait = 100)
        {
            ModelTools.RetryOnException<DbException>(() =>
            {
                using (var db = dbFactory.Create())
                {
                    action(db);
                }
            }, x =>
            {
                if (dbFactory.Provider.IsDbBusy(x)) return true;
                         
                throw new PersistenceException(x);

            });
        }

        /// <summary>
        /// Wraps db access action and retries it for 10 times with a slight delay between retries
        /// if the db is temporary unavailable (timeout or connection limit reached)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbFactory"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <param name="tryCount"></param>
        /// <param name="wait">How much to wait in miliseconds before retrying</param>
        /// <exception cref="PersistenceException"></exception>
        /// <returns></returns>
        public static async Task<T> HandleTransientErrorsAsync<T>(this IDbFactory dbFactory, CancellationToken token,Func<DbConnection,CancellationToken,Task<T>> action, int tryCount = 10, int wait = 100)
        {
            T result = default(T);
       
            await ModelTools.RetryOnException<DbException>(async () =>
            {
                using (var db = await dbFactory.CreateAsync(token).ConfigureAwait(false))
                {
                  result = await action(db,token).ConfigureAwait(false);
                }
            }, x =>
            {
               if (dbFactory.Provider.IsDbBusy(x)) return true;
               throw new PersistenceException(x);

            }).ConfigureAwait(false);
            return result;
        }


        /// <summary>
        /// Wraps db access action and retries it for 10 times with a slight delay between retries
        /// if the db is temporary unavailable (timeout or connection limit reached)
        /// </summary>
        /// <param name="dbFactory"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <param name="tryCount"></param>
        /// <param name="wait">How much to wait in miliseconds before retrying</param>
        /// <exception cref="PersistenceException"></exception>
        /// <returns></returns>
        public static async Task HandleTransientErrorsAsync(this IDbFactory dbFactory, CancellationToken token,Func<DbConnection,CancellationToken,Task> action, int tryCount = 10, int wait = 100)
        {
            await ModelTools.RetryOnException<DbException>(async () =>
            {
                using (var db = await dbFactory.CreateAsync(token).ConfigureAwait(false))
                {
                   await action(db,token).ConfigureAwait(false);
                }
            }, x =>
            {
               if (dbFactory.Provider.IsDbBusy(x)) return true;
               throw new PersistenceException(x);

            }).ConfigureAwait(false);
        
        }

       


    }
}