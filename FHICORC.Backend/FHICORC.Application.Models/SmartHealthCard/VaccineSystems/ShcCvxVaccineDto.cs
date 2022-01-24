using System.Collections.Generic;
using System.Linq;

namespace FHICORC.Application.Models.SmartHealthCard.VaccineSystems
{
    public class ShcCvxVaccineListDto
    {
        public List<ShcCvxVaccineDto> CvxVaccines { get; private set; }

        public ShcCvxVaccineListDto()
        {
            CvxVaccines = new();
        }
    }

    public class ShcCvxVaccineDto
    {
        public string CdcProductName { get; set; }
        public string ShortDescription { get; set; }
        public string CvxCode { get; set; }
        public string Manufacturer { get; set; }
        public string Status { get; set; }
        public string LastUpdated { get; set; }

        // CSV contructor
        public ShcCvxVaccineDto(string[] csvValues)
        {
            foreach (var field in csvValues.Select((value, i) => new { i, value }))
            {
                switch (field.i)
                {
                    case (int) ShcCvxVaccineDtoCsvElements.CdcProductNameIndex:
                        CdcProductName = field.value;
                        break;
                    case (int)ShcCvxVaccineDtoCsvElements.ShortDescriptionIndex:
                        ShortDescription = field.value;
                        break;
                    case (int)ShcCvxVaccineDtoCsvElements.CvxCodeIndex:
                        CvxCode = field.value;
                        break;
                    case (int)ShcCvxVaccineDtoCsvElements.ManufacturerIndex:
                        Manufacturer = field.value;
                        break;
                    case (int)ShcCvxVaccineDtoCsvElements.StatusIndex:
                        Status = field.value;
                        break;
                    case (int)ShcCvxVaccineDtoCsvElements.LastUpdatedIndex:
                        LastUpdated = field.value;
                        break;
                }
            }
        }

        public enum ShcCvxVaccineDtoCsvElements
        {
            CdcProductNameIndex = 0,
            ShortDescriptionIndex = 1,
            CvxCodeIndex = 2,
            ManufacturerIndex = 3,
            StatusIndex = 6,
            LastUpdatedIndex = 7
        }
    }
}
