using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ACHGenerator.CustomAttributes;
using ACHGenerator.DTO;

namespace ACHGenerator
{
    public class ACHFileGenerator
    {
        private static readonly string FillerLine =
            "9999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999";

        public char ImmidiateDestinationFiller { get; set; } = ' ';
        public char ImmidiateOriginFiller { get; set; } = ' ';

        public IEnumerable<string> GenerateACH(ACHTransaction transaction)
        {
            var achLines = new List<string>
            {
                GenerateRecordLine(transaction.FileHeaderRecord)
            };

            foreach (var batchHeaderRecord in transaction.BatchHeaderRecords)
            {
                achLines.Add(GenerateRecordLine(batchHeaderRecord.BatchHeaderRecord));

                achLines.AddRange(batchHeaderRecord.EntryDetailRecords
                    .Select(GenerateRecordLine));

                achLines.Add(batchHeaderRecord.BatchControlRecord == null
                    ? GenerateRecordLine(GenerateBatchControlRecord(batchHeaderRecord))
                    : GenerateRecordLine(batchHeaderRecord.BatchControlRecord));
            }


            if (transaction.FileControlRecord == null)
            {
                transaction.FileControlRecord = GenerateFileControlRecord(transaction);
            }

            var fillerLinesToAdd = CalculateFillerLines(transaction, achLines);

            achLines.Add(GenerateRecordLine(transaction.FileControlRecord));


            achLines.AddRange(
                Enumerable.Repeat(FillerLine, fillerLinesToAdd).ToArray());

            return achLines;
        }

        private BatchControl GenerateBatchControlRecord(BatchHeaderRecordList batchHeaderRecord)
        {
            return new BatchControl
            {
                BatchNumber = batchHeaderRecord.BatchHeaderRecord.BatchNumber,

                CompanyIdentification = batchHeaderRecord.BatchHeaderRecord.CompanyIdentification,

                EntryCount = batchHeaderRecord.EntryDetailRecords.Count(),
                //Calculate Hash from all the entry detail records 
                //and assign it to the Batch Control Record's "Entry Hash"
                EntryHash = CapHashToMaxSize(CalculateEntryHash(batchHeaderRecord.EntryDetailRecords)),

                ODFIIdentification = batchHeaderRecord.BatchHeaderRecord.ODFIIdentification,

                ServiceCode = batchHeaderRecord.BatchHeaderRecord.ServiceCode,
                //Sum of all the credit in the batch
                TotalCreditEntryDollarAmount = SumTotalCreditAmount(batchHeaderRecord.EntryDetailRecords),
                //Sum of all the debit in the batch
                TotalDebitEntryDollarAmount = SumTotalDebitAmount(batchHeaderRecord.EntryDetailRecords)
            };
        }

        private FileControl GenerateFileControlRecord(ACHTransaction transaction)
        {
            return new FileControl
            {
                //Sum all the entryDetailRecords in the transaction
                EntryCount =
                    transaction.BatchHeaderRecords.Sum(b => b.EntryDetailRecords.Count()),
                TotalCreditEntryAmount =
                    transaction.BatchHeaderRecords.Sum(b => SumTotalCreditAmount(b.EntryDetailRecords)),
                TotalDebitEntryAmount =
                    transaction.BatchHeaderRecords.Sum(b => SumTotalDebitAmount(b.EntryDetailRecords)),
                //Sum all the batch records in the transaction
                BatchCount = transaction.BatchHeaderRecords.Count(),
                //Sum of all the entry hashes in the transaction
                EntryHash = CapHashToMaxSize(transaction.BatchHeaderRecords
                    .Sum(b => CalculateEntryHash(b.EntryDetailRecords)))
            };
        }

        private static int CalculateFillerLines(ACHTransaction transaction, List<string> achLines)
        {
            /*Block count is the amount of lines in the ACH file divided by 10
            The number of line sin the file must be divisible by 10
            If its not, we add lines of all 9's (Filler lines)*/
            var fillerLinesToAdd = 0;

            //Number of lines we currently have in the file
            var numberOfLinesInFile = achLines.Count + 1;

            if (numberOfLinesInFile % 10 == 0)
            {
                transaction.FileControlRecord.BlockCount = numberOfLinesInFile / 10;
                return fillerLinesToAdd;
            }

            while (numberOfLinesInFile % 10 != 0)
            {
                fillerLinesToAdd++;
                numberOfLinesInFile++;
            }

            transaction.FileControlRecord.BlockCount = numberOfLinesInFile / 10;

            return fillerLinesToAdd;
        }

        private decimal SumTotalDebitAmount(IEnumerable<EntryDetail> entryDetails)
        {
            return entryDetails.Where(e => e.TransactionCode == 
                (int)ACHTransactionCode.DebitToChecking || e.TransactionCode == (int)ACHTransactionCode.DebitToSavings)
                .Sum(e => e.Amount);
        }

        private decimal SumTotalCreditAmount(IEnumerable<EntryDetail> entryDetails)
        {
            return entryDetails.Where(e => e.TransactionCode ==
                (int)ACHTransactionCode.CreditToChecking || e.TransactionCode == (int)ACHTransactionCode.CreditToSavings)
                .Sum(e => e.Amount);
        }

