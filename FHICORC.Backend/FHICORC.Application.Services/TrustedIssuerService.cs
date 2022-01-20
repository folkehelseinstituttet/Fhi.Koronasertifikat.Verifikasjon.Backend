using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class TrustedIssuerService : ITrustedIssuerService
    {
        private readonly ILogger<TrustedIssuerService> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly ITrustedIssuerRepository _trustedIssuerRepository;

        public static string _tree = "";

        public TrustedIssuerService(
            ILogger<TrustedIssuerService> logger,
            IMetricLogService metricLogService,
            ITrustedIssuerRepository trustedIssuerRepository) // change a repo
        {
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

        public TrustedIssuerModel GetIssuer(string iss)
        {
            try
            {
                return _trustedIssuerRepository.GetIssuer(iss);
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Issuer" + ex.Message);
                return null;
            }
        }

        public async Task AddIssuers(AddIssuersRequest issuers, bool isAddManually)
        {
            IEnumerable<TrustedIssuerModel> trustedIssuers = issuers.issuers.Select(x => new TrustedIssuerModel()
            {
                Name = x.name,
                Iss = x.issuer,
                IsAddManually = isAddManually
            });
            await _trustedIssuerRepository.AddIssuers(trustedIssuers);
        }

        public async Task ReplaceAutomaticallyAddedIssuers(ShcIssuersDto issuers)
        {
            IEnumerable<TrustedIssuerModel> trustedIssuers = issuers.ParticipatingIssuers.Select(x => new TrustedIssuerModel()
            {
                Name = x.Name,
                Iss = x.Iss,
                IsAddManually = false
            });
            await _trustedIssuerRepository.ReplaceAutomaticallyAddedIssuers(trustedIssuers);
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

        public async Task<bool> RemoveAllIssuers(bool keepIsAddManually = false)
        {
            try
            {
                var res = await _trustedIssuerRepository.CleanTable(keepIsAddManually);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove all issuers" + ex.Message);
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
