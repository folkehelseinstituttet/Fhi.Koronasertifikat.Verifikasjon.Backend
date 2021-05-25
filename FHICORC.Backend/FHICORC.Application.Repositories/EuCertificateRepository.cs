using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Z.BulkOperations;

namespace FHICORC.Application.Repositories
{
    public class EuCertificateRepository : IEuCertificateRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<EuCertificateRepository> _logger;

        public EuCertificateRepository(CoronapassContext coronapassContext, ILogger<EuCertificateRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
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

        public async Task<bool> BulkUpsertEuDocSignerCertificates(List<EuDocSignerCertificate> euDocSignerCertificates)
        {
            try
            {
                var deleted = await _coronapassContext.EuDocSignerCertificates.DeleteFromQueryAsync();
                _logger.LogDebug("Deleted EuDocSignerCertificates {DeletedRows}", deleted);

                var insertedResult = new ResultInfo();
                await _coronapassContext.EuDocSignerCertificates.BulkInsertAsync(euDocSignerCertificates,
                    options =>
                    {
                        options.UseRowsAffected = true;
                        options.ResultInfo = insertedResult;
                    });

                _logger.LogDebug("Inserted EuDocSignerCertificates {InsertedRows}", insertedResult.RowsAffected);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Failed to upsert items into EuDocSignerCertificates");
                return false;
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
