﻿using System;
using System.Data;
using MongoDB.Driver;
using System.Collections.Generic;

namespace NDbUnit.Core.MongoDB
{
    public class MongoDBConnection : IDbConnection
    {
        private MongoServer _mongoServer;
        private string _database;
        private readonly Dictionary<MongoServerState, ConnectionState> _mongoServerToConnectionXRef;


        public MongoDBConnection()
        {
            _mongoServerToConnectionXRef = new Dictionary<MongoServerState, ConnectionState>()
                                               {
                                                   {MongoServerState.Connected, ConnectionState.Open},
                                                   {MongoServerState.Connecting, ConnectionState.Connecting},
                                                   {MongoServerState.Disconnected, ConnectionState.Closed},
                                                   {MongoServerState.None, ConnectionState.Broken}
                                               };
        }

        public MongoDBConnection(string connectionString) : this()
        {
            ConnectionString = connectionString;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            _mongoServer.Disconnect();
        }

        public void ChangeDatabase(string databaseName)
        {
            _database = databaseName;
            _mongoServer.GetDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            _mongoServer = MongoServer.Create(ConnectionString);
            _mongoServer.Connect();
        }

        public string ConnectionString { get; set; }

        public int ConnectionTimeout
        {
            get { return 0; }
        }

        public string Database
        {
            get { return _database; }
        }

        public ConnectionState State
        {
            get
            {
                if (_mongoServer == null)
                    return ConnectionState.Closed;

                return _mongoServerToConnectionXRef[_mongoServer.State];
            }
        }
    }
}