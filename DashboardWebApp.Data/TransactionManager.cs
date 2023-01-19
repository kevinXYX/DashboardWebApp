
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Transactions;

namespace DashboardWebApp.Data
{
    internal class TransactionManager : IDisposable
    {
        internal static bool DalManagedTransactions
        {
            get
            {
                return TransactionManager.m_dalManagedTransactions;
            }
        }

        internal bool IsSimpleTransaction
        {
            get
            {
                return this.m_isSimpleTransaction;
            }
            set
            {
                this.m_isSimpleTransaction = value;
                if (!value && this.m_transactions.Peek().Value.State != ConnectionState.Closed)
                {
                    this.m_transactions.Peek().Value.Close();
                }
            }
        }

        static TransactionManager()
        {
            TransactionManager.m_dalManagedTransactions = true;
        }

        internal static TransactionManager GetCurrentTransactionManager()
        {
            TransactionManager result;
            if (TransactionManager.m_dalManagedTransactions)
            {
                result = TransactionManager.GetTransactionManagerFromCallContext();
            }
            else
            {
                result = new TransactionManager();
            }
            return result;
        }

        internal static void SetTransactionManager(TransactionManager transMan)
        {
            if (TransactionManager.m_dalManagedTransactions)
            {
                TransactionManager.SetTransactionManagerFromCallContext(transMan);
            }
        }

        private static TransactionManager GetTransactionManagerFromCallContext()
        {
            TransactionManager transactionManager = CallContext.GetData("dal_transaction_key") as TransactionManager;
            if (transactionManager != null && transactionManager.m_isDisposed)
            {
                transactionManager = null;
            }
            return transactionManager;
        }

        private static void SetTransactionManagerFromCallContext(TransactionManager transMan)
        {
            CallContext.SetData("dal_transaction_key", transMan);
        }

        internal TransactionManager()
        {
            this.m_transactions = new Stack<DBConnectionContainer>();
        }

        internal TransactionManager(Transaction newTrans)
        {
            this.m_transactions = new Stack<DBConnectionContainer>();
            DBConnectionContainer item = new DBConnectionContainer(newTrans.TransactionInformation.LocalIdentifier);
            this.m_transactions.Push(item);
            newTrans.TransactionCompleted += this.TransactionCompleted;
        }

        internal void AddTransaction(Transaction newTrans)
        {
            this.m_transactions.Push(new DBConnectionContainer(newTrans.TransactionInformation.LocalIdentifier));
            newTrans.TransactionCompleted += this.TransactionCompleted;
        }

        internal void AddConnection(Transaction transaction, string connectionString, IDbConnection connection)
        {
            DBConnectionContainer dbconnectionContainer = this.m_transactions.Peek();
            dbconnectionContainer.Key = connectionString;
            dbconnectionContainer.Value = connection;
        }

        internal string GetLastAddedTransactionLocalIdentifier()
        {
            return this.m_transactions.Peek().TransactionId;
        }

        internal IDbConnection GetConnection(string connString, string transactionKey)
        {
            IDbConnection result = null;
            DBConnectionContainer dbconnectionContainer = this.m_transactions.Peek();
            if (dbconnectionContainer.Key == connString)
            {
                result = dbconnectionContainer.Value;
            }
            return result;
        }

        private void TransactionCompleted(object sender, TransactionEventArgs e)
        {
            DBConnectionContainer dbconnectionContainer = this.m_transactions.Pop();
            if (dbconnectionContainer.Value != null && dbconnectionContainer.Value.State != ConnectionState.Closed)
            {
                dbconnectionContainer.Value.Close();
            }
            if (this.m_transactions.Count == 0)
            {
                this.Dispose();
                this.FreeNamedDataSlot();
            }
        }

        private void FreeNamedDataSlot()
        {
            if (TransactionManager.m_dalManagedTransactions)
            {
                CallContext.FreeNamedDataSlot("dal_transaction_key");
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.m_transactions.Count; i++)
            {
                DBConnectionContainer dbconnectionContainer = this.m_transactions.Pop();
                if (dbconnectionContainer.Value.State != ConnectionState.Closed)
                {
                    dbconnectionContainer.Value.Close();
                }
            }
            this.m_isDisposed = true;
        }

        internal const string CONTEXT_KEY = "dal_transaction_key";
        private const string DISABLE_TRANSACTION_KEY = "dalManagedTransactions";
        private static bool m_dalManagedTransactions;
        private bool m_isDisposed;
        private bool m_isSimpleTransaction = true;
        private Stack<DBConnectionContainer> m_transactions;
    }
}
