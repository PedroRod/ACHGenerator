using ACHGenerator;
using ACHGenerator.DTO;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CanGenerateFileHeaderRecord()
        {
            var generator = new ACHFileGenerator();

            var fileHeaderRecord = new FileHeader
                {
                    ImmidiateDestination = "121140399",
                    ImmidiateOrigin = "123456789",
                    FileCreationDate = new DateTime(2017,9,15,12,30,15),
                    FileCreationTime = new DateTime(2017, 9, 15, 12, 30, 15),
                    ImmediateDestinationName = "Bank Of America",
                    ImmediateOriginName = "Pedro's Bank",
                    ReferenceCode = 1235
                };

            var line = generator.GenerateRecordLine(fileHeaderRecord);

            Assert.AreEqual(line,
                "101121140399 123456789 1709151230A094101Bank Of America        Pedro's Bank           00001235");
        }
        
        [Test]
        public void CanGenerateFileHeaderRecordForTenDigits()
        {
            var generator = new ACHFileGenerator();

            var fileHeaderRecord = new FileHeader
                {
                    ImmidiateDestination = "121140399I",
                    ImmidiateOrigin = "123456789B",
                    FileCreationDate = new DateTime(2017,9,15,12,30,15),
                    FileCreationTime = new DateTime(2017, 9, 15, 12, 30, 15),
                    ImmediateDestinationName = "Bank Of America",
                    ImmediateOriginName = "Pedro's Bank",
                    ReferenceCode = 1235
                };

            var line = generator.GenerateRecordLine(fileHeaderRecord);

            Assert.AreEqual(line,
                "101121140399I123456789B1709151230A094101Bank Of America        Pedro's Bank           00001235");
        }

        [Test]
        public void CanGenerateBatchHeaderRecord()
        {
            var generator = new ACHFileGenerator();

            var batchHeader = new BatchHeader
            {
                BatchNumber = 0,
                ServiceCode = (int)ACHServiceCode.Debit,
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

            Assert.AreEqual(line,
                "5225TestCompName    ForBatchDesc        1234567890PPDPayroll   170915170916   1001234560000000");
        }

        [Test]
        public void CanGenerateEntryDetailRecord()
        {
            var generator = new ACHFileGenerator();

            var entryDetail = new EntryDetail
            {
                TransactionCode = (int) ACHTransactionCode.DebitToChecking,
                RDFIIdentification = 123456,
                CheckDigit = 5,
                DFIAccountNumber = "ABC123BA#",
                Amount = (decimal) 1500.53,
                IndividualIdentificationNumber = "BCA123",
                IndividualName = "Pedro Rodriguez",
                TraceNumber = "TRCNUM"
            };

            var line = generator.GenerateRecordLine(entryDetail);

            Assert.AreEqual(line,
                "627001234565ABC123BA#        0000150053BCA123         Pedro Rodriguez         0TRCNUM         ");

        }

        [Test]
        public void CanGenerateBatchControlRecord()
        {
            var generator = new ACHFileGenerator();

            var batchControl = new BatchControl
            {
                ServiceCode = (int) ACHServiceCode.Debit,
                EntryCount = 2,
                EntryHash = 12456,
                TotalDebitEntryDollarAmount = (decimal) 1500.53,
                TotalCreditEntryDollarAmount = (decimal) 1500.53,
                ODFIIdentification = 123456,
                BatchNumber = 0,
                CompanyIdentification = "1234567890"
            };

            var line = generator.GenerateRecordLine(batchControl);

            Assert.AreEqual(line,
                "822500000200000124560000001500530000001500531234567890                         001234560000000");

        }

        [Test]
        public void CanGenerateFileControlRecord()
        {
            var generator = new ACHFileGenerator();

            var fileControl = new FileControl
            {
                BatchCount = 2,
                BlockCount = 1,
                EntryCount = 1,
                EntryHash = 2,
                TotalDebitEntryAmount = (decimal)1500.53,
                TotalCreditEntryAmount = (decimal)1500.53
            };

            var line = generator.GenerateRecordLine(fileControl);

            Assert.AreEqual(line,
                "9000002000001000000010000000002000000150053000000150053                                       ");

        }
    }
}
