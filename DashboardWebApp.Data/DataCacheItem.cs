using System;
using System.Data;

namespace DashboardWebApp.Data
{
	public class DataCacheItem
	{
		public DataCacheItem()
		{
		}

		public DataCacheItem(int expirationTimeIntervalInMinutes)
		{
			this.m_expirationIntervalInMinutes = expirationTimeIntervalInMinutes;
		}

		public DataCacheItem(int expirationTimeIntervalInMinutes, DataSet cachedData)
		{
			this.m_expirationIntervalInMinutes = expirationTimeIntervalInMinutes;
			this.CachedData = cachedData;
		}

		public DataSet CachedData
		{
			get
			{
				this.m_lastAccessTime = DateTime.Now;
				return this.m_cachedData;
			}
			set
			{
				this.m_lastRefreshTime = DateTime.Now;
				this.m_cachedData = value;
			}
		}

		public DateTime LastRefreshTime
		{
			get
			{
				return this.m_lastRefreshTime;
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				return this.m_lastAccessTime;
			}
		}

		public int ExpirationIntervalInMinutes
		{
			get
			{
				return this.m_expirationIntervalInMinutes;
			}
			set
			{
				this.m_expirationIntervalInMinutes = value;
			}
		}

		public bool HasExpired
		{
			get
			{
				bool result = false;
				DateTime now = DateTime.Now;
				if (this.m_lastRefreshTime.AddMinutes((double)this.m_expirationIntervalInMinutes) <= now)
				{
					result = true;
				}
				return result;
			}
		}

		public static int DEFAULT_EXPIRATION_INTERVAL = 10;
		private DateTime m_lastRefreshTime;
		private DateTime m_lastAccessTime;
		private DataSet m_cachedData;
		private int m_expirationIntervalInMinutes = DataCacheItem.DEFAULT_EXPIRATION_INTERVAL;
	}
}
