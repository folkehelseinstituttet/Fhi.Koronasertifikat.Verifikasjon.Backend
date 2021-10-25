using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FHICORC.Application.Repositories;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Repositories
{
    [Category("Unit")]
    public class BusinessRuleRepositoryTests
    {
        private readonly ILogger<BusinessRuleRepository> _nullLogger = new NullLoggerFactory().CreateLogger<BusinessRuleRepository>();
        private readonly Mock<CoronapassContext> _coronapassContextMock = new Mock<CoronapassContext>(new DbContextOptions<CoronapassContext>());
        private readonly Mock<DbSet<BusinessRule>> _dbSetMock = new Mock<DbSet<BusinessRule>>();
        
        private BusinessRuleRepository _businessRuleRepository;

        [SetUp]
        public void SetUp()
        {
            _businessRuleRepository = new BusinessRuleRepository(_coronapassContextMock.Object, _nullLogger);
        }
        
        [Test]
        public async Task GetAllBusinessRules_with_ruleId_that_is_not_string_will_skip_rule()
        {
            var data = new List<BusinessRule>
            {
                new BusinessRule()
                {
                    BusinessRuleId = 1,
                    RuleIdentifier = "10",
                    RuleJson = "{ \"rule\": \"exampleRule\", \"Identifier\": 10 }",
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                }
            }.AsQueryable();
            
            SetUpMocks(data);

            var response = await _businessRuleRepository.GetAllBusinessRules();

            var expectedEmptyJArray = new JArray();
            
            Assert.AreEqual(expectedEmptyJArray, response);
        }
        
        [Test]
        public async Task GetAllBusinessRules_with_ruleId_that_does_not_match_RuleIdentifier_will_skip_rule()
        {
            var data = new List<BusinessRule>
            {
                new()
                {
                    BusinessRuleId = 1,
                    RuleIdentifier = "RuleId",
                    RuleJson = "{ \"rule\": \"exampleRule\", \"Identifier\": \"NoMatch\" }",
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                }
            }.AsQueryable();
            
            SetUpMocks(data);

            var response = await _businessRuleRepository.GetAllBusinessRules();

            var expectedEmptyJArray = new JArray();
            
            Assert.AreEqual(expectedEmptyJArray, response);
        }
        
        [Test]
        public async Task GetAllBusinessRules_with_RuleJson_that_is_not_json_will_skip_rule()
        {
            var data = new List<BusinessRule>
            {
                new()
                {
                    BusinessRuleId = 1,
                    RuleIdentifier = "NotJsonRule",
                    RuleJson = "not json {1222%6",
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                }
            }.AsQueryable();
            
            SetUpMocks(data);

            var response = await _businessRuleRepository.GetAllBusinessRules();

            var expectedEmptyJArray = new JArray();
            
            Assert.AreEqual(expectedEmptyJArray, response);
        }
        
        [Test]
        public async Task GetAllBusinessRules_with_Id_in_RuleJson_matching_RuleIdentifier_will_return_rule()
        {
            const string jsonRule = "{ \"rule\": \"exampleRule\", \"Identifier\": \"RuleIdMatch\" }";
            var data = new List<BusinessRule>
            {
                new()
                {
                    BusinessRuleId = 1,
                    RuleIdentifier = "RuleIdMatch",
                    RuleJson = jsonRule,
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                }
            }.AsQueryable();
            
            SetUpMocks(data);

            var response = await _businessRuleRepository.GetAllBusinessRules();

            var expectedJArray = new JArray { JToken.Parse(jsonRule) };

            Assert.AreEqual(expectedJArray, response);
        }

        [Test]
        public async Task GetAllBusinessRules_will_return_list_of_validated_rules()
        {
            const string matchingRule1 = "{ \"rule\": \"exampleRule\", \"Identifier\": \"MatchingRule1\" }";
            const string matchingRule2 = "{ \"rule\": \"testRule\", \"Identifier\": \"MatchingRule2\" }";
            const string matchingRule3 = "{ \"rule\": \"anotherRule\", \"Identifier\": \"MatchingRule3\" }";
            var data = new List<BusinessRule>
            {
                new()
                {
                    BusinessRuleId = 1,
                    RuleIdentifier = "MatchingRule1",
                    RuleJson = matchingRule1,
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                },
                new()
                {
                    BusinessRuleId = 2,
                    RuleIdentifier = "10",
                    RuleJson = "{ \"rule\": \"exampleRule\", \"Identifier\": 10 }",
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(2))
                },
                new()
                {
                    BusinessRuleId = 3,
                    RuleIdentifier = "MatchingRule2",
                    RuleJson = matchingRule2,
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(7))
                },
                new()
                {
                    BusinessRuleId = 4,
                    RuleIdentifier = "MatchingRule3",
                    RuleJson = matchingRule3,
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(7))
                },
                new()
                {
                    BusinessRuleId = 5,
                    RuleIdentifier = "noMatchId", 
                    RuleJson = "{ \"rule\": \"exampleRule\", \"Identifier\": \"993883\" }",
                    Created = DateTime.MinValue.Add(TimeSpan.FromHours(10))
                }
            }.AsQueryable();
            
            SetUpMocks(data);

            var response = await _businessRuleRepository.GetAllBusinessRules();
            
            var expectedJArray = new JArray { JToken.Parse(matchingRule1), JToken.Parse(matchingRule2), JToken.Parse(matchingRule3) };

            Assert.AreEqual(expectedJArray, response);
        }

        private void SetUpMocks(IQueryable<BusinessRule> data)
        {
            _coronapassContextMock.Reset();
            _dbSetMock.Reset();
            
            _dbSetMock.As<IAsyncEnumerable<BusinessRule>>()
                .Setup(m => m.GetAsyncEnumerator(new CancellationToken()))
                .Returns(new TestAsyncEnumerator<BusinessRule>(data.GetEnumerator()));
            _dbSetMock.As<IQueryable<BusinessRule>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<BusinessRule>(data.Provider));
            _dbSetMock.As<IQueryable<BusinessRule>>().Setup(m => m.Expression).Returns(data.Expression);
            _dbSetMock.As<IQueryable<BusinessRule>>().Setup(m => m.ElementType).Returns(data.ElementType);
            _dbSetMock.As<IQueryable<BusinessRule>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _coronapassContextMock.Object.BusinessRules = _dbSetMock.Object;
        }
    }
}