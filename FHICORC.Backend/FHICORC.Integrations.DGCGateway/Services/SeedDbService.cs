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
        protected CoronapassContext _coronapassContext;
        protected DGCGRevocationService _dgcgRevocationService;
        protected readonly IDgcgService _dgcgService;
        protected IBloomBucketService bloomBucketService;


        public SeedDbService(CoronapassContext coronapassContext, DGCGRevocationService dgcgRevocationService, IDgcgService dgcgService, IBloomBucketService bloomBucketService) { 
            _coronapassContext = coronapassContext;
            _dgcgRevocationService = dgcgRevocationService;
            _dgcgService = dgcgService;

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
            Console.WriteLine(_coronapassContext.RevocationBatch.ToQueryString());
        }

    }
}
