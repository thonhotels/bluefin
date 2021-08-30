namespace Bluefin.Role

module Common =
    let roleDefinition roleId =
        sprintf "/subscriptions/%s/providers/Microsoft.Authorization/roleDefinitions/%s" Bluefin.Core.subscriptionId
            roleId

    let Owner = roleDefinition "8e3af657-a8ff-443c-a75c-2fe8c4bcb635"
    let Contributor = roleDefinition "b24988ac-6180-42a0-ab88-20f7382dd24c"
    let Reader = roleDefinition "acdd72a7-3385-48ef-bd42-f606fba81ae7"
    let AcrDelete = roleDefinition "c2f4ef07-c644-48eb-af81-4b1b4947fb11"
    let AcrImageSigner = roleDefinition "6cef56e8-d556-48e5-a04f-b8e64114680f"
    let AcrPull = roleDefinition "7f951dda-4ed3-4680-a7ca-43fe172d538d"
    let AcrPush = roleDefinition "8311e382-0749-4cb8-b61a-304f252e45ec"
    let AcrQuarantineReader = roleDefinition "cdda3590-29a3-44f6-95f2-9f980659eb04"
    let AcrQuarantineWriter = roleDefinition "c8d4ff99-41c3-41a8-9f60-21dfdad59608"
    let APIManagementServiceContributor = "312a565d-c81f-4fd8-895a-4e21e48d571c"
    let APIManagementServiceOperator = "e022efe7-f5ba-4159-bbe4-b44f577e9b61"
    let APIManagementServiceReader = "71522526-b88f-4d52-b57f-d31fc3546d0d"
    let AppConfigurationDataReader = roleDefinition "516239f1-63e1-4d78-a4de-a74fb236a071"
    let ApplicationInsightsComponentContributor = roleDefinition "ae349356-3a1b-4a5e-921d-050484c6347e"
    let ApplicationInsightsSnapshotDebugger = roleDefinition "08954f03-6346-4c2e-81c0-ec3a5cfae23b"
    let AzureKubernetesServiceClusterAdmin = roleDefinition "0ab0b1a8-8aac-4efd-b8c2-3ee1fb270be8"
    let AzureKubernetesServiceClusterUser = roleDefinition "4abbcc35-e782-43d8-92c5-2d3f1bd2253f"
    let AzureServiceBusDataOwner = roleDefinition "090c5cfd-751d-490a-894a-3ce6f1109419"
    let AzureServiceBusDataReceiver = roleDefinition "4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0"
    let AzureServiceBusDataSender = roleDefinition "69a216fc-b8fb-44d8-bc22-1f3c2cd27a39"
    let CosmosDBAccountReader = roleDefinition "fbdf93bf-df7d-467e-a4d2-9458aa1360c8"
    let CosmosDBOperator = roleDefinition "230815da-be43-4aae-9cb4-875f7bd000aa"
    let CosmosBackupOperator = roleDefinition "db7b14f2-5adf-42da-9f96-f2ee17bab5cb"
    let CosmosDBAccountContributor = roleDefinition "5bd9cd88-fe45-4216-938b-f97437e15450"
    let KeyVaultContributor = roleDefinition "f25e0fa2-a7c8-4377-a976-54943a77a395"
    let KeyVaultSecretsOfficer = "b86a8fe4-44ce-4948-aee5-eccb2c155cd7"
    let KeyVaultSecretsUser = "4633458b-17de-408a-b874-0445c86b69e6"
    let ManagedIdentityContributor = roleDefinition "e40ec5ca-96e0-45a2-b4ff-59039f2c2b59"
    let ManagedIdentityOperator = roleDefinition "f1a07417-d97a-45cb-824c-7a7467783830"
    let MonitoringContributor = roleDefinition "749f88d5-cbae-40b8-bcfc-e573ddc772fa"
    let MonitoringMetricsPublisher = roleDefinition "3913510d-42f4-4e42-8a64-420c390055eb"
    let MonitoringReader = roleDefinition "43d0d8ad-25c7-4714-9337-8ba259a9fe05"
    let NetworkContributor = roleDefinition "4d97b98b-1d4f-4787-a291-c67834d212e7"
    let ReaderAndDataAccess = roleDefinition "c12c1c16-33a1-487b-954d-41c89c60f349"
    let RedisCacheContributor = roleDefinition "e0f68234-74aa-48ed-b826-c38b57376e17"
    let SqlDbContributor = roleDefinition "9b7fa17d-e63e-47b0-bb0a-15c516ac86ec"
    let SqlManagedInstanceContributor = roleDefinition "4939a1f6-9ae0-4e48-a1e0-f2cbe897382d"
    let SqlSecurityManager = roleDefinition "056cd41c-7e88-42e1-933e-88ba6a50c9c3"
    let SqlServerContributor = roleDefinition "6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437"
    let StorageAccountContributor = roleDefinition "17d1049b-9a84-46fb-8f53-869881c3d3ab"
    let StorageAccountKeyOperatorService = roleDefinition "81a9662b-bebf-436f-a333-f67b29880f12"
    let StorageBlobDataContributor = roleDefinition "ba92f5b4-2d11-453d-a403-e96b0029c9fe"
    let StorageBlobDataOwner = roleDefinition "b7e6dc6d-f1e8-4753-8033-0f276bb0955b"
    let StorageBlobDataReader = roleDefinition "2a2b9908-6ea1-4ae2-8e65-a410df84e7d1"
    let StorageQueueDataContributor = roleDefinition "974c5e8b-45b9-4653-ba55-5f855dd0fb88"
    let StorageQueueDataMessageProcessor = roleDefinition "8a0f0c08-91a1-4084-bc3d-661d67233fed"
    let StorageQueueDataMessageSender = roleDefinition "c6a89b2d-59bc-44d0-9896-0f6e12d7b80a"
    let StorageQueueDataReader = roleDefinition "19e7f393-937e-4f77-808e-94535e297925"
    let VirtualMachineContributor = roleDefinition "9980e02c-c2be-4d73-94e8-173b1dc7cf3c"
 