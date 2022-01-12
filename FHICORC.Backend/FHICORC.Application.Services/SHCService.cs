using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace FHICORC.Application.Services
{
    public class SHCService : ISHCService
    {
        private const string CacheKey = "shcCacheKey";

        private readonly ICacheManager _cacheManager;
        private readonly ShcCacheOptions _shcCacheOptions;
        private readonly ILogger<SHCService> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly IBusinessRuleRepository _businessRuleRepository;

        private static string _tree = "";

        public SHCService(ICacheManager cacheManager, ShcCacheOptions shcCacheOptions,ILogger<SHCService> logger,
            IMetricLogService metricLogService, IBusinessRuleRepository businessRuleRepository) // change a repo
        {
            _cacheManager = cacheManager;
            _shcCacheOptions = shcCacheOptions;
            _logger = logger;
            _metricLogService = metricLogService;
            _businessRuleRepository = businessRuleRepository;
        }

        public async Task<ShcVaccineResponseDto> GetVaccinationInfosync(ShcRequestDto shcRequestList)   // TO DO
        {
            foreach (var shcRequest in shcRequestList.Codes)
            {
                if (shcRequest.System == CodingSystem.Cvx)
                {
                    if (shcRequest.Code == "207" )
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/20/1507",
                            Name = "Moderna",
                            Manufacturer = "Moderna US, Inc.",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                };
                    if (shcRequest.Code == "212" )
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/20/1525",
                            Name = "Jannsen",
                            Manufacturer = "Janssen Products, LP",
                            Type = "SARS CoV-2 Vector Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "208" || shcRequest.Code == "217" || shcRequest.Code == "218" || shcRequest.Code == "219" )
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/21/1528",
                            Name = "Pfizer",
                            Manufacturer = "Pfizer-BioNTech",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "210")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/21/1529",
                            Name = "Astra Zeneca",
                            Manufacturer = "AstraZeneca Pharmaceuticals LP",
                            Type = "SARS CoV-2 Vector Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "211")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "NVX-CoV2373",
                            Name = "Novavax",
                            Manufacturer = "Novavax, Inc.",
                            Type = "SARS CoV-2 Protein subunit vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "510")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "BBIBP-CorV",
                            Name = "BIBP-Sinopharm",
                            Manufacturer = "Sinopharm-Biotech",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "511")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "CoronaVac",
                            Name = "CoronaVac",
                            Manufacturer = "Sinovac",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "502")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "Covaxin",
                            Name = "Covaxin",
                            Manufacturer = "Bharat Biotech International Limited",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                        };
                    if (shcRequest.Code == "503")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "CoviVac",
                            Name = "Covivac",
                            Manufacturer = "Covivac",
                            Type = "SARS CoV-2 mRNA Vaccine",
                            Target = "Sars-CoV-2"
                        };
                }
                if (shcRequest.System == CodingSystem.Atc)
                {
                    if (shcRequest.Code == "207")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/20/1507",
                            Name = "Moderna",
                            Manufacturer = "Moderna US, Inc.",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "212")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/20/1525",
                            Name = "Jannsen",
                            Manufacturer = "Janssen Products, LP",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "208" || shcRequest.Code == "217" || shcRequest.Code == "218" || shcRequest.Code == "219")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/21/1528",
                            Name = "Pfizer",
                            Manufacturer = "Pfizer-BioNTech",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "210")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "EU/1/21/1529",
                            Name = "Astra Zeneca",
                            Manufacturer = "AstraZeneca Pharmaceuticals LP",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "211")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "NVX-CoV2373",
                            Name = "Novavax",
                            Manufacturer = "Novavax, Inc.",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "510")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "BBIBP-CorV",
                            Name = "BIBP-Sinopharm",
                            Manufacturer = "Sinopharm-Biotech",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "511")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "CoronaVac",
                            Name = "CoronaVac",
                            Manufacturer = "Sinovac",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "502")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "Covaxin",
                            Name = "Covaxin",
                            Manufacturer = "Bharat Biotech International Limited",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                    if (shcRequest.Code == "503")
                        return new ShcVaccineResponseDto()
                        {
                            //EUCode = "CoviVac",
                            Name = "Covivac",
                            Manufacturer = "Covivac",
                            Type = "SARS CoV-2 mRNA Vaccine"
                        };
                }
            }

            return new ShcVaccineResponseDto()
            {
                //EUCode = "",
                Name = "Unknown"
            };

            //if (_cacheManager.TryGetValue(CacheKey, out ShcVaccineResponseDto cachedData))
            //{
            //    _logger.LogDebug("Vaccination code found in cache");
            //    _metricLogService.AddMetric("Vaccination code_CacheHit", true);
            //    return cachedData;
            //}
            //_logger.LogDebug("Vaccination code not found in cache. Fetching from repository");
            //_metricLogService.AddMetric("Vaccination code_CacheHit", false);

            //var databaseResults = await _businessRuleRepository.GetAllBusinessRules();  // TO FIX

            //var ruleResponseDto = new ShcVaccineResponseDto() {VaccineCode = databaseResults.ToString()};
            //_cacheManager.Set(CacheKey, ruleResponseDto, _ruleCacheOptions);

            //return ruleResponseDto;
        }

        public async Task<ShcTrustResponseDto> GetIsTrustedsync(ShcTrustRequestDto shcRequestDeserialized)
        {
            try
            {
                Rootobject vciList = (Rootobject)JsonSerializer.Deserialize<Rootobject>(System.IO.File.ReadAllText(@".\TestExamples\vci2.json"));
           
                var result = vciList.participating_issuers.Single(s => s.iss == shcRequestDeserialized.iss);

                return new ShcTrustResponseDto()
                {
                    Trusted = true,
                    //Canonical_iss = result.canonical_iss,
                    Name = result.name
                };
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError(e, "File vci not found");
                _tree = "";
                PrintFolder(".");
                return new ShcTrustResponseDto()
                {
                    Trusted = false,
                    Name = "File vci not found \n" + _tree
                };
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Specified iss name not found");

                return new ShcTrustResponseDto()
                {
                    Trusted = false,
                    //Canonical_iss = result.canonical_iss,
                    Name = "Specified iss name not found"
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Specified iss name not found");
                
                return new ShcTrustResponseDto()
                {
                    Trusted = false,
                    //Canonical_iss = result.canonical_iss,
                    Name = "Specified iss name not found"
                };
            }
            
        }

       

    

        static void PrintFolder(string sDir)
        {
            try
            {
                _tree += "    Search...  " + sDir + "      ";

                foreach (string f in Directory.GetFiles(sDir))
                {
                    _tree += f + "  ||   ";
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    PrintFolder(d);
                }
            }
            catch (Exception ex)
            {
                _tree += ex.Message + ex.StackTrace;
            }
        }
        
    }
}