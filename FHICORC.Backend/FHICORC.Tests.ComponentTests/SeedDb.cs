using System;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;


using NUnit.Framework;

namespace FHICORC.Tests.UnitTests
{
    public static class SeedDb
    {


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
