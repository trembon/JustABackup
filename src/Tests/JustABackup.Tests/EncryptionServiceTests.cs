using JustABackup.Core.Services;
using JustABackup.Database.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace JustABackup.Tests
{
    public class EncryptionServiceTests
    {
        private EncryptionService SetupService()
        {
            string passphraseInDB = Guid.NewGuid().ToString();

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["encryption:storage"]).Returns("database");

            var passphraseRepo = new Mock<IPassphraseRepository>();
            passphraseRepo.Setup(pr => pr.GetOrAdd()).ReturnsAsync(passphraseInDB);

            return new EncryptionService(configuration.Object, passphraseRepo.Object);
        }

        [Theory]
        [InlineData("hello world")]
        [InlineData(1)]
        [InlineData(1.1)]
        [InlineData(true)]
        public async void EncryptionWithoutExceptions(object param)
        {
            var service = SetupService();
            byte[] encryptedObject = await service.Encrypt(param);
            Assert.NotEmpty(encryptedObject);
        }

        [Fact]
        public async void EncryptionWithoutExceptions_Null()
        {
            var service = SetupService();
            byte[] encryptedObject = await service.Encrypt<object>(null);
            Assert.Empty(encryptedObject);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("hello world")]
        [InlineData(1)]
        [InlineData(1.1)]
        [InlineData(true)]
        public async void DecryptAfterEncryptionSameObject(object param)
        {
            var service = SetupService();

            byte[] encryptedObject = await service.Encrypt(param);
            object decryptedObject = await service.Decrypt<object>(encryptedObject);

            Assert.Equal(param, decryptedObject);
        }

        [Fact]
        public async void DecryptNullAndEmptyWithoutError_String()
        {
            var service = SetupService();
            string decryptedObjectNull = await service.Decrypt<string>(null);
            string decryptedObjectEmpty = await service.Decrypt<string>(new byte[0]);

            Assert.Null(decryptedObjectNull);
            Assert.Null(decryptedObjectEmpty);
        }

        [Fact]
        public async void DecryptNullAndEmptyWithoutError_Integer()
        {
            var service = SetupService();
            int decryptedObjectNull = await service.Decrypt<int>(null);
            int decryptedObjectEmpty = await service.Decrypt<int>(new byte[0]);

            Assert.Equal(0, decryptedObjectNull);
            Assert.Equal(0, decryptedObjectEmpty);
        }

        [Fact]
        public async void DecryptNullAndEmptyWithoutError_Boolean()
        {
            var service = SetupService();
            bool decryptedObjectNull = await service.Decrypt<bool>(null);
            bool decryptedObjectEmpty = await service.Decrypt<bool>(new byte[0]);

            Assert.False(decryptedObjectNull);
            Assert.False(decryptedObjectEmpty);
        }
    }
}