        public string GenerateRecordLine<T>(T record) where T : IACHRecord
        {
            //Every Record Line is 94 characters long Max
            var recordLine = string.Empty;

            foreach (var recordProperty in record.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var attribute = recordProperty.
                    GetCustomAttributes(typeof(ACHField), true).FirstOrDefault() as ACHField;

                if (attribute == null)
                    throw new Exception($"Property '{recordProperty.Name}' in " +
                                        $"'{record.GetType().Name}' does not have the required 'ACHField' attribute.");

                switch (recordProperty.PropertyType.Name)
                {
                    //If the property type is decimal, it's money and ACH has special rules for currency
                    case "Decimal":
                    {
                        if (string.IsNullOrEmpty(attribute.Format))
                        {
                            throw new Exception
                            ($"Property '{recordProperty.Name}' in '{record.GetType().Name}' " +
                             "must have 'Format' attribute parameter since is currency.");
                        }

                        var propertyValue = (decimal)recordProperty.GetValue(record);

                        var valueAsString = propertyValue.ToString(attribute.Format);

                        //Remove decimal point from value to conform to ACH standard (E.g: 1500.99 to 150099)
                        valueAsString = valueAsString.Replace(".", "");

                        if (valueAsString.Length > attribute.Length)
                        {
                            throw new Exception($"Property '{recordProperty.Name}' in" +
                                                $" '{record.GetType().Name}' has an invalid length, " +
                                                $"should be ({attribute.Length}) and is ({valueAsString.Length}).");
                        }

                        while (valueAsString.Length < attribute.Length)
                        {
                            //If the length of the currency string is less than the 
                            //required field length, pre-pend with 0's until correct length has been reached.
                            //(Numeric Rule)
                            valueAsString = valueAsString.Insert(0, "0");
                        }

                        //Insert value into position set in attribute
                        recordLine = recordLine.Insert(attribute.Position - 1, valueAsString);

                        break;
                    }
                    case "Int32":
                    {
                        var propertyValue = (int)recordProperty.GetValue(record);

                        var valueAsString = string.IsNullOrEmpty(attribute.Format)
                            ? propertyValue.ToString()
                            : propertyValue.ToString(attribute.Format);

                        if (valueAsString.Length > attribute.Length)
                        {
                            throw new Exception($"Property '{recordProperty.Name}' in" +
                                                $" '{record.GetType().Name}' has an invalid length, " +
                                                $"should be ({attribute.Length}) and is ({valueAsString.Length}).");
                        }

                        while (valueAsString.Length < attribute.Length)
                        {
                                //If the length of the number string is less than the 
                                //required field length, pre-pend with 0's until correct length has been reached.
                                //(Numeric Rule)
                            valueAsString = valueAsString.Insert(0, "0");
                        }

                        recordLine = recordLine.Insert(attribute.Position - 1, valueAsString);

                        break;
                    }
                    case "Int64":
                        {
                            var propertyValue = (long)recordProperty.GetValue(record);
                            var valueAsString = string.IsNullOrEmpty(attribute.Format)
                                ? propertyValue.ToString()
                                : propertyValue.ToString(attribute.Format);
                            if (valueAsString.Length > attribute.Length)
                            {
                                throw new Exception($"Property '{recordProperty.Name}' in" +
                                                    $" '{record.GetType().Name}' has an invalid length, " +
                                                    $"should be ({attribute.Length}) and is ({valueAsString.Length}).");
                            }
                            while (valueAsString.Length < attribute.Length)
                            {
                                //If the length of the number string is less than the 
                                //required field length, pre-pend with 0's until correct length has been reached.
                                //(Numeric Rule)
                                valueAsString = valueAsString.Insert(0, "0");
                            }
                            recordLine = recordLine.Insert(attribute.Position - 1, valueAsString);
                            break;
                        }
                    case "DateTime":
                    {
                        if (attribute.Format == null)
                        {
                            throw new Exception($"Property '{recordProperty.Name}' in" +
                                                $" '{record.GetType().Name}' is of 'DateTime' type and must contain a 'Format' attribute parameter.");
                        }

                        var propertyValue = (DateTime)recordProperty.GetValue(record);

                        //All Date and Times must be formatted, apply formatting
                        recordLine = recordLine.Insert(attribute.Position - 1,
                            propertyValue.ToString(attribute.Format));

                        break;
                    }
                    case "String":
                    {
                        var propertyValue = (string)recordProperty.GetValue(record);

                        if (!string.IsNullOrEmpty(attribute.Format))
                        {
                            propertyValue = string.Format(attribute.Format, propertyValue);
                        }

                        if (propertyValue.Length > attribute.Length)
                        {
                            throw new Exception($"Property '{recordProperty.Name}' in" +
                                                $" '{record.GetType().Name}' has an invalid length, " +
                                                $"should be ({attribute.Length}) and is ({propertyValue.Length}).");
                        }

                        while (propertyValue.Length < attribute.Length)
                        {
                            //If the length of the number string is less than the 
                            //required field length, post-pend with white space until correct length has been reached.
                            //(Alphanumeric Rule)

                            propertyValue = recordProperty.Name switch
                            {
                                // Determine which filler to use based on the property name
                                // Different banks have different rules for what filler to use
                                nameof(FileHeader.ImmidiateOrigin) => 
                                    propertyValue.Insert(0, ImmidiateOriginFiller.ToString()),
                                nameof(FileHeader.ImmidiateDestination) => 
                                    propertyValue.Insert(0, ImmidiateDestinationFiller.ToString()),
                                _ => 
                                    propertyValue.Insert(propertyValue.Length, " ")
                            };
                        }

                        recordLine = recordLine.Insert(attribute.Position - 1, propertyValue);

                        break;
                    }
                }
            }

            return recordLine;
        }

        private long CalculateEntryHash(IEnumerable<EntryDetail> entryDetailRecords)
        {
            return entryDetailRecords.Sum(e => e.RDFIIdentification);
        }

        private long CapHashToMaxSize(long hash)
        {
            //all hashes in ACH are 10 digits long max, 
            //so we will lop off the most significant digits until we reach a minimum of 10 digits.
            var hashString = hash.ToString();

            return hashString.Length > 10 ? 
                Convert.ToInt32(hashString.Substring(hashString.Length, -10)) : 
                hash;
        }
    }
}
