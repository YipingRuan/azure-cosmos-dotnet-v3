﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.SDK.EmulatorTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.Azure.Cosmos.Tracing;
    using Microsoft.Azure.Cosmos.Telemetry;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Azure.Cosmos.CosmosElements;

    [TestClass]
    public class ClientTelemetryTests : BaseCosmosClientHelper
    {
        private const string telemetryEndpointUrl = "http://dummy.telemetry.endpoint/";
        private const int scheduledInSeconds = 1;
        private CosmosClientBuilder cosmosClientBuilder;
        private AccountProperties accountProperties;

        private List<ClientTelemetryProperties> actualInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            this.actualInfo = new List<ClientTelemetryProperties>();

            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, "1");
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetryEndpoint, telemetryEndpointUrl);

            HttpClientHandlerHelper httpHandler = new HttpClientHandlerHelper
            {
                RequestCallBack = (request, cancellation) =>
                {
                    if (request.RequestUri.AbsoluteUri.Equals(ClientTelemetryOptions.GetClientTelemetryEndpoint().AbsoluteUri))
                    {
                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                        string jsonObject = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        lock (this.actualInfo)
                        {
                            this.actualInfo.Add(JsonConvert.DeserializeObject<ClientTelemetryProperties>(jsonObject));
                        }

                        return Task.FromResult(result);
                    }
                    else if (request.RequestUri.AbsoluteUri.Equals(ClientTelemetryOptions.GetVmMetadataUrl().AbsoluteUri))
                    {
                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                        object jsonObject = JsonConvert.DeserializeObject("{\"compute\":{\"azEnvironment\":\"AzurePublicCloud\",\"customData\":\"\",\"isHostCompatibilityLayerVm\":\"false\",\"licenseType\":\"\",\"location\":\"eastus\",\"name\":\"sourabh-testing\",\"offer\":\"UbuntuServer\",\"osProfile\":{\"adminUsername\":\"azureuser\",\"computerName\":\"sourabh-testing\"},\"osType\":\"Linux\",\"placementGroupId\":\"\",\"plan\":{\"name\":\"\",\"product\":\"\",\"publisher\":\"\"},\"platformFaultDomain\":\"0\",\"platformUpdateDomain\":\"0\",\"provider\":\"Microsoft.Compute\",\"publicKeys\":[{\"keyData\":\"ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC5uCeOAm3ehmhI+2PbMoMl17Eo\r\nqfHKCycSaBJsv9qxlmBOuFheSJc1XknJleXUSsuTO016/d1PyWpevnqOZNRksWoa\r\nJvQ23sDTxcK+X2OP3QlCUeX4cMjPXqlL8z1UYzU4Bx3fFvf8fs67G3N72sxWBw5P\r\nZyuXyhBm0NCe/2NYMKgEDT4ma8XszO0ikbhoPKbMbgHAQk/ktWQHNcqYOPQKEWqp\r\nEK1R0rjS2nmtovfScP/ZGXcvOpJ1/NDBo4dh1K+OxOGM/4PSH/F448J5Zy4eAyEk\r\nscys+IpeIOTOlRUy/703SNIX0LEWlnYqbyL9c1ypcYLQqF76fKkDfzzFI/OWVlGw\r\nhj/S9uP8iMsR+fhGIbn6MAa7O4DWPWLuedSp7KDYyjY09gqNJsfuaAJN4LiC6bPy\r\nhknm0PVLK3ux7EUOt+cZrHCdIFWbdOtxiPNIl1tkv9kV5aE5Aj2gJm4MeB9uXYhS\r\nOuksboBc0wyUGrl9+XZJ1+NlZOf7IjVi86CieK8= generated-by-azure\r\n\",\"path\":\"/home/azureuser/.ssh/authorized_keys\"}],\"publisher\":\"Canonical\",\"resourceGroupName\":\"sourabh-telemetry-sdk\",\"resourceId\":\"/subscriptions/8fba6d4f-7c37-4d13-9063-fd58ad2b86e2/resourceGroups/sourabh-telemetry-sdk/providers/Microsoft.Compute/virtualMachines/sourabh-testing\",\"securityProfile\":{\"secureBootEnabled\":\"false\",\"virtualTpmEnabled\":\"false\"},\"sku\":\"18.04-LTS\",\"storageProfile\":{\"dataDisks\":[],\"imageReference\":{\"id\":\"\",\"offer\":\"UbuntuServer\",\"publisher\":\"Canonical\",\"sku\":\"18.04-LTS\",\"version\":\"latest\"},\"osDisk\":{\"caching\":\"ReadWrite\",\"createOption\":\"FromImage\",\"diffDiskSettings\":{\"option\":\"\"},\"diskSizeGB\":\"30\",\"encryptionSettings\":{\"enabled\":\"false\"},\"image\":{\"uri\":\"\"},\"managedDisk\":{\"id\":\"/subscriptions/8fba6d4f-7c37-4d13-9063-fd58ad2b86e2/resourceGroups/sourabh-telemetry-sdk/providers/Microsoft.Compute/disks/sourabh-testing_OsDisk_1_9a54abfc5ba149c6a106bd9e5b558c2a\",\"storageAccountType\":\"Premium_LRS\"},\"name\":\"sourabh-testing_OsDisk_1_9a54abfc5ba149c6a106bd9e5b558c2a\",\"osType\":\"Linux\",\"vhd\":{\"uri\":\"\"},\"writeAcceleratorEnabled\":\"false\"}},\"subscriptionId\":\"8fba6d4f-7c37-4d13-9063-fd58ad2b86e2\",\"tags\":\"azsecpack:nonprod;platformsettings.host_environment.service.platform_optedin_for_rootcerts:true\",\"tagsList\":[{\"name\":\"azsecpack\",\"value\":\"nonprod\"},{\"name\":\"platformsettings.host_environment.service.platform_optedin_for_rootcerts\",\"value\":\"true\"}],\"version\":\"18.04.202103250\",\"vmId\":\"d0cb93eb-214b-4c2b-bd3d-cc93e90d9efd\",\"vmScaleSetName\":\"\",\"vmSize\":\"Standard_D2s_v3\",\"zone\":\"1\"},\"network\":{\"interface\":[{\"ipv4\":{\"ipAddress\":[{\"privateIpAddress\":\"10.0.7.5\",\"publicIpAddress\":\"\"}],\"subnet\":[{\"address\":\"10.0.7.0\",\"prefix\":\"24\"}]},\"ipv6\":{\"ipAddress\":[]},\"macAddress\":\"000D3A8F8BA0\"}]}}");
                        string payload = JsonConvert.SerializeObject(jsonObject);
                        result.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                        return Task.FromResult(result);
                    }
                    return null;
                }
            };

            List<string> preferredRegionList = new List<string>
            {
                "region1",
                "region2"
            };

            this.cosmosClientBuilder = TestCommon.GetDefaultConfiguration()
                                        .WithApplicationPreferredRegions(preferredRegionList)
                                        .WithTelemetryEnabled()
                                        .WithHttpClientFactory(() => new HttpClient(httpHandler));
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, null);
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetryEndpoint, null);

            await base.TestCleanup();
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task PointSuccessOperationsTest(ConnectionMode mode)
        {
            // Passing consistency level at client level
            Container container = await this.GetContainer(
                mode: mode,
                consistencyLevel: ConsistencyLevel.Eventual);

            // Create an item
            ToDoActivity testItem = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue");
            ItemResponse<ToDoActivity> createResponse = await container.CreateItemAsync<ToDoActivity>(testItem);
            ToDoActivity testItemCreated = createResponse.Resource;

            // Read an Item
            await container.ReadItemAsync<ToDoActivity>(testItem.id, new Cosmos.PartitionKey(testItem.id));

            // Upsert an Item
            await container.UpsertItemAsync<ToDoActivity>(testItem);

            // Replace an Item
            await container.ReplaceItemAsync<ToDoActivity>(testItemCreated, testItemCreated.id.ToString());

            // Patch an Item
            List<PatchOperation> patch = new List<PatchOperation>()
            {
                PatchOperation.Add("/new", "patched")
            };
            await ((ContainerInternal)container).PatchItemAsync<ToDoActivity>(
                testItem.id,
                new Cosmos.PartitionKey(testItem.id),
                patch);

            // Delete an Item
            await container.DeleteItemAsync<ToDoActivity>(testItem.id, new Cosmos.PartitionKey(testItem.id));

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Create.ToString(), 1},
                { Documents.OperationType.Upsert.ToString(), 1},
                { Documents.OperationType.Read.ToString(), 1},
                { Documents.OperationType.Replace.ToString(), 1},
                { Documents.OperationType.Patch.ToString(), 1},
                { Documents.OperationType.Delete.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 12,
                expectedOperationRecordCountMap: expectedRecordCountInOperation,
                expectedConsistencyLevel: ConsistencyLevel.Eventual.ToString().ToUpper());
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task SingleOperationMultipleTimes(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            ItemRequestOptions requestOptions = new ItemRequestOptions()
            {
                Properties = properties
            };

            // Create an item
            ToDoActivity testItem = ToDoActivity.CreateRandomToDoActivity();

            await container.CreateItemAsync<ToDoActivity>(testItem, requestOptions: requestOptions);

            Task.Delay(2000).Wait();
           /* for(int count = 0; count < 1; count++)
            {
                // Read an Item
                await container.ReadItemAsync<ToDoActivity>(testItem.id, new Cosmos.PartitionKey(testItem.id));
            }

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Read.ToString(), 1},
                { Documents.OperationType.Create.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 4, // 2 (read, requetLatency + requestCharge) + 2 (create, requestLatency + requestCharge)
                expectedOperationRecordCountMap: expectedRecordCountInOperation); */
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task PointReadFailureOperationsTest(ConnectionMode mode)
        {
            // Fail Read
            try
            {
                Container container = await this.GetContainer(mode);
                await container.ReadItemAsync<JObject>(
                    new Guid().ToString(),
                    new Cosmos.PartitionKey(new Guid().ToString()));
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                string message = ce.ToString();
                Assert.IsNotNull(message);
            }


            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Read.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 2,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task StreamReadFailureOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            // Fail Read
            try
            {
                await container.ReadItemStreamAsync(
                    new Guid().ToString(),
                    new Cosmos.PartitionKey(new Guid().ToString()));
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                string message = ce.ToString();
                Assert.IsNotNull(message);
            }

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Read.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 2,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task StreamOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);
            // Create an item
            var testItem = new { id = "MyTestItemId", partitionKeyPath = "MyTestPkValue", details = "it's working", status = "done" };
            await container
                .CreateItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem),
                new Cosmos.PartitionKey(testItem.id));

            //Upsert an Item
            await container.UpsertItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem), new Cosmos.PartitionKey(testItem.id));

            //Read an Item
            await container.ReadItemStreamAsync(testItem.id, new Cosmos.PartitionKey(testItem.id));

            //Replace an Item
            await container.ReplaceItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem), testItem.id, new Cosmos.PartitionKey(testItem.id));

            // Patch an Item
            List<PatchOperation> patch = new List<PatchOperation>()
            {
                PatchOperation.Add("/new", "patched")
            };
            await ((ContainerInternal)container).PatchItemStreamAsync(
                partitionKey: new Cosmos.PartitionKey(testItem.id),
                id: testItem.id,
                patchOperations: patch);

            //Delete an Item
            await container.DeleteItemStreamAsync(testItem.id, new Cosmos.PartitionKey(testItem.id));

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Create.ToString(), 1},
                { Documents.OperationType.Upsert.ToString(), 1},
                { Documents.OperationType.Read.ToString(), 1},
                { Documents.OperationType.Replace.ToString(), 1},
                { Documents.OperationType.Patch.ToString(), 1},
                { Documents.OperationType.Delete.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 12,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task BatchOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);
            using (BatchAsyncContainerExecutor executor =
                new BatchAsyncContainerExecutor(
                    (ContainerInlineCore)container,
                    ((ContainerInlineCore)container).ClientContext,
                    20,
                    Documents.Constants.MaxDirectModeBatchRequestBodySizeInBytes)
                )
            {
                List<Task<TransactionalBatchOperationResult>> tasks = new List<Task<TransactionalBatchOperationResult>>();
                for (int i = 0; i < 10; i++)
                {
                    tasks.Add(executor.AddAsync(ClientTelemetryTests.CreateItem(i.ToString()), NoOpTrace.Singleton, default));
                }

                await Task.WhenAll(tasks);
            }


            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Batch.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 2,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryOperationSinglePartitionTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            ToDoActivity testItem = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue", "MyTestItemId");
            Dictionary<string, object> properties = new Dictionary<string, object>();
            ItemRequestOptions requestOptions = new ItemRequestOptions()
            {
                ConsistencyLevel = ConsistencyLevel.ConsistentPrefix,
                Properties = properties
            };

            ItemResponse<ToDoActivity> createResponse = await container.CreateItemAsync<ToDoActivity>(
                item: testItem, 
                requestOptions: requestOptions);

            QueryRequestOptions queryRequestOptions = new QueryRequestOptions()
            {
                ConsistencyLevel = ConsistencyLevel.ConsistentPrefix,
            };

            List<object> families = new List<object>();
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                string sqlQueryText = "SELECT * FROM c";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                using (FeedIterator<object> queryResultSetIterator = container.GetItemQueryIterator<object>(
                    queryDefinition: queryDefinition,
                    requestOptions: queryRequestOptions))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (object family in currentResultSet)
                        {
                            families.Add(family);
                        }
                    }
                }

                Assert.AreEqual(1, families.Count);
                
            }

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Query.ToString(), 1},
                { Documents.OperationType.Create.ToString(), 1}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 4,
                expectedOperationRecordCountMap: expectedRecordCountInOperation,
                expectedConsistencyLevel: ConsistencyLevel.ConsistentPrefix.ToString().ToUpper());
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryMultiPageSinglePartitionOperationTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode: mode, 
                consistencyLevel: ConsistencyLevel.Eventual);

            ItemRequestOptions requestOptions = new ItemRequestOptions()
            {
                ConsistencyLevel = ConsistencyLevel.ConsistentPrefix
            };

            ToDoActivity testItem1 = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue1", "MyTestItemId1");
            ItemResponse<ToDoActivity> createResponse1 = await container.CreateItemAsync<ToDoActivity>(
                item: testItem1,
                requestOptions: requestOptions);
            ToDoActivity testItem2 = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue2", "MyTestItemId2");
            ItemResponse<ToDoActivity> createResponse2 = await container.CreateItemAsync<ToDoActivity>(
                item:testItem2,
                requestOptions: requestOptions);

            if (createResponse1.StatusCode == HttpStatusCode.Created && 
                createResponse2.StatusCode == HttpStatusCode.Created)
            {
                string sqlQueryText = "SELECT * FROM c";

                List<object> families = new List<object>();
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                using (FeedIterator<object> queryResultSetIterator = container.GetItemQueryIterator<object>(
                    queryDefinition: queryDefinition,
                    requestOptions: new QueryRequestOptions()
                    {
                        ConsistencyLevel = ConsistencyLevel.ConsistentPrefix,
                        MaxItemCount = 1
                    }))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (object family in currentResultSet)
                        {
                            families.Add(family);
                        }
                    }
                }

                Assert.AreEqual(2, families.Count);
               
            }

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Query.ToString(), 3},
                { Documents.OperationType.Create.ToString(), 2}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 4,
                expectedOperationRecordCountMap: expectedRecordCountInOperation,
                expectedConsistencyLevel: ConsistencyLevel.ConsistentPrefix.ToString().ToUpper());
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryOperationCrossPartitionTest(ConnectionMode mode)
        {
            // Multi Partiton Operation takes time
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, "20");

            ContainerInternal itemsCore = (ContainerInternal)await this.GetContainer(
                mode: mode,
                isLargeContainer: true);

            // Verify container has multiple partitions
            int pkRangesCount = (await itemsCore.ClientContext.DocumentClient.ReadPartitionKeyRangeFeedAsync(itemsCore.LinkUri)).Count;

            IEnumerable<FeedRange> tokens = await itemsCore.GetFeedRangesAsync();
            Assert.IsTrue(pkRangesCount > 1, "Should have created a multi partition container.");
            Assert.AreEqual(pkRangesCount, tokens.Count());

            Container container = (Container)itemsCore;

            await ToDoActivity.CreateRandomItems(
                container: container, 
                pkCount: 2, 
                perPKItemCount: 5);
            
            string sqlQueryText = "SELECT * FROM c";

            List<object> families = new List<object>();

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using (FeedIterator<object> queryResultSetIterator = container.GetItemQueryIterator<object>(queryDefinition))
            {
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (object family in currentResultSet)
                    {
                        families.Add(family);
                    }
                }
            }

            Assert.AreEqual(10, families.Count);

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Query.ToString(), pkRangesCount},
                { Documents.OperationType.Create.ToString(), 10}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 6,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryOperationMutiplePageCrossPartitionTest(ConnectionMode mode)
        {
            // Multi Partiton Operation takes time
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, "20");

            ContainerInternal itemsCore = (ContainerInternal)await this.GetContainer(
                mode: mode,
                isLargeContainer: true);

            // Verify container has multiple partitions
            int pkRangesCount = (await itemsCore.ClientContext.DocumentClient.ReadPartitionKeyRangeFeedAsync(itemsCore.LinkUri)).Count;

            IEnumerable<FeedRange> tokens = await itemsCore.GetFeedRangesAsync();
            Assert.IsTrue(pkRangesCount > 1, "Should have created a multi partition container.");
            Assert.AreEqual(pkRangesCount, tokens.Count());

            Container container = (Container)itemsCore;

            await ToDoActivity.CreateRandomItems(
                container: container,
                pkCount: 2,
                perPKItemCount: 5);

            string sqlQueryText = "SELECT * FROM c";

            List<object> families = new List<object>();
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using (FeedIterator<object> queryResultSetIterator = container.GetItemQueryIterator<object>(
                 queryDefinition: queryDefinition,
                 requestOptions: new QueryRequestOptions()
                 {
                     MaxItemCount = 1
                 }))
            {
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (object family in currentResultSet)
                    {
                        families.Add(family);
                    }
                }
            }

            Assert.AreEqual(10, families.Count);

            IDictionary<string, long> expectedRecordCountInOperation = new Dictionary<string, long>
            {
                { Documents.OperationType.Query.ToString(), pkRangesCount + 10}, // 10 is number of items
                { Documents.OperationType.Create.ToString(), 10}
            };

            await this.WaitAndAssert(
                expectedOperationCount: 4,
                expectedOperationRecordCountMap: expectedRecordCountInOperation);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryOperationInvalidContinuationToken(ConnectionMode mode)
        {
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, "1");
            Container container = await this.GetContainer(mode);

            List<ToDoActivity> results = new List<ToDoActivity>();
            using (FeedIterator<ToDoActivity> resultSetIterator = container.GetItemQueryIterator<ToDoActivity>(
                  "SELECT * FROM c",
                  continuationToken: "dummy token"))
            {
                try
                {
                    while (resultSetIterator.HasMoreResults)
                    {
                        FeedResponse<ToDoActivity> response = await resultSetIterator.ReadNextAsync();
                        results.AddRange(response);
                    }
                }
                catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.BadRequest)
                {
                    string message = ce.ToString();
                    Assert.IsNotNull(message);
                }
            }

            await this.WaitAndAssert(expectedOperationCount: 0); // Does not record telemetry
        }

        private async Task WaitAndAssert(int? expectedOperationCount, 
            IDictionary<string, long> expectedOperationRecordCountMap = null, 
            string expectedConsistencyLevel = null)
        {
            Assert.IsNotNull(this.actualInfo, "Telemetry Information not available");

            // As this feature is thread based execution so wait for the results to avoid test flakiness
            List<ClientTelemetryProperties> localCopyOfActualInfo = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                lock (this.actualInfo)
                {
                    int operationCount = this.actualInfo.Sum(x => x.OperationInfo.Count);

                    Assert.IsTrue(operationCount <= expectedOperationCount, $"actual operations({operationCount}) are more than expected operations({expectedOperationCount})");

                    if (operationCount == expectedOperationCount)
                    {
                        // Copy the list to avoid it being modified while validating
                        localCopyOfActualInfo = new List<ClientTelemetryProperties>(this.actualInfo);
                        break;
                    }

                    Assert.IsTrue(stopwatch.Elapsed.TotalMinutes < 1, $"The expected operation count({expectedOperationCount}) was never same as actual operation count({operationCount}).  ActualInfo:{JsonConvert.SerializeObject(this.actualInfo)}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
            while (localCopyOfActualInfo == null);

            if(expectedConsistencyLevel == null)
            {
                expectedConsistencyLevel = this.accountProperties.Consistency.DefaultConsistencyLevel.ToString().ToUpper();
            }

            List<OperationInfo> actualOperationList = new List<OperationInfo>();
            List<SystemInfo> actualSystemInformation = new List<SystemInfo>();

            // Asserting If basic client telemetry object is as expected
            foreach (ClientTelemetryProperties telemetryInfo in localCopyOfActualInfo)
            {
                actualOperationList.AddRange(telemetryInfo.OperationInfo);
                actualSystemInformation.AddRange(telemetryInfo.SystemInfo);

                // TODO: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2728
                //Assert.AreEqual(2, telemetryInfo.SystemInfo.Count, $"System Information Count doesn't Match; {JsonConvert.SerializeObject(telemetryInfo.SystemInfo)}");

                Assert.IsNotNull(telemetryInfo.GlobalDatabaseAccountName, "GlobalDatabaseAccountName is null");
                Assert.IsNotNull(telemetryInfo.DateTimeUtc, "Timestamp is null");
                Assert.AreEqual(2, telemetryInfo.PreferredRegions.Count);
                Assert.AreEqual("region1", telemetryInfo.PreferredRegions[0]);
                Assert.AreEqual("region2", telemetryInfo.PreferredRegions[1]);

            }
            Assert.AreEqual(expectedOperationCount, actualOperationList.Count, "Operation Information Count doesn't Match");

            IDictionary<string, long> actualOperationRecordCountMap = new Dictionary<string, long>();
            // Asserting If operation list is as expected
            foreach (OperationInfo operation in actualOperationList)
            {
                Assert.IsNotNull(operation.Operation, "Operation Type is null");
                Assert.IsNotNull(operation.Resource, "Resource Type is null");
                Assert.IsNotNull(operation.StatusCode, "StatusCode is null");
                Assert.IsNotNull(operation.Consistency, "Consistency is null");
                Assert.AreEqual(expectedConsistencyLevel, operation.Consistency, $"Consistency is not {this.accountProperties.Consistency.DefaultConsistencyLevel.ToString().ToUpper()}");

                Assert.IsNotNull(operation.MetricInfo, "MetricInfo is null");
                Assert.IsNotNull(operation.MetricInfo.MetricsName, "MetricsName is null");
                Assert.IsNotNull(operation.MetricInfo.UnitName, "UnitName is null");
                Assert.IsNotNull(operation.MetricInfo.Percentiles, "Percentiles is null");
                Assert.IsTrue(operation.MetricInfo.Count > 0, "MetricInfo Count is not greater than 0");

                if(operation.MetricInfo.MetricsName.Equals(ClientTelemetryOptions.RequestChargeName)) // putting this condition to avoid doubling of count as we have same information for each metrics
                {
                    if (!actualOperationRecordCountMap.TryGetValue(operation.Operation.ToString(), out long recordCount))
                    {
                        actualOperationRecordCountMap.Add(operation.Operation.ToString(), operation.MetricInfo.Count);
                    }
                    else
                    {
                        actualOperationRecordCountMap.Remove(operation.Operation.ToString());
                        actualOperationRecordCountMap.Add(operation.Operation.ToString(), recordCount + operation.MetricInfo.Count);
                    }
                }

                Assert.IsTrue(operation.MetricInfo.Mean >= 0, "MetricInfo Mean is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Max >= 0, "MetricInfo Max is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Min >= 0, "MetricInfo Min is not greater than or equal to 0");
            }

            if (expectedOperationRecordCountMap != null)
            {
                Assert.IsTrue(expectedOperationRecordCountMap.EqualsTo(actualOperationRecordCountMap), "record count for operation does not match");
            }

            // Asserting If system information list is as expected
            foreach (SystemInfo operation in actualSystemInformation)
            {
                Assert.IsNotNull(operation.MetricInfo, "MetricInfo is null");
                Assert.IsNotNull(operation.MetricInfo.MetricsName, "MetricsName is null");
                Assert.IsNotNull(operation.MetricInfo.UnitName, "UnitName is null");
                Assert.IsNotNull(operation.MetricInfo.Percentiles, "Percentiles is null");
                Assert.IsTrue(operation.MetricInfo.Count > 0, "MetricInfo Count is not greater than 0");
                Assert.IsTrue(operation.MetricInfo.Mean >= 0, "MetricInfo Mean is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Max >= 0, "MetricInfo Max is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Min >= 0, "MetricInfo Min is not greater than or equal to 0");
            }
        }

        private static ItemBatchOperation CreateItem(string itemId)
        {
            var testItem = new { id = itemId, Status = itemId };
            return new ItemBatchOperation(
                operationType: Documents.OperationType.Create, 
                operationIndex: 0, 
                partitionKey: new Cosmos.PartitionKey(itemId), 
                id: itemId, 
                resourceStream: TestCommon.SerializerCore.ToStream(testItem));
        }

        private async Task<ContainerResponse> GetContainer(ConnectionMode mode, bool isLargeContainer = false, ConsistencyLevel? consistencyLevel = null)
        {
            if (consistencyLevel.HasValue)
            {
                this.cosmosClientBuilder = this.cosmosClientBuilder.WithConsistencyLevel(consistencyLevel.Value);
            }

            if (mode == ConnectionMode.Direct)
            {
                this.cosmosClient = this.cosmosClientBuilder.WithConnectionModeDirect().Build();
            }

            if (mode == ConnectionMode.Gateway)
            {
                this.cosmosClient = this.cosmosClientBuilder.WithConnectionModeGateway().Build();
            }

            this.accountProperties = await this.cosmosClient.ReadAccountAsync();
            this.database = await this.cosmosClient.CreateDatabaseAsync(Guid.NewGuid().ToString());
            if (isLargeContainer)
            {
                return await this.database.CreateContainerAsync(
                    id: Guid.NewGuid().ToString(), 
                    partitionKeyPath: "/id", 
                    throughput: 30000);
            }
            return await this.database.CreateContainerAsync(
                id: Guid.NewGuid().ToString(),
                partitionKeyPath: "/id");
        }

    }
}
