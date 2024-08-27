namespace stock_management_system.Models.common
{
    public class Enums
    {
        /// <summary>
        /// レーン状態
        /// </summary>
        public enum LaneState
        {
			/// <summary>
			/// 荷物なし
			/// </summary>
			NoLuggage = 0,

			/// <summary>
			/// 荷物あり
			/// </summary>
			WithLuggage = 1,

			/// <summary>
			/// 禁止
			/// </summary>
			Prohibit = 2
		}

		/// <summary>
		/// ハンディページID
		/// </summary>
		public enum HandyPageID
		{
			/// <summary>
			/// 入庫-通常
			/// </summary>
			NormalStoreIn = 206,

			/// <summary>
			/// 入庫-はみ出し
			/// </summary>
			ProtrudeStoreIn = 207,

			/// <summary>
			/// 入庫-パレット単位
			/// </summary>
			PaletteStoreIn = 208,

			/// <summary>
			/// 出庫-通常
			/// </summary>
			NormalStoreOut = 301,

            /// <summary>
            /// 入庫-番地戻し
            /// </summary>
            ReturnStoreIn = 401,

            /// <summary>
            /// AGF指示
            /// </summary>
            AGFInstruction = 601
		}


		public enum UserCompanyId
        {
            test = 1,
            technoleight = 2
        }

        public enum ShipmentImportDuplicationCheckType
        {
            None = 0,
            Error = 1,
            Skip = 2,
        }

        public enum SystemSetting
        {
            /// <summary>
            /// 【在庫状況】集計の単位
            /// 0…箱数　1…数量
            /// </summary>
            StockStatus_3010 = 3010,
            /// <summary>
            /// 【在庫状況】商品名の表示
            /// 0…無　1…有
            /// </summary>
            StockStatus_3011 = 3011,
            /// <summary>
            /// 【在庫状況】商品略称の表示
            /// 0…無　1…有
            /// </summary>
            StockStatus_3012 = 3012,
        }

        public enum ProcessID
        {
            /// <summary>
            /// 出荷データ取込
            /// </summary>
            ShipmentImport = 17
        }
    }
}
