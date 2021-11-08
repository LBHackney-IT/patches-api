using FluentAssertions;
using PatchesApi.V1.Domain;
using PatchesApi.V1.Factories;
using Xunit;

namespace PatchesApi.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {

        [Fact]
        public void CanMapADatabaseEntityToADomainObject()
        {
            //Arrange
            var domain = new PatchEntity();
            //Act
            var response = domain.ToResponse();

            //Assert
            domain.Id.Should().Be(response.Id);
            domain.Name.Should().Be(response.Name);
            domain.ParentId.Should().Be(response.ParentId);
            domain.PatchType.Should().Be(response.PatchType);
            domain.ResponsibleEntities.Should().BeEquivalentTo(response.ResponsibleEntities);
        }
    }
}
