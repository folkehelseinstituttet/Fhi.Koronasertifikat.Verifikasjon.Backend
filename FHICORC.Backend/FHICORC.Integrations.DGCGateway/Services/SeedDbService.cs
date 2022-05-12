using System;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Text;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;


namespace FHICORC.Integrations.DGCGateway.Services { 
    public class SeedDbService : ISeedDbService
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly DGCGRevocationService _dgcgRevocationService;

        public SeedDbService(CoronapassContext coronapassContext) { 
            _coronapassContext = coronapassContext;
            //_dgcgRevocationService = dgcgRevocationService;

            SeedDatabase();


        }

        public void SeedDatabase()
        {

            var revocationBatchList = JsonConvert.DeserializeObject<DgcgRevocationBatchListRespondDto>(File.ReadAllText("TestFiles/tst_revocation_batch_list.json"));

            foreach (var rb in revocationBatchList.Batches)
            {
                var response = File.ReadAllText("TestFiles/BatchHashes/" + rb.BatchId + ".txt");

                try
                {
                    var encodedMessage = Convert.FromBase64String(response);

                    var signedCms = new SignedCms();
                    signedCms.Decode(encodedMessage);
                    signedCms.CheckSignature(true);

                    var decodedMessage = Encoding.UTF8.GetString(signedCms.ContentInfo.Content);
                    var parsedResponse = JsonConvert.DeserializeObject<DGCGRevocationBatchRespondDto>(decodedMessage);

                    _dgcgRevocationService.AddToDatabase(rb, parsedResponse);


                }
                catch (Exception e) { }
            }

            _dgcgRevocationService.OrganizeBatches();

        }

        public void GetInfoAboutDb() {
            //Console.WriteLine(_coronapassContext.RevocationBatch.ToQueryString());
        }

    }
}
