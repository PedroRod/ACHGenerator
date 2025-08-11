using ACHGenerator.CustomAttributes;
using ACHGenerator.DTO;

namespace UnitTests.Model;

public class TrustMarkBankBatchHeader: BatchHeader
{
    [ACHField(Position = 64, Length = 6, Format = "MMddyy")]
    public override DateTime CompanyDescriptiveDate { get; set; }
}
