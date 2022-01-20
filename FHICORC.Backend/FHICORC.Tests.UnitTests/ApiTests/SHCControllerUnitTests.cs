using System.IO;
using System.Text;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Api.Controllers;
using FHICORC.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.ApiTests
{
    [Category("Unit")]
    public class SHCControllerUnitTests
    {
        private readonly Mock<ITrustedIssuerService> shcService = new Mock<ITrustedIssuerService>();
        private readonly Mock<ILogger<ShCController>> logger = new Mock<ILogger<ShCController>>();

        private ShCController controller;

        [Test]
        public async Task GetIsTrusted_WithKnownIss_ReturnsTrusted()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"" + iss + "\"}";
            TrustedIssuerModel expected = new TrustedIssuerModel() { Name = "name", Iss = "iss" };
            shcService.Setup(x => x.GetIssuer(iss))
                .Returns(expected);
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetIsTrusted();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.IsInstanceOf<ShcTrustResponseDto>(okResponse.Value);
            ShcTrustResponseDto responseBody = okResponse.Value as ShcTrustResponseDto;

            Assert.AreEqual(200, okResponse.StatusCode);
            Assert.NotNull(responseBody);
            Assert.AreEqual(expected.Name, responseBody.Name);
            Assert.True(responseBody.Trusted);
        }

        [Test]
        public async Task GetIsTrusted_WithUnknownIss_ReturnsNotTrusted()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"bad iss\"}";
            TrustedIssuerModel responseModel = new TrustedIssuerModel() { Name = "name", Iss = "iss" };
            shcService.Setup(x => x.GetIssuer(iss))
                .Returns(responseModel);
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetIsTrusted();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.IsInstanceOf<ShcTrustResponseDto>(okResponse.Value);
            ShcTrustResponseDto responseBody = okResponse.Value as ShcTrustResponseDto;

            Assert.AreEqual(200, okResponse.StatusCode);
            Assert.NotNull(responseBody);
            Assert.Null(responseBody.Name);
            Assert.False(responseBody.Trusted);
        }

        [Test]
        public async Task GetIsTrusted_WithInvalidRequestParameterType_ReturnsBadRequest()
        {
            const string jsonString = "{\"iss\": 123}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetIsTrusted();

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task GetVaccineInfo_WithValidRequest_ReturnsOK()
        {
            const string jsonString = "{\"codes\": []}";
            ShcVaccineResponseDto expected = new ShcVaccineResponseDto() { Name = "name", Manufacturer = "manu"};
            shcService.Setup(x => x.GetVaccinationInfosync(It.IsAny<ShcCodeRequestDto>()))
                .ReturnsAsync(expected);
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetVaccineInfo();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.IsInstanceOf<ShcVaccineResponseDto>(okResponse.Value);
            ShcVaccineResponseDto responseBody = okResponse.Value as ShcVaccineResponseDto;

            Assert.AreEqual(200, okResponse.StatusCode);
            Assert.NotNull(responseBody);
            Assert.AreEqual(expected.Name, responseBody.Name);
            Assert.AreEqual(expected.Manufacturer, responseBody.Manufacturer);
        }

        [Test]
        public async Task GetVaccineInfo_WithInvalidRequestParameterType_ReturnsBadRequest()
        {
            const string jsonString = "{\"codes\": \"123\"}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetVaccineInfo();

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        private void CreateControllerWithRequest(string json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            HttpContext httpContext = new DefaultHttpContext()
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            ControllerContext controllerContext = new ControllerContext { HttpContext = httpContext };

            controller = new ShCController(shcService.Object, logger.Object) { ControllerContext = controllerContext };
        }
    }
}
