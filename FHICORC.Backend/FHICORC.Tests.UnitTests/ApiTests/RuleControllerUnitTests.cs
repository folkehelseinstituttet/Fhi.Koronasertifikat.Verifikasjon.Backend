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
    public class RuleControllerUnitTests
    {
        private readonly Mock<IRuleService> _ruleServiceMock = new Mock<IRuleService>();
        private RuleController _ruleController;

        [SetUp]
        public void Setup()
        {
            _ruleController = new RuleController(_ruleServiceMock.Object);
        }

        [Test]
        public async Task Returns_response_with_content_from_service_When_no_errors()
        {
            const string jsonString = "[{ \"rule\": \"exampleRule\" }]";
            var rulesDto = new RuleResponseDto {RuleListJson = jsonString};
            _ruleServiceMock.Setup(x => x.GetRulesAsync()).ReturnsAsync(rulesDto);

            var result = await _ruleController.GetRulesV3();
            var contentResult = result as ContentResult;
            
            Assert.NotNull(contentResult);
            Assert.AreEqual(jsonString, contentResult.Content);
        }
    }
}