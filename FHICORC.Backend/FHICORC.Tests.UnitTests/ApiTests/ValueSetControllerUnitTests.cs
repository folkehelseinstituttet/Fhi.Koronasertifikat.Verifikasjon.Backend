using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.ApiTests
{
    [Category("Unit")]
    public class ValueSetControllerUnitTests
    {
        private readonly ValueSetRequestDto _valueSetRequestDto = new ValueSetRequestDto();
        private readonly Mock<IValueSetService> _valueSetServiceMock = new Mock<IValueSetService>();
        private readonly byte[] _cacheData = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        [Test]
        public async Task Returns_Status_204_When_UpToDate()
        {
            //ResponseDto set to up to date
            ValueSetResponseDto valueSetResponseDto = new ValueSetResponseDto(true, false);

            _valueSetServiceMock.Setup(x => x.GetLatestVersionAsync(_valueSetRequestDto)).ReturnsAsync(valueSetResponseDto);
            var valueSetController = new ValueSetController(_valueSetServiceMock.Object);

            var response = await valueSetController.GetLatestVersion(_valueSetRequestDto);

            var sampleResponse = new StatusCodeResult(204);
            Assert.IsInstanceOf<StatusCodeResult>(response);
            Assert.AreEqual(((StatusCodeResult)response).StatusCode, sampleResponse.StatusCode);
        }

        [Test]
        public async Task Returns_File_Content_Result_When_OutDated()
        {
            ValueSetResponseDto valueSetResponseDto = new ValueSetResponseDto(false, false)
            {
                ZipContents = _cacheData
            };
            var sampleResponse = new FileContentResult(_cacheData, "application/octet-stream");

            _valueSetServiceMock.Setup(x => x.GetLatestVersionAsync(_valueSetRequestDto)).ReturnsAsync(valueSetResponseDto);
            var valueSetController = new ValueSetController(_valueSetServiceMock.Object);

            var response = await valueSetController.GetLatestVersion(_valueSetRequestDto);

            Assert.IsInstanceOf<FileContentResult>(response);
            Assert.AreEqual(((FileContentResult)response).FileContents, sampleResponse.FileContents);
        }
    }
}
