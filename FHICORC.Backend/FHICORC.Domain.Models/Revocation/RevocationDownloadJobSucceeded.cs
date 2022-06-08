using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models.Revocation
{
    public class RevocationDownloadJobSucceeded
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public DateTime LastDownloadJobSucceeded { get; set; }

        public RevocationDownloadJobSucceeded(DateTime lastDownloadJobSucceeded)
        {
            this.LastDownloadJobSucceeded = lastDownloadJobSucceeded;
        }
    }
}
