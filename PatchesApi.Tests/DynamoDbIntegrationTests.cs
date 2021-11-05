using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace PatchesApi.Tests
{
    public class DynamoDbIntegrationTests<TStartup> : IDisposable where TStartup : class
    {
        public HttpClient Client { get; private set; }
        public IDynamoDBContext DynamoDbContext => _factory?.DynamoDbContext;

        public static string[] PatchByParentId { get; private set; }

        private readonly DynamoDbMockWebApplicationFactory<TStartup> _factory;

        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef {
                Name = "Patches",
                KeyName = "id",
                KeyType = ScalarAttributeType.S,
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(new[]
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "PatchByParentId",
                        KeySchema = new List<KeySchemaElement>(new[]
                        {
                            new KeySchemaElement("parentId", KeyType.HASH)
                        }),
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 10,
                            WriteCapacityUnits = 10
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                })
            }
        };

        public DynamoDbIntegrationTests()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");

            _factory = new DynamoDbMockWebApplicationFactory<TStartup>(_tables);

            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {

                if (null != _factory)
                    _factory.Dispose();
                _disposed = true;
            }
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }


    }

    public class TableDef
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public ScalarAttributeType KeyType { get; set; }
        public List<GlobalSecondaryIndex> GlobalSecondaryIndexes { get; set; } = new List<GlobalSecondaryIndex>();

    }


    [CollectionDefinition("DynamoDb collection", DisableParallelization = true)]
    public class AwsCollection : ICollectionFixture<DynamoDbIntegrationTests<Startup>>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
