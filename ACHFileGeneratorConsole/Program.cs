using ACHGenerator;
using ACHGenerator.DTO;

var achTransaction = new ACHTransaction
{
    FileHeaderRecord = new FileHeader
    {
        ImmidiateDestination = "121140399",
        ImmidiateOrigin = "123456789",
        FileCreationDate = DateTime.Now,
        FileCreationTime = DateTime.Now,
        ImmediateDestinationName = "Bank Of America",
        ImmediateOriginName = "Pedro's Bank",
        ReferenceCode = 1235
    },
    BatchHeaderRecords = new List<BatchHeaderRecordList>
    {
        new()
        {
            BatchHeaderRecord = new BatchHeader
            {
                BatchNumber = 0,
                ServiceCode = (int) ACHServiceCode.Debit,
                CompanyName = "TestCompName",
                CompanyDiscretionaryData = "ForBatchDesc",
                CompanyIdentification = "1234567890",
                StandardEntryClassCode = "PPD",
                CompanyEntryDescription = "Payroll",
                CompanyDescriptiveDate = DateTime.Today,
                EffectiveEntryDate = DateTime.Today.AddDays(1),
                ODFIIdentification = 123456
            },
            EntryDetailRecords = new List<EntryDetail>
            {
                new()
                {
                    TransactionCode = (int) ACHTransactionCode.DebitToChecking,
                    RDFIIdentification = 123456,
                    CheckDigit = 5,
                    DFIAccountNumber = "ABC123BA#",
                    Amount = (decimal) 1500.53,
                    IndividualIdentificationNumber = "BCA123",
                    IndividualName = "Pedro Rodriguez",
                    TraceNumber = "TRCNUM"
                },
                new()
                {
                    TransactionCode = (int) ACHTransactionCode.CreditToChecking,
                    RDFIIdentification = 654231,
                    CheckDigit = 1,
                    DFIAccountNumber = "DFIAccountNum",
                    Amount = (decimal) 1500.53,
                    IndividualIdentificationNumber = "IndiIDNum",
                    IndividualName = "Pedro Rodriguez",
                    TraceNumber = "TraceNumber"
                },
                new()
                {
                    TransactionCode = (int) ACHTransactionCode.CreditToChecking,
                    RDFIIdentification = 654231,
                    CheckDigit = 1,
                    DFIAccountNumber = "DFIAccountNum",
                    Amount = (decimal) 1500.53,
                    IndividualIdentificationNumber = "IndiIDNum",
                    IndividualName = "Pedro Rodriguez",
                    TraceNumber = "TraceNumber"
                },
                new()
                {
                    TransactionCode = (int) ACHTransactionCode.CreditToChecking,
                    RDFIIdentification = 654231,
                    CheckDigit = 1,
                    DFIAccountNumber = "DFIAccountNum",
                    Amount = (decimal) 1500.53,
                    IndividualIdentificationNumber = "IndiIDNum",
                    IndividualName = "Pedro Rodriguez",
                    TraceNumber = "TraceNumber"
                }
            }
        }
    }
};

var achFileGenerator = new ACHFileGenerator();

var achLines = achFileGenerator.GenerateACH(achTransaction);

File.WriteAllLines("ACH.txt", achLines);