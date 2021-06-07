using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Npgsql.Bulk;

namespace FHICORC.Application.Repositories
{
    public class EuCertificateRepository : IEuCertificateRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<EuCertificateRepository> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly NpgsqlBulkUploader _bulkUploader;

        public EuCertificateRepository(CoronapassContext coronapassContext, ILogger<EuCertificateRepository> logger, IMetricLogService metricLogService)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
            _metricLogService = metricLogService;
            _bulkUploader = new NpgsqlBulkUploader(coronapassContext);
        }

        public async Task<bool> PersistEuDocSignerCertificate(EuDocSignerCertificate euDocSignerCertificate)
        {
            try
            {
                _coronapassContext.EuDocSignerCertificates.Add(euDocSignerCertificate);
                await _coronapassContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to insert EuDocSignerCertificate into the database");
                return false;
            }
            
        }

        public async Task<bool> CleanupAndPersistEuDocSignerCertificates(List<EuDocSignerCertificate> euDocSignerCertificates)
        {
            await using (var transaction = await _coronapassContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var euCertTableName = _coronapassContext.Model.FindEntityType(typeof(EuDocSignerCertificate)).GetTableName();
                    var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"delete from \"{euCertTableName}\"");

                    _metricLogService.AddMetric("EUCertificate_DeletedEUDocs", rowsAffected);
                    _logger.LogDebug("Deleted EuDocSignerCertificates {DeletedRows}", rowsAffected);

                    await _bulkUploader.InsertAsync<EuDocSignerCertificate>(euDocSignerCertificates, InsertConflictAction.DoNothing());

                    _logger.LogDebug("Inserted EuDocSignerCertificates {InsertedRows}", euDocSignerCertificates.Count);
                    _metricLogService.AddMetric("EUCertificate_InsertedEUDocs", euDocSignerCertificates.Count);

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Failed to upsert items into EuDocSignerCertificates");
                    return false;
                }
            }
        }


        public async Task<List<EuDocSignerCertificate>> GetAllEuDocSignerCertificates()
        {
            try
            {
                return await _coronapassContext.EuDocSignerCertificates.ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Failed to retrieve EuDocSignerCertificates");
                return new List<EuDocSignerCertificate>(0);
            }
        }
    }
}
