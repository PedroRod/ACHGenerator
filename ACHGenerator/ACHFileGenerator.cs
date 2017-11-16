using System;
using System.Collections.Generic;
using System.Linq;
using ACHGenerator.CustomAttributes;
using ACHGenerator.DTO;

namespace ACHGenerator
{
    public class ACHFileGenerator
    {
        public IEnumerable<string> GenerateACH(ACHTransaction transaction)
        {
            var achFile = new List<string>
            {
                GenerateRecordLine(transaction.FileHeaderRecord)
            };

            foreach (var batchHeaderRecord in transaction.BatchHeaderRecords)
            {
                achFile.Add(GenerateRecordLine(batchHeaderRecord.BatchHeaderRecord));

                achFile.AddRange(batchHeaderRecord.EntryDetailRecords
                    .Select(entryDetailRecord => GenerateRecordLine(entryDetailRecord)));

                achFile.Add(GenerateRecordLine(batchHeaderRecord.BatchControlRecord));
            }

            achFile.Add(GenerateRecordLine(transaction.FileControlRecord));

            return achFile;
        }

        public string GenerateRecordLine<T>(T record) where T : IACHRecord
        {
            //Every Record Line is 94 characters long Max
            var recordLine = string.Empty;

            foreach (var recordProperty in record.GetType().GetProperties())
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
                    case "DateTime":
                    {
                        if (attribute.Format == null)
                        {
                            throw new Exception($"Property '{recordProperty.Name}' in" +
                                                $" '{record.GetType().Name}' is of 'DateTime' type and must contain a 'Format' attribute parameter.");
                        }

                        var propertyValue = (DateTime)recordProperty.GetValue(record);

                        //All Date and Times must be formated, apply formating
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
                            propertyValue = propertyValue.Insert(propertyValue.Length, " ");
                        }

                        recordLine = recordLine.Insert(attribute.Position - 1, propertyValue);

                        break;
                    }
                }
            }

            return recordLine;
        }
    }
}
