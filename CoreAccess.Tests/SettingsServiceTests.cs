using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CoreAccess.Tests
{
    public class SettingsServiceTests
    {
        private readonly Mock<ISettingsRepository> _settingsRepoMock;
        private readonly Mock<ISecretProtector> _protectorMock;
        private readonly IMemoryCache _cache;
        private readonly SettingsService _settingsService;

        public SettingsServiceTests()
        {
            _settingsRepoMock = new Mock<ISettingsRepository>();
            _protectorMock = new Mock<ISecretProtector>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            _settingsService = new SettingsService(
                _settingsRepoMock.Object,
                _protectorMock.Object,
                _cache
            );
        }

        [Fact]
        public async Task GetAsync_ReturnsValue_FromRepository()
        {
            // Arrange
            var setting = new Setting { Key = "SiteName", Value = "CoreAccess" };
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.Is<SettingSearchOptions>(o => o.Key == "SiteName"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting> { setting });

            // Act
            var result = await _settingsService.GetAsync("SiteName");

            // Assert
            Assert.Equal("CoreAccess", result);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.IsAny<SettingSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting>());

            // Act
            var result = await _settingsService.GetAsync("UnknownKey");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_UsesCache_OnSecondCall()
        {
            // Arrange
            var setting = new Setting { Key = "CachedKey", Value = "FirstValue" };
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.IsAny<SettingSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting> { setting });

            // Act
            var firstCall = await _settingsService.GetAsync("CachedKey");
            var secondCall = await _settingsService.GetAsync("CachedKey");

            // Assert
            Assert.Equal("FirstValue", firstCall);
            Assert.Equal("FirstValue", secondCall);
            _settingsRepoMock.Verify(r => r.SearchSettingsAsync(It.IsAny<SettingSearchOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_DecryptsSecret_WhenIsSecret()
        {
            // Arrange
            var setting = new Setting { Key = "ApiKey", IsSecret = true, EncryptedValue = "Encrypted123" };
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.Is<SettingSearchOptions>(o => o.Key == "ApiKey"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting> { setting });
            _protectorMock
                .Setup(p => p.Unprotect("Encrypted123"))
                .Returns("DecryptedValue");

            // Act
            var result = await _settingsService.GetAsync("ApiKey");

            // Assert
            Assert.Equal("DecryptedValue", result);
        }

        [Fact]
        public async Task GetTokenLifetimeAsync_ReturnsParsedValue()
        {
            // Arrange
            var setting = new Setting { Key = "TokenLifetime", Value = "7200" };
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.Is<SettingSearchOptions>(o => o.Key == "TokenLifetime"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting> { setting });

            // Act
            var result = await _settingsService.GetTokenLifetimeAsync();

            // Assert
            Assert.Equal(7200, result);
        }

        [Fact]
        public async Task GetTokenLifetimeAsync_ReturnsDefault_WhenInvalid()
        {
            // Arrange
            var setting = new Setting { Key = "TokenLifetime", Value = "notanumber" };
            _settingsRepoMock
                .Setup(r => r.SearchSettingsAsync(It.IsAny<SettingSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Setting> { setting });

            // Act
            var result = await _settingsService.GetTokenLifetimeAsync();

            // Assert
            Assert.Equal(3600, result);
        }
    }
}
