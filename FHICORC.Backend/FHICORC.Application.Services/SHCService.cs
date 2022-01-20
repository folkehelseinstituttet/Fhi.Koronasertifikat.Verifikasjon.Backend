using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class SHCService : ISHCService
    {
        private const string CacheKey = "shcCacheKey";

        private readonly ICacheManager _cacheManager;
        private readonly ShcCacheOptions _shcCacheOptions;
        private readonly ILogger<SHCService> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly ITrustedIssuerRepository _trustedIssuerRepository;

        public static string _tree = "";

        public SHCService(
            ICacheManager cacheManager,
            ShcCacheOptions shcCacheOptions,
            ILogger<SHCService> logger,
            IMetricLogService metricLogService,
            ITrustedIssuerRepository trustedIssuerRepository) // change a repo
        {
            _cacheManager = cacheManager;
            _shcCacheOptions = shcCacheOptions;
            _logger = logger;
            _metricLogService = metricLogService;
            _trustedIssuerRepository = trustedIssuerRepository;
        }

        public async Task<ShcVaccineResponseDto> GetVaccinationInfosync(ShcCodeRequestDto shcRequestList)   // TODO
        {
            foreach (var shcRequest in shcRequestList.Codes)
            {
                switch (shcRequest.System)
                {
                    case CodingSystem.Cvx:
                        if (shcRequest.Code == "207")
                            return new ShcVaccineResponseDto()
                            {
                                //EUCode = "EU/1/20/1507",
                                Name = "Moderna",
                                Manufacturer = "Moderna US, Inc.",
                                Type = "SARS CoV-2 mRNA Vaccine",
                                Target = "Sars-CoV-2"
                            };
                        if (shcRequest.Code == "212")
                            return new ShcVaccineResponseDto()
                            {
                                //EUCode = "EU/1/20/1525",
                                Name = "Jannsen",
                                Manufacturer = "Janssen Products, LP",
                                Type = "SARS CoV-2 Vector Vaccine",
                                Target = "Sars-CoV-2"
                            };
                        if (shcRequest.Code == "208" || shcRequest.Code == "217" || shcRequest.Code == "218" || shcRequest.Code == "219")
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
                        break;
                    case CodingSystem.Atc:
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
                        break;
                }
            }

            return new ShcVaccineResponseDto()
            {
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
                Rootobject vciList = JsonSerializer.Deserialize<Rootobject>(File.ReadAllText(@"./TestExamples/vci.json"));

                var result = vciList.participating_issuers.Single(s => s.iss == shcRequestDeserialized.iss);

                return new ShcTrustResponseDto()
                {
                    Trusted = true,
                    Name = result.name
                };
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError(e, "File vci not found");
                
                //PrintFolder(".");
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
        public async Task<string> AddIssuer(AddIssuersRequest iss)
        {
            string res = string.Empty;
            try
            {
                foreach (var i in iss.issuers)
                {
                    res = await _trustedIssuerRepository.AddIssuer(new TrustedIssuerModel()
                        { Iss = i.issuer, Name = i.name, IsAddManually = true });
                }

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddIssuer" + ex.Message);
                return ex.Message;
            }
        }
        public async Task<bool> CleanTable(bool cleanOnlyAuto = true)
        {
            try
            {
                var res = await _trustedIssuerRepository.CleanTable();
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Clean Table" + ex.Message);
                return false;
            }
        }

        public async Task<TrustedIssuerModel> GetIssuer(string iss)
        {
            try
            {
                var res = await _trustedIssuerRepository.GetIssuer(iss);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Issuer" + ex.Message);
                return null;
            }
        }
        public async Task<bool> RemoveIssuer(string iss)
        {
            try
            {
                var res = await _trustedIssuerRepository.RemoveIssuer(iss);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove Issuer" + ex.Message);
                return false;
            }
        }

        public static string PrintFolder(string sDir)
        {
            _tree = "";
            PrintFolderHelper(sDir);
            return _tree;
        }
        public static void PrintFolderHelper(string sDir)
        {
            try
            {
                _tree += sDir + Environment.NewLine;

                foreach (string f in Directory.GetFiles(sDir))
                {
                    _tree += f + Environment.NewLine;
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    PrintFolderHelper(d);
                }
            }
            catch (Exception ex)
            {
                _tree += ex.Message + ex.StackTrace;
            }
        }
        
    }
}
