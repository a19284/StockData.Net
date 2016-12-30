using System.Collections.Specialized;
using System.Configuration;

namespace StockDataQuartz
{
	/// <summary>
	/// Configuration for the Quartz server.
	/// </summary>
	public class Configuration
	{
        private const string PrefixServerConfiguration = "StockDataQuartz";
		private const string KeyServiceName = PrefixServerConfiguration + ".ServiceName";
		private const string KeyServiceDisplayName = PrefixServerConfiguration + ".ServiceDisplayName";
		private const string KeyServiceDescription = PrefixServerConfiguration + ".ServiceDescription";
        private const string KeyServerImplementationType = PrefixServerConfiguration + ".type";

        private const string DefaultServiceName = "StockDataQuartz.ScheduleService";
		private const string DefaultServiceDisplayName = "StockDataQuartz";
        private const string DefaultServiceDescription = "StockDataQuartz ScheduleService";
	    private static readonly string DefaultServerImplementationType = typeof(QuartzServer).AssemblyQualifiedName;

	    private static readonly NameValueCollection configuration;

        /// <summary>
        /// Initializes the <see cref="Configuration"/> class.
        /// </summary>
		static Configuration()
		{
            //不能采用下面的语句，除非把Quartz.config的所有信息都配置到quartz的节点下
			//configuration = (NameValueCollection) ConfigurationManager.GetSection("quartz");
            
            configuration = (NameValueCollection)ConfigurationManager.AppSettings;
		}

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
		public static string ServiceName
		{
			get { return GetConfigurationOrDefault(KeyServiceName, DefaultServiceName); }
		}

        /// <summary>
        /// Gets the display name of the service.
        /// </summary>
        /// <value>The display name of the service.</value>
		public static string ServiceDisplayName
		{
			get { return GetConfigurationOrDefault(KeyServiceDisplayName, DefaultServiceDisplayName); }
		}

        /// <summary>
        /// Gets the service description.
        /// </summary>
        /// <value>The service description.</value>
		public static string ServiceDescription
		{
			get { return GetConfigurationOrDefault(KeyServiceDescription, DefaultServiceDescription); }
		}

        /// <summary>
        /// Gets the type name of the server implementation.
        /// </summary>
        /// <value>The type of the server implementation.</value>
	    public static string ServerImplementationType
	    {
            get { return GetConfigurationOrDefault(KeyServerImplementationType, DefaultServerImplementationType); }
	    }

		/// <summary>
		/// Returns configuration value with given key. If configuration
		/// for the does not exists, return the default value.
		/// </summary>
		/// <param name="configurationKey">Key to read configuration with.</param>
		/// <param name="defaultValue">Default value to return if configuration is not found</param>
		/// <returns>The configuration value.</returns>
		private static string GetConfigurationOrDefault(string configurationKey, string defaultValue)
		{
			string retValue = null;
            if (configuration != null)
            {
                retValue = configuration[configurationKey];
            }

			if (retValue == null || retValue.Trim().Length == 0)
			{
				retValue = defaultValue;
			}
			return retValue;
		}
	}
}
