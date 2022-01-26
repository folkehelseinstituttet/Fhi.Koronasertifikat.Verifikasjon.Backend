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
        private readonly Mock<ITrustedIssuerService> trustedIssuerService = new Mock<ITrustedIssuerService>();
        private readonly Mock<IVaccineCodesService> vaccineCodesService = new Mock<IVaccineCodesService>();
        private readonly Mock<ILogger<SHCController>> logger = new Mock<ILogger<SHCController>>();

        private SHCController controller;

        #region TrustedIssuer
        [Test]
        public async Task GetIsTrusted_WithKnownIss_ReturnsTrusted()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"" + iss + "\"}";
            TrustedIssuerModel expected = new TrustedIssuerModel() { Name = "name", Iss = "iss", IsTrusted = true };
            trustedIssuerService.Setup(x => x.GetIssuer(iss))
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
            trustedIssuerService.Setup(x => x.GetIssuer(iss))
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
        public async Task AddIssuer_ValidJson_ReturnsCreated()
        {
            const string jsonString = "{\"issuers\": []}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.AddIssuer();

            Assert.IsInstanceOf<StatusCodeResult>(response);
            StatusCodeResult statusCode = response as StatusCodeResult;
            Assert.AreEqual(201, statusCode.StatusCode);
        }

        [Test]
        public async Task AddIssuer_InvalidJson_ReturnsBadRequest()
        {
            const string jsonString = "{\"issuers\": 123}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.AddIssuer();

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
            BadRequestObjectResult statusCode = response as BadRequestObjectResult;
            Assert.AreEqual(400, statusCode.StatusCode);
        }

        [Test]
        public async Task TrustIssuer_CallsServiceWithTrustedTrue()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"" + iss + "\"}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.TrustIssuer();

            trustedIssuerService.Verify(x => x.UpdateIsTrusted(iss, true), Times.Once);
        }

        [Test]
        public async Task DistrustIssuer_CallsServiceWithTrustedFalse()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"" + iss + "\"}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.DistrustIssuer();

            trustedIssuerService.Verify(x => x.UpdateIsTrusted(iss, false), Times.Once);
        }


        [Test]
        public async Task CleanTableTrustedIssuer_CallsService()
        {
            CreateControllerWithRequest("{}");

            IActionResult response = await controller.CleanTableTrustedIssuer();

            Assert.IsInstanceOf<OkResult>(response);
            OkResult statusCode = response as OkResult;
            Assert.AreEqual(200, statusCode.StatusCode);
            trustedIssuerService.Verify(x => x.RemoveAllIssuers(It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task RemoveIssuer_CallsServiceWithIss_ServiceReturnsFalse_GivesNotFound()
        {
            const string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            const string jsonString = "{\"iss\": \"" + iss + "\"}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.RemoveIssuer();

            Assert.IsInstanceOf<NotFoundObjectResult>(response);
            NotFoundObjectResult statusCode = response as NotFoundObjectResult;
            Assert.AreEqual(404, statusCode.StatusCode);
            trustedIssuerService.Verify(x => x.RemoveIssuer(iss), Times.Once);
        }
        #endregion

        #region Vaccine Codes
        [Test]
        public async Task GetVaccineInfo_WithValidRequest_ReturnsOK()
        {
            const string jsonString = "{\"codes\": []}";
            VaccineCodesModel expected = new() { Name = "name", Manufacturer = "manu" };
            vaccineCodesService.Setup(x => x.GetVaccinationInfo(It.IsAny<ShcCodeRequestDto>()))
                .Returns(Task.FromResult(expected));
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.GetVaccineInfo();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.IsInstanceOf<VaccineCodesModel>(okResponse.Value);
            VaccineCodesModel responseBody = okResponse.Value as VaccineCodesModel;

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

        [Test]
        public async Task AddVaccineCode_ValidJson_ReturnsCreated()
        {
            const string jsonString = "{\"codes\": []}";
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.AddVaccineCode();

            Assert.IsInstanceOf<ObjectResult>(response);
            ObjectResult statusCode = response as ObjectResult;
            Assert.AreEqual(201, statusCode.StatusCode);
        }

        [Test]
        public async Task CleanTableVaccineCodes_CallsService()
        {
            CreateControllerWithRequest("{}");

            IActionResult response = await controller.CleanTableVaccineCodes();

            Assert.IsInstanceOf<OkResult>(response);
            OkResult statusCode = response as OkResult;
            Assert.AreEqual(200, statusCode.StatusCode);
            vaccineCodesService.Verify(x => x.RemoveAllVaccineCodes(It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task RemoveVaccineCode_WithKnownId_ReturnsOK()
        {
            const string jsonString = "{\"code\": \"code\"}";
            vaccineCodesService.Setup(x => x.RemoveVaccineCode(It.IsAny<VaccineCodeKey>()))
                .Returns(Task.FromResult(true));
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.RemoveVaccineCode();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult okResponse = response as OkObjectResult;
            Assert.AreEqual(200, okResponse.StatusCode);
        }

        [Test]
        public async Task RemoveVaccineCode_WithUnknownId_ReturnsNotFound()
        {
            const string jsonString = "{\"code\": \"code\"}";
            vaccineCodesService.Setup(x => x.RemoveVaccineCode(It.IsAny<VaccineCodeKey>()))
                .Returns(Task.FromResult(false));
            CreateControllerWithRequest(jsonString);

            IActionResult response = await controller.RemoveVaccineCode();

            Assert.IsInstanceOf<NotFoundObjectResult>(response);
            NotFoundObjectResult notFound = response as NotFoundObjectResult;
            Assert.AreEqual(404, notFound.StatusCode);
        }
        #endregion

        #region Other
        [Test]
        public void DiagnosticPrintDir_ReturnsHttpOk()
        {
            CreateControllerWithRequest("{}");

            IActionResult response = controller.DiagnosticPrintDir();

            Assert.IsInstanceOf<OkObjectResult>(response);
            OkObjectResult notFound = response as OkObjectResult;
            Assert.AreEqual(200, notFound.StatusCode);
        }
        #endregion

        private void CreateControllerWithRequest(string json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            HttpContext httpContext = new DefaultHttpContext()
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            ControllerContext controllerContext = new ControllerContext { HttpContext = httpContext };

            controller = new SHCController(
                trustedIssuerService.Object,
                vaccineCodesService.Object,
                logger.Object) { ControllerContext = controllerContext };
        }
    }
}
