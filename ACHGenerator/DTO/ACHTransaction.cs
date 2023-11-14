using System;
using System.Collections.Generic;
using ACHGenerator.CustomAttributes;

namespace ACHGenerator.DTO
{
    public class FileHeader : IACHRecord
    {
        [ACHField(Position = 1, Length = 1)]
        private string RecordTypeCode => "1";

        [ACHField(Position = 2, Length = 2)]
        private string PriorityCode => "01";

        [ACHField(Position = 4, Length = 10, Format = "{0}")]
        public string ImmidiateDestination { get; set; }

        [ACHField(Position = 14, Length = 10, Format = "{0}")]
        public string ImmidiateOrigin { get; set; }

        [ACHField(Position = 24, Length = 6, Format = "yyMMdd")]
        public DateTime FileCreationDate { get; set; }

        [ACHField(Position = 30, Length = 4, Format = "HHmm")]
        public DateTime FileCreationTime { get; set; }

        [ACHField(Position = 34, Length = 1)]
        private string FileIdModifier => "A";

        [ACHField(Position = 35, Length = 3)]
        private string RecordSize => "094";

        [ACHField(Position = 38, Length = 2)]
        private string BlockingFactor => "10";

        [ACHField(Position = 40, Length = 1)]
        private string FormatCode => "1";

        [ACHField(Position = 41, Length = 23)]
        public string ImmediateDestinationName { get; set; }

        [ACHField(Position = 64, Length = 23)]
        public string ImmediateOriginName { get; set; }

        [ACHField(Position = 87, Length = 8)]
        public int ReferenceCode { get; set; }
    }

    public class BatchHeader : IACHRecord
    {
        [ACHField(Position = 1, Length = 1)]
        private string RecordTypeCode => "5";

        [ACHField(Position = 2, Length = 3)]
        public int ServiceCode { get; set; }

        [ACHField(Position = 5, Length = 16)]
        public string CompanyName { get; set; }

        [ACHField(Position = 21, Length = 20)]
        public string CompanyDiscretionaryData { get; set; }

        [ACHField(Position = 41, Length = 10)]
        public string CompanyIdentification { get; set; }

        [ACHField(Position = 51, Length = 3)]
        public string StandardEntryClassCode { get; set; }

        [ACHField(Position = 54, Length = 10)]
        public string CompanyEntryDescription { get; set; }

        [ACHField(Position = 64, Length = 6, Format = "yyMMdd")]
        public DateTime CompanyDescriptiveDate { get; set; }

        [ACHField(Position = 70, Length = 6, Format = "yyMMdd")]
        public DateTime EffectiveEntryDate { get; set; }

        [ACHField(Position = 76, Length = 3)]
        private string SettlementDate => "   ";

        [ACHField(Position = 79, Length = 1)]
        private string OriginatorStatusCode => "1";

        [ACHField(Position = 80, Length = 8)]
        public int ODFIIdentification { get; set; }

        [ACHField(Position = 88, Length = 7)]

        public int BatchNumber { get; set; }
    }

    public class EntryDetail : IACHRecord
    {
        [ACHField(Position = 1, Length = 1)]
        private string RecordTypeCode => "6";

        [ACHField(Position = 2, Length = 2)]
        public int TransactionCode { get; set; }

        [ACHField(Position = 4, Length = 8)]
        public long RDFIIdentification { get; set; }

        [ACHField(Position = 12, Length = 1)]
        public int CheckDigit { get; set; }

        [ACHField(Position = 13, Length = 17)]
        public string DFIAccountNumber { get; set; }

        [ACHField(Position = 30, Length = 10, Format = "0.00")]
        public decimal Amount { get; set; }

        [ACHField(Position = 40, Length = 15)]
        public string IndividualIdentificationNumber { get; set; }

        [ACHField(Position = 55, Length = 22)]
        public string IndividualName { get; set; }

        [ACHField(Position = 77, Length = 2)]
        private string DiscretionaryData => "  ";

        [ACHField(Position = 79, Length = 1)]
        private string AddendaRecordindicator => "0";

        [ACHField(Position = 80, Length = 15)]
        public string TraceNumber { get; set; }
    }

    public class BatchControl : IACHRecord
    {
        [ACHField(Position = 1, Length = 1)]
        private string RecordTypeCode => "8";

        [ACHField(Position = 2, Length = 3)]
        public int ServiceCode { get; set; }

        [ACHField(Position = 5, Length = 6)]
        public int EntryCount { get; set; }

        [ACHField(Position = 11, Length = 10)]
        public long EntryHash { get; set; }

        [ACHField(Position = 21, Length = 12, Format = "0.00")]
        public decimal TotalDebitEntryDollarAmount { get; set; }

        [ACHField(Position = 33, Length = 12, Format = "0.00")]
        public decimal TotalCreditEntryDollarAmount { get; set; }

        [ACHField(Position = 45, Length = 10)]
        public string CompanyIdentification { get; set; }

        [ACHField(Position = 55, Length = 19)]
        private string MessageAuthenticationCode => "                   ";

        [ACHField(Position = 74, Length = 6)]
        private string Filler => "      ";

        [ACHField(Position = 80, Length = 8)]
        public int ODFIIdentification { get; set; }

        [ACHField(Position = 88, Length = 7)]
        public int BatchNumber { get; set; }
    }

    public class FileControl : IACHRecord
    {
        [ACHField(Position = 1, Length = 1)]
        private string RecordTypeCode => "9";

        [ACHField(Position = 2, Length = 6)]
        public int BatchCount { get; set; }

        [ACHField(Position = 8, Length = 6)]
        public int BlockCount { get; set; }

        [ACHField(Position = 14, Length = 8)]
        public int EntryCount { get; set; }

        [ACHField(Position = 22, Length = 10)]
        public long EntryHash { get; set; }

        [ACHField(Position = 32, Length = 12, Format = "0.00")]
        public decimal TotalDebitEntryAmount { get; set; }

        [ACHField(Position = 44, Length = 12, Format = "0.00")]
        public decimal TotalCreditEntryAmount { get; set; }

        [ACHField(Position = 56, Length = 39)]
        private string Filler => "                                       ";
    }

    public class BatchHeaderRecordList
    {
        public BatchHeader BatchHeaderRecord { get; set; }
        public IEnumerable<EntryDetail> EntryDetailRecords { get; set; }
        public BatchControl BatchControlRecord { get; set; }
    }

    public class ACHTransaction
    {
        public FileHeader FileHeaderRecord { get; set; }
        public IEnumerable<BatchHeaderRecordList> BatchHeaderRecords { get; set; }
        public FileControl FileControlRecord { get; set; }
    }
}
