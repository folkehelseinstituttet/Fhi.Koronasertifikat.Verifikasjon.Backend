using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Repositories
{
    public class CountriesReportRepository : ICountriesReportRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<CountriesReportRepository> _logger;

        public CountriesReportRepository(CoronapassContext coronapassContext, ILogger<CountriesReportRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
        }

        public async Task<bool> Save(CountriesReportModel countriesReportModel)
        {
            try
            {
                await CleanCountriesReportTable();
                await Add(countriesReportModel);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to insert Coutries Report into the database");
                return false;
            }

        }
        private async Task<bool> CleanCountriesReportTable()
        {
            await using (var transaction = await _coronapassContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var countriesReportTableName = _coronapassContext.Model.FindEntityType(typeof(CountriesReportModel)).GetTableName();
                    var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"delete from \"{countriesReportTableName}\"");

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Failed to upset items into CountriesReportModel");
                    return false;
                }
            }
        }
        private async Task<bool> Add(CountriesReportModel countriesReportModel)
        {
            await using (var transaction = await _coronapassContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var countriesReportTableName = _coronapassContext.Model.FindEntityType(typeof(CountriesReportModel)).GetTableName();
                    var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"insert into \"{countriesReportTableName}\" values (DEFAULT, '{countriesReportModel.CountriesReport}')");

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Failed insert statement CountriesReportModel");
                    return false;
                }
            }
        }

        public async Task<IList<string>> GetAllCountries()
        {
            var certificates = await _coronapassContext.EuDocSignerCertificates.ToListAsync();
            var countriesHist = await _coronapassContext.CountriesReportModels.ToListAsync();

            var helper = new ReportCountriesHelper(certificates, countriesHist);

            var res = Save(new CountriesReportModel()
            {
                CountriesReport = helper.AllCountriesList
            });

            return helper.GetReport();
        }

        private class ReportCountriesHelper
        {
            public ReportCountriesHelper(List<EuDocSignerCertificate> certificates, List<CountriesReportModel> countriesHist)
            {
                _certificates = certificates;
                _countriesHist = countriesHist;
                SetAllCountriesList();
            }

            private List<EuDocSignerCertificate> _certificates;
            private List<CountriesReportModel> _countriesHist;
            private List<string> _allCountriesList = new List<string>();
            private List<string> _allCountriesListHist = new List<string>();
            private List<string> _addedCountriesList = new List<string>();
            private List<string> _removedCountriesList = new List<string>();

            public string AllCountriesList
            {
                get
                {
                    if (_allCountriesList.Count > 0)
                    {
                        return string.Join(",", _allCountriesList);
                    }
                    return "None.";
                }
            }
            public string AddCountriesList
            {
                get
                {
                    if (_addedCountriesList.Count > 0)
                    {
                        return string.Join(",", _addedCountriesList);
                    }
                    return "None.";
                }
            }
            public string RemovedCountriesList
            {
                get
                {
                    if (_removedCountriesList.Count > 0)
                    {
                        return string.Join(",", _removedCountriesList);
                    }
                    return "None.";
                }
            }

            public void SetAllCountriesList()
            {

                foreach (var certificate in _certificates)
                {
                    _allCountriesList.Add(certificate.Country);
                }
                _allCountriesList = _allCountriesList.Distinct().ToList();

               
                foreach (var countries in _countriesHist)
                {
                    _allCountriesListHist = countries.CountriesReport.Split(',').ToList();
                    break;
                }

                _addedCountriesList = _allCountriesList.Except(_allCountriesListHist).ToList();
                _removedCountriesList = _allCountriesListHist.Except(_allCountriesList).ToList();
            }

            public List<string> GetReport()
            {
                List<string> retList = new List<string>();

                retList.Add(AllCountriesList);
                retList.Add(AddCountriesList);
                retList.Add(RemovedCountriesList);

                return retList;
            }
        }
    }
}
