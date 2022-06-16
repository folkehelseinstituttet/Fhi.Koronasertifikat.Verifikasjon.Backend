using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.ApiTests
{
    [Category("Unit")]
    public class TextControllerUnitTests
    {
        private readonly TextRequestDto textRequestDto = new TextRequestDto
        {
            CurrentVersionNo = "1.0",
        };

        private readonly Mock<ITextService> textServiceMock = new Mock<ITextService>();

        private readonly Mock<ILogger<TextController>> logger = new Mock<ILogger<TextController>>();
        private byte[] cacheData = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        [Test]
        public async Task Returns_Status_204_When_UpToDate()
        {
            //ResponseDto set to up to date
            TextResponseDto textResponseDto = new TextResponseDto(true, false);

            textServiceMock.Setup(x => x.GetLatestVersionAsync(textRequestDto)).ReturnsAsync(textResponseDto);
            var _TextController = new TextController(textServiceMock.Object, logger.Object);

            var response = await _TextController.GetLatestVersionV3(textRequestDto);

            var sampleResponse = new StatusCodeResult(204);
            Assert.IsInstanceOf<StatusCodeResult>(response);
            Assert.AreEqual(((StatusCodeResult)response).StatusCode, sampleResponse.StatusCode);
        }

        [Test]
        public async Task Returns_File_Content_Result_When_OutDated()
        {
            TextResponseDto textResponseDto = new TextResponseDto(false, false)
            {
                ZipContents = cacheData
            };
            var sampleResponse = new FileContentResult(cacheData, "application/octet-stream");

            textServiceMock.Setup(x => x.GetLatestVersionAsync(textRequestDto)).ReturnsAsync(textResponseDto);
            var _TextController = new TextController(textServiceMock.Object, logger.Object);

            var response = await _TextController.GetLatestVersionV3(textRequestDto);

            Assert.IsInstanceOf<FileContentResult>(response);
            Assert.AreEqual(((FileContentResult)response).FileContents, sampleResponse.FileContents);
        }

    }
}