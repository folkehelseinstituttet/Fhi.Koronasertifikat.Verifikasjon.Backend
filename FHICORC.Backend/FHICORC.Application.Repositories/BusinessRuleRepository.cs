using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FHICORC.Application.Repositories
{
    public class BusinessRuleRepository : IBusinessRuleRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<BusinessRuleRepository> _logger;

        public BusinessRuleRepository(CoronapassContext coronapassContext, ILogger<BusinessRuleRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
        }

        public async Task<JToken> GetAllBusinessRules()
        {
            var rules = await _coronapassContext.BusinessRules.ToListAsync();
            return MapRules(rules);
        }

        private JToken MapRules(List<BusinessRule> rules)
        {
            var mappedRules = new JArray();
            foreach (var businessRule in rules)
            {
                try
                {
                    
                    var rule = JToken.Parse(businessRule.RuleJson);
                    if (rule["id"]?.Type != JTokenType.String || (string)rule["id"] != businessRule.RuleIdentifier)
                    {
                        _logger.LogError("Failed to match id of business rule {ruleId}", businessRule.RuleIdentifier);
                        continue;
                    }

                    mappedRules.Add(rule);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to parse JSON of rule {ruleId}", businessRule.RuleIdentifier);
                }
            }

            return mappedRules;
        }
    }
}
