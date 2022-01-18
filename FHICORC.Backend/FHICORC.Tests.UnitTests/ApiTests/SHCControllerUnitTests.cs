using System.IO;
using System.Text;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Api.Controllers;
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
        private readonly Mock<ISHCService> shcService = new();
        private readonly Mock<ILogger<ShCController>> logger = new();

        private ShCController controller;

        [Test]
        public async Task GetIsTrusted_WithValidRequest_ReturnsOK()
        {
            const string jsonString = "{\"iss\": \"https://ekeys.ny.gov/epass/doh/dvc/2021\"}";
            ShcTrustResponseDto expected = new() { Name = "name", Trusted = true };
            shcService.Setup(x => x.GetIsTrustedsync(It.IsAny<ShcTrustRequestDto>()))
                .ReturnsAsync(expected);
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetIsTrusted();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.IsInstanceOf<ShcTrustResponseDto>(okResponse.Value);
            ShcTrustResponseDto responseBody = okResponse.Value as ShcTrustResponseDto;

            Assert.AreEqual(200, okResponse.StatusCode);
            Assert.NotNull(responseBody);
            Assert.AreEqual(expected.Name, responseBody.Name);
            Assert.AreEqual(expected.Trusted, responseBody.Trusted);
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
            ShcVaccineResponseDto expected = new() { Name = "name", Manufacturer = "manu"};
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
            MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
            HttpContext httpContext = new DefaultHttpContext()
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            ControllerContext controllerContext = new ControllerContext { HttpContext = httpContext };

            controller = new ShCController(shcService.Object, logger.Object) { ControllerContext = controllerContext };
        }
    }
}
