using Catalog.API;
using Catalog.API.Controllers;
using Catalog.API.Infrastructure;
using Catalog.API.IntegrationEvents;
using Catalog.API.Model;
using Catalog.API.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace Catalog.UnitTests
{
    public class CatalogControllerTest
    {
        private readonly DbContextOptions<CatalogContext> _options;
        public CatalogControllerTest()
        {
            _options = new DbContextOptionsBuilder<CatalogContext>()
               .UseInMemoryDatabase(databaseName: "in-memory")
               .Options;

            using var dbContext = new CatalogContext(_options);
            dbContext.AddRange(GetFakeCatalog());
            dbContext.SaveChanges();
        }
        [Fact]
        public async Task Get_catalog_items_successAsync()
        {
            // Arrange
            var brandFilterApplied = 1;
            var typesFilterApplied = 2;
            var pageSize = 4;
            var pageIndex = 1;

            var expectedItemsInPage = 2;
            var expectedTotalItems = 6;

            var catalogContext = new CatalogContext(_options);
            var testSetting = new TestCatalogSettings();
            var integrationServicesMock = new Mock<ICatalogIntegrationEventService>();

            //Act
            CatalogController catalogController = new CatalogController(catalogContext, testSetting, integrationServicesMock.Object);
            var actionResult = await catalogController.ItemsByTypeIdAndBrandIdAsync(typesFilterApplied, brandFilterApplied, pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actionResult);
            var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actionResult.Value);

            Assert.Equal(expectedTotalItems, page.Count);
            Assert.Equal(pageIndex, page.PageIndex);
            Assert.Equal(pageSize, page.PageSize);
            Assert.Equal(expectedItemsInPage, page.Data.Count());
        }

        private List<CatalogItem> GetFakeCatalog()
        {
            return new List<CatalogItem>()
        {
            new()
            {
                Id = 1,
                Name = "fakeItemA",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemA.png"
            },
            new()
            {
                Id = 2,
                Name = "fakeItemB",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemB.png"
            },
            new()
            {
                Id = 3,
                Name = "fakeItemC",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemC.png"
            },
            new()
            {
                Id = 4,
                Name = "fakeItemD",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemD.png"
            },
            new()
            {
                Id = 5,
                Name = "fakeItemE",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemE.png"
            },
            new()
            {
                Id = 6,
                Name = "fakeItemF",
                Description = "Test",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemF.png"
            }
        };
        }
    }
    public class TestCatalogSettings : IOptionsSnapshot<CatalogSettings>
    {
        public CatalogSettings Value => new()
        {
            PicBaseUrl = "http://image-server.com/",
            AzureStorageEnabled = true
        };

        public CatalogSettings Get(string name) => Value;
    }
}