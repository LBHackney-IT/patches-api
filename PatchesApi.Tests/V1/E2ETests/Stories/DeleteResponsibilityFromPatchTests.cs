using AutoFixture;
using PatchesApi.Tests.V1.E2ETests.Fixtures;
using PatchesApi.Tests.V1.E2ETests.Steps;
using PatchesApi.V1.Boundary.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace PatchesApi.Tests.V1.E2ETests.Stories
{
    [Story(
       AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
       IWant = "the ability to delete a responsibility from a patch/area",
       SoThat = " can end their relationship to the patch/area"
   )]
    [Collection("DynamoDb collection")]
    public class DeleteResponsibilityFromPatchTests : IDisposable
    {
        private readonly DynamoDbIntegrationTests<Startup> _dbFixture;

        private readonly PatchesFixtures _patchFixture;
        private readonly DeleteResponsibilityFromPatchStep _steps;
        private readonly Fixture _fixture = new Fixture();

        public DeleteResponsibilityFromPatchTests(DynamoDbIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _patchFixture = new PatchesFixtures(_dbFixture.DynamoDbContext);
            _steps = new DeleteResponsibilityFromPatchStep(_dbFixture.Client);
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
                if (null != _patchFixture)
                    _patchFixture.Dispose();
                if (null != _steps)
                    _steps.Dispose();
                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturns404WhenPatchDoesntExist()
        {
            var query = _fixture.Create<DeleteResponsibilityFromPatchRequest>();

            this.Given(g => _patchFixture.GivenAPatchDoesNotExist())
                .When(w => _steps.WhenDeleteResponsibilityFromPatchApiIsCalledAsync(query))
                .Then(t => _steps.NotFoundResponseReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns404WhenResponsibilityIdDoesntExistInPatch()
        {
            this.Given(g => _patchFixture.GivenAPatchExistsWithNoResponsibileEntity())
                .When(w => _steps.WhenDeleteResponsibilityFromPatchApiIsCalledAsync(new DeleteResponsibilityFromPatchRequest
                {
                    Id = _patchFixture.Id,
                    ResponsibileEntityId = _fixture.Create<Guid>()
                }))
                .Then(t => _steps.NotFoundResponseReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns204WhenResponsibilityWasRemovedFromPatch()
        {
            // patch and responsibility exist
            this.Given(g => _patchFixture.GivenAPatchExistsWithManyResponsibility())
                .When(w => _steps.WhenDeleteResponsibilityFromPatchApiIsCalledAsync(new DeleteResponsibilityFromPatchRequest
                {
                    Id = _patchFixture.Id,
                    ResponsibileEntityId = _patchFixture.PatchesDb.ResponsibleEntities.First().Id
                }))
                .Then(t => _steps.NoContentResponseReturned())
                .And(a => _steps.ResponsibilityRemovedFromPatch(_patchFixture.Id, _patchFixture.PatchesDb.ResponsibleEntities.First().Id, _patchFixture))
                .BDDfy();
        }

    }
}
