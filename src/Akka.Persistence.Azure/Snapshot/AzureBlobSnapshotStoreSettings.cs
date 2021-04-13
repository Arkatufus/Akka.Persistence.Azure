// -----------------------------------------------------------------------
// <copyright file="AzureBlobSnapshotStoreSettings.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Akka.Configuration;
using Akka.Persistence.Azure.Util;
using Azure.Storage.Blobs.Models;

namespace Akka.Persistence.Azure.Snapshot
{
    /// <summary>
    ///     Configuration settings for the <see cref="AzureBlobSnapshotStore" />.
    ///     Loads settings from the `akka.persistence.snapshot-store.azure-blob-store` HOCON section.
    /// </summary>
    public sealed class AzureBlobSnapshotStoreSettings
    {
        [Obsolete]
        public AzureBlobSnapshotStoreSettings(
            string connectionString, 
            string containerName,
            TimeSpan connectTimeout, 
            TimeSpan requestTimeout, 
            bool verboseLogging, 
            bool development, 
            bool autoInitialize):
            this(connectionString, containerName, connectTimeout, requestTimeout, verboseLogging, development, autoInitialize, PublicAccessType.BlobContainer)
        {
        }

        public AzureBlobSnapshotStoreSettings(
            string connectionString, 
            string containerName,
            TimeSpan connectTimeout, 
            TimeSpan requestTimeout, 
            bool verboseLogging, 
            bool development, 
            bool autoInitialize, 
            PublicAccessType publicAccessType)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ConfigurationException("[AzureBlobSnapshotStore] Container name is null or empty.");

            NameValidator.ValidateContainerName(containerName);
            ConnectionString = connectionString;
            ContainerName = containerName;
            RequestTimeout = requestTimeout;
            ConnectTimeout = connectTimeout;
            VerboseLogging = verboseLogging;
            Development = development;
            AutoInitialize = autoInitialize;
            ContainerPublicAccessType = publicAccessType;
        }

        /// <summary>
        ///     The connection string for connecting to Windows Azure blob storage account.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        ///     The table of the container we'll be using to serialize these blobs.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        /// The public access type of the newly auto-initialized snapshot container.
        /// </summary>
        public PublicAccessType ContainerPublicAccessType { get; }

        /// <summary>
        ///     Initial timeout to use when connecting to Azure Container Storage for the first time.
        /// </summary>
        public TimeSpan ConnectTimeout { get; }

        /// <summary>
        ///     Timeouts for individual read, write, and delete requests to Azure Container Storage.
        /// </summary>
        public TimeSpan RequestTimeout { get; }

        /// <summary>
        ///     For debugging purposes only. Logs every individual operation to Azure table storage.
        /// </summary>
        public bool VerboseLogging { get; }

        /// <summary>
        ///     For development purposes only. Setting this to true will override the connection string with
        ///     "UseDevelopmentStorage=true" to connect to Azure Storage Emulator.
        /// </summary>
        public bool Development { get; }

        /// <summary>
        ///     When set to true, the snapshot store will attempt to automatically create the snapshot container
        ///     resource when none exists in the storage account.
        /// </summary>
        public bool AutoInitialize { get; }

        /// <summary>
        ///     Creates an <see cref="AzureBlobSnapshotStoreSettings" /> instance using the
        ///     `akka.persistence.snapshot-store.azure-blob-store` HOCON configuration section.
        /// </summary>
        /// <param name="config">The `akka.persistence.snapshot-store.azure-blob-store` HOCON section.</param>
        /// <returns>A new settings instance.</returns>
        public static AzureBlobSnapshotStoreSettings Create(Config config)
        {
            var connectionString = config.GetString("connection-string");
            var containerName = config.GetString("container-name", "akka-persistence-default-container");
            var connectTimeout = config.GetTimeSpan("connect-timeout", TimeSpan.FromSeconds(3));
            var requestTimeout = config.GetTimeSpan("request-timeout", TimeSpan.FromSeconds(3));
            var verbose = config.GetBoolean("verbose-logging", false);
            var development = config.GetBoolean("development", false);
            var autoInitialize = config.GetBoolean("auto-initialize", true);

            if (string.IsNullOrWhiteSpace(connectionString) && !development)
                throw new ConfigurationException(
                    "Invalid [connection-string]. Connection string must not be null or empty when development mode is turned off.");

            if(!Enum.TryParse<PublicAccessType>(
                value: config.GetString("container-public-access-type", "BlobContainer"),
                ignoreCase: true, 
                out var publicAccessType))
                throw new ConfigurationException("Invalid [container-public-access-type] value. Valid values are 'None', 'Blob', or 'BlobContainer'");

            return new AzureBlobSnapshotStoreSettings(
                connectionString, 
                containerName, 
                connectTimeout, 
                requestTimeout,
                verbose,
                development,
                autoInitialize, 
                publicAccessType);
        }
    }
}