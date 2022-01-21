using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class VaccineCodesService : IVaccineCodesService
    {
        private readonly ILogger<TrustedIssuerService> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly IVaccineCodesRepository _vaccineCodesRepository;


        public VaccineCodesService(
            ILogger<TrustedIssuerService> logger,
            IMetricLogService metricLogService,
            IVaccineCodesRepository vaccineCodesRepository) 
        {
            _logger = logger;
            _metricLogService = metricLogService;
            _vaccineCodesRepository = vaccineCodesRepository;
        }

        public async Task<ShcVaccineResponseDto> GetVaccinationInfoSync(ShcCodeRequestDto shcRequestList)
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
        }

        public async Task<VaccineCodesModel> GetVaccinationInfo(ShcCodeRequestDto shcRequestList)
        {
            try
            {
                foreach (var shcRequest in shcRequestList.Codes)
                {
                     VaccineCodesModel ret = await _vaccineCodesRepository.GetVaccInfo(new VaccineCodeKey() { VaccineCode = shcRequest.Code, CodingSystem = shcRequest.System });

                     if (ret != null)
                         return ret;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Issuer" + ex.Message);
                return null;
            }
        }

        public async Task UpdateVaccineCodesList(List<VaccineCodesModel> vaccineCodesList)
        {
            await _vaccineCodesRepository.UpdatevaccineCodesList(vaccineCodesList);
        }

        public async Task<bool> RemoveAllVaccineCodes(bool onlyAuto = false)
        {
            try
            {
                var res = await _vaccineCodesRepository.CleanTable(onlyAuto); ;
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove all codes" + ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey)
        {
            try
            {
                var res = await _vaccineCodesRepository.RemoveVaccineCode(vaccineCodeKey);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove Issuer" + ex.Message);
                return false;
            }
        }

        public async Task AddVaccineCode(VaccineCodesDto vaccineCodesModel , bool addedManually)
        {
            IEnumerable<VaccineCodesModel> vaccineCodesModels = vaccineCodesModel.Codes.Select(x => new VaccineCodesModel()
            {
                VaccineCode = x.Code,
                CodingSystem = x.System,
                Name = x.Name,
                Manufacturer = x.Manufacturer,
                Target = x.Target,
                Type = x.Type,
                IsAddManually = addedManually
            });
            await _vaccineCodesRepository.AddVaccineCode(vaccineCodesModels);
        }
    }
}
