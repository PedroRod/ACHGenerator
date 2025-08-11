using ACHGenerator;
using ACHGenerator.DTO;
using NUnit.Framework;
using UnitTests.Model;

namespace UnitTests;

[TestFixture]
public class OverrideBatchHeaderFormat
{
    [Test]
    public void CanGenerateBatchHeaderRecord()
    {
        var generator = new ACHFileGenerator();

        var batchHeader = new TrustMarkBankBatchHeader
        {
            BatchNumber = 0,
            ServiceCode = (int) ACHServiceCode.Debit,
            CompanyName = "TestCompName",
            CompanyDiscretionaryData = "ForBatchDesc",
            CompanyIdentification = "1234567890",
            StandardEntryClassCode = "PPD",
            CompanyEntryDescription = "Payroll",
            CompanyDescriptiveDate = new DateTime(2017, 9, 15, 12, 30, 15),
            EffectiveEntryDate = new DateTime(2017, 9, 16, 12, 30, 15),
            ODFIIdentification = 123456
        };

        var line = generator.GenerateRecordLine(batchHeader);

        Assert.That(line, Is.EqualTo("5225TestCompName    ForBatchDesc        1234567890PPDPayroll   091517170916   1001234560000000"));
    }
}
