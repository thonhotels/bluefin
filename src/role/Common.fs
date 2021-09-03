namespace Bluefin.Role

module Common =
    let roleDefinition roleId =
        sprintf "/subscriptions/%s/providers/Microsoft.Authorization/roleDefinitions/%s" Bluefin.Core.subscriptionId
            roleId

    let Owner = roleDefinition Id.Owner
    let Contributor = roleDefinition Id.Contributor
    let Reader = roleDefinition Id.Reader
    let AcrDelete = roleDefinition Id.AcrDelete
    let AcrImageSigner = roleDefinition Id.AcrImageSigner
    let AcrPull = roleDefinition Id.AcrPull
    let AcrPush = roleDefinition Id.AcrPush
    let AcrQuarantineReader = roleDefinition Id.AcrQuarantineReader
    let AcrQuarantineWriter = roleDefinition Id.AcrQuarantineWriter
    let APIManagementServiceContributor = Id.APIManagementServiceContributor
    let APIManagementServiceOperator = Id.APIManagementServiceOperator
    let APIManagementServiceReader = Id.APIManagementServiceReader
    let AppConfigurationDataReader = roleDefinition Id.AppConfigurationDataReader
    let ApplicationInsightsComponentContributor = roleDefinition Id.ApplicationInsightsComponentContributor
    let ApplicationInsightsSnapshotDebugger = roleDefinition Id.ApplicationInsightsSnapshotDebugger
    let AzureKubernetesServiceClusterAdmin = roleDefinition Id.AzureKubernetesServiceClusterAdmin
    let AzureKubernetesServiceClusterUser = roleDefinition Id.AzureKubernetesServiceClusterUser
    let AzureServiceBusDataOwner = roleDefinition Id.AzureServiceBusDataOwner
    let AzureServiceBusDataReceiver = roleDefinition Id.AzureServiceBusDataReceiver
    let AzureServiceBusDataSender = roleDefinition Id.AzureServiceBusDataSender
    let CosmosDBAccountReader = roleDefinition Id.CosmosDBAccountReader
    let CosmosDBOperator = roleDefinition Id.CosmosDBOperator
    let CosmosBackupOperator = roleDefinition Id.CosmosBackupOperator
    let CosmosDBAccountContributor = roleDefinition Id.CosmosDBAccountContributor
    let KeyVaultContributor = roleDefinition Id.KeyVaultContributor
    let KeyVaultSecretsOfficer = Id.KeyVaultSecretsOfficer
    let KeyVaultSecretsUser = Id.KeyVaultSecretsUser
    let ManagedIdentityContributor = roleDefinition Id.ManagedIdentityContributor
    let ManagedIdentityOperator = roleDefinition Id.ManagedIdentityOperator
    let MonitoringContributor = roleDefinition Id.MonitoringContributor
    let MonitoringMetricsPublisher = roleDefinition Id.MonitoringMetricsPublisher
    let MonitoringReader = roleDefinition Id.MonitoringReader
    let NetworkContributor = roleDefinition Id.NetworkContributor
    let ReaderAndDataAccess = roleDefinition Id.ReaderAndDataAccess
    let RedisCacheContributor = roleDefinition Id.RedisCacheContributor
    let SqlDbContributor = roleDefinition Id.SqlDbContributor
    let SqlManagedInstanceContributor = roleDefinition Id.SqlManagedInstanceContributor
    let SqlSecurityManager = roleDefinition Id.SqlSecurityManager
    let SqlServerContributor = roleDefinition Id.SqlServerContributor
    let StorageAccountContributor = roleDefinition Id.StorageAccountContributor
    let StorageAccountKeyOperatorService = roleDefinition Id.StorageAccountKeyOperatorService
    let StorageBlobDataContributor = roleDefinition Id.StorageBlobDataContributor
    let StorageBlobDataOwner = roleDefinition Id.StorageBlobDataOwner
    let StorageBlobDataReader = roleDefinition Id.StorageBlobDataReader
    let StorageQueueDataContributor = roleDefinition Id.StorageQueueDataContributor
    let StorageQueueDataMessageProcessor = roleDefinition Id.StorageQueueDataMessageProcessor
    let StorageQueueDataMessageSender = roleDefinition Id.StorageQueueDataMessageSender
    let StorageQueueDataReader = roleDefinition Id.StorageQueueDataReader
    let VirtualMachineContributor = roleDefinition Id.VirtualMachineContributor
 