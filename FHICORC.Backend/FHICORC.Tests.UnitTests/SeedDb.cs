using System;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;


using NUnit.Framework;

namespace FHICORC.Tests.UnitTests
{
    public static class SeedDb
    {

        //public static void AddOrganisasjoner(IkkeDigitaleInnbyggereContext dbContext)
        //{

        //    var org1 = new Domene.Organisasjon()
        //    {
        //        Opprettettidspunkt = DateTime.UtcNow.AddDays(-2),
        //        Navn = "FHI",
        //        VaksineregisteringFhn = true,
        //        OpprettetAvId = 2,
        //        Aktiv = true,
        //        OrgNr = "123",
        //        OrganisasjonAdmin = true,
        //        KoronasertifikatFhn = true,
        //        KoronasertifikatFnrDnr = true,
        //        Helfo = false,
        //        EUImmun = true,
        //        EUTest = true,
        //        EUVaksine = true,
        //        NorskSertifikat = true,
        //        OrganisasjonsVilkarGodtattTidspunkt = DateTime.UtcNow
        //    };
        //    dbContext.Organisasjon.Add(org1);
        //    dbContext.SaveChanges();

        //}



        public static CoronapassContext GetInMemoryContext()
        {
            var dbNavn = TestContext.CurrentContext.Test.Name;
            var options = new DbContextOptionsBuilder<CoronapassContext>()
                .UseInMemoryDatabase(databaseName: dbNavn)
                .Options;

            return new CoronapassContext(options);
        }
    }
}
