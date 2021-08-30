using Microsoft.AspNetCore.Mvc;
using FHICORC.Application.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using FHICORC.Application.Models;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class PublicKeyController : ControllerBase
    {
        private readonly IPublicKeyService _publicKeyService;
        public PublicKeyController(IPublicKeyService publicKeyService)
        {
            _publicKeyService = publicKeyService;
        }

        [HttpGet]
        [MapToApiVersion("1")]
        public async Task<IActionResult> GetPublicKey()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList.Concat(new []
            {
                new CertificatePublicKey
                {
                    kid = "cHJpdmF0ZWtleQ==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEqErhaHuVTpPQN3jU6ZO5zv/8sIXYIOh3hw87tTYlwVH8Tcov/vizbk2MZU8+gzmJmRcy9BXiTlH7UKesnMtvDg=="
                },
                new CertificatePublicKey
                {
                    kid = "ZGV2a2V5Mg==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEXKME6H60XwR5wCOvD63veSarJDE6DVDyrhak8kHpqr7koAtd3eObk14Sn8k93F9AvQ3fMOh1GoetK4iD6vf7iA=="
                },
                new CertificatePublicKey
                {
                    kid = "c3ByZGtleTE",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPtYvyfeF/2VvzK7yZFtTzzZjvIzmdDCCHqeQi7/R/PiKJ9HL3exOeVzabVvBkW5uP/+5RnwfSvyxYW/xIhooNg=="
                },
                new CertificatePublicKey
                {
                    kid = "c3ByZGtleTI",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAELv/LGmc0H46LwmLHIRD89CmSRlW8aiFyniNkOgkpjpJ+sApFzfQScbmLlRJx1vZ+PYhWeL5Ktb6w+5ajWj8h5Q=="
                },
                new CertificatePublicKey
                {
                    kid = "d3ByZGtleTE",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEzQtHAcVujgAyvWzHOx9mrtIW3muUpB0sCl22Kt3vMbTMN81yZnEoqkOyOBu3UHE9ifr+RoEVCgMZ7GcDF9Ix1A=="
                },
                new CertificatePublicKey
                {
                    kid = "d3ByZGtleTI",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPJq53/TUr83Jx/KibQsq/FsKOdUBogz9YsH157l/mlrmjEmdToz6vDFMxvvWfdRIvNL0LoRuTSg5XjIvoAYqZA=="
                },
                new CertificatePublicKey
                {
                    kid = "dHN0a2V5MQ==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE0y/p+EB6nMSZEa1GsEfCDrKmZlB5BSMuyFLMDojHYcFTVbA06BTJgjFKQbjcfxTeVVT+csTM8HIlC0dtJZQ8Aw=="
                },
                new CertificatePublicKey
                {
                    kid = "dHN0a2V5Mw==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEorsMdkjyinWvBk4yXGfo1f+GAN1ZfyjPgUOmBL9IyRLcGQS37EYWCJTPPwQXJM3z7b6imf3qNcemOzp71Teb2w=="
                }
            }));
        }

        [HttpGet]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetPublicKeyV2()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList.Concat(new [] 
            {
                new CertificatePublicKey
                {
                    kid = "cHJpdmF0ZWtleQ==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEqErhaHuVTpPQN3jU6ZO5zv/8sIXYIOh3hw87tTYlwVH8Tcov/vizbk2MZU8+gzmJmRcy9BXiTlH7UKesnMtvDg=="
                },
                new CertificatePublicKey
                {
                    kid = "ZGV2a2V5Mg==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEXKME6H60XwR5wCOvD63veSarJDE6DVDyrhak8kHpqr7koAtd3eObk14Sn8k93F9AvQ3fMOh1GoetK4iD6vf7iA=="
                },
                new CertificatePublicKey
                {
                    kid = "c3ByZGtleTE",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPtYvyfeF/2VvzK7yZFtTzzZjvIzmdDCCHqeQi7/R/PiKJ9HL3exOeVzabVvBkW5uP/+5RnwfSvyxYW/xIhooNg=="
                },
                new CertificatePublicKey
                {
                    kid = "c3ByZGtleTI",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAELv/LGmc0H46LwmLHIRD89CmSRlW8aiFyniNkOgkpjpJ+sApFzfQScbmLlRJx1vZ+PYhWeL5Ktb6w+5ajWj8h5Q=="
                },
                new CertificatePublicKey
                {
                    kid = "d3ByZGtleTE",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEzQtHAcVujgAyvWzHOx9mrtIW3muUpB0sCl22Kt3vMbTMN81yZnEoqkOyOBu3UHE9ifr+RoEVCgMZ7GcDF9Ix1A=="
                },
                new CertificatePublicKey
                {
                    kid = "d3ByZGtleTI",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPJq53/TUr83Jx/KibQsq/FsKOdUBogz9YsH157l/mlrmjEmdToz6vDFMxvvWfdRIvNL0LoRuTSg5XjIvoAYqZA=="
                },
                new CertificatePublicKey
                {
                    kid = "dHN0a2V5MQ==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE0y/p+EB6nMSZEa1GsEfCDrKmZlB5BSMuyFLMDojHYcFTVbA06BTJgjFKQbjcfxTeVVT+csTM8HIlC0dtJZQ8Aw=="
                },
                new CertificatePublicKey
                {
                    kid = "dHN0a2V5Mw==",
                    publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEorsMdkjyinWvBk4yXGfo1f+GAN1ZfyjPgUOmBL9IyRLcGQS37EYWCJTPPwQXJM3z7b6imf3qNcemOzp71Teb2w=="
                }
            }));
        }
    }
}
