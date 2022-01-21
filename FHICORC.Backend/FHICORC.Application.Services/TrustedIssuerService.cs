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
