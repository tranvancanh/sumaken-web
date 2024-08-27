using System;

namespace stock_management_system.Models
{
    public class D_StoreOutModel
    {
        public long ScanRecordID { get; set; }
        public long ShipmentInstructionDetailID { get; set; }
        public long StoreInID { get; set; }
        public int DepoID { get; set; }
        public DateTime StoreOutDate { get; set; }
        public string ProductCode { get; set; }
        public string Packing { get; set; }
        public int PackingCount { get; set; }
        public int Quantity { get; set; }
        public string StockLocation1 { get; set; }
        public string StockLocation2 { get; set; }
        public bool AdjustmentFlag { get; set; }
        public string Remark { get; set; }
        public bool DeleteFlag { get; set; }
        public long DeleteStoreOutID { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserID { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserID { get; set; }

        public D_StoreOutModel()
        {
            AdjustmentFlag = false;
            Remark = "";
            DeleteFlag = false;
            DeleteStoreOutID = 0;
        }
    }
}
