using System;
using System.Collections.Generic;
using System.IO;
using ACHGenerator;
using ACHGenerator.DTO;

namespace ACHFileGeneratorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    new BatchHeaderRecordList
                    {
                        BatchHeaderRecord = new BatchHeader
                        {
                            BatchNumber = 0,
                            ServiceCode = (int)ACHServiceCode.Debit,
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
                            new EntryDetail
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
                            new EntryDetail
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
                        },
                        BatchControlRecord = new BatchControl
                        {
                            ServiceCode = (int)ACHServiceCode.Debit,
                            EntryCount = 2,
                            EntryHash = 12456,
                            TotalDebitEntryDollarAmount = (decimal) 1500.53,
                            TotalCreditEntryDollarAmount = (decimal) 1500.53,
                            ODFIIdentification = 123456,
                            BatchNumber = 0,
                            CompanyIdentification = 55
                        }
                    }
                },
                FileControlRecord = new FileControl
                {
                    BatchCount = 2,
                    BlockCount = 1,
                    EntryCount = 1,
                    EntryHash = 2,
                    TotalDebitEntryAmount = (decimal)1500.53,
                    TotalCreditEntryAmount = (decimal)1500.53
                }
            };

            var achFileGenerator = new ACHFileGenerator();

            var achLines = achFileGenerator.GenerateACH(achTransaction);

            File.WriteAllLines("ACH.txt", achLines);
        }
    }
}
