using Newtonsoft.Json;
using OfficeOpenXml;
using stock_management_system.Models;
using stock_management_system.Models.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace stock_management_system.common
{
    // 共通関数
    // 参考：https://gist.github.com/Buravo46/49c34e77ff1a75177340

    public static class Util
    {

        public static string GetHashString<T>(string text) where T : HashAlgorithm, new()
        {
            // 文字列をバイト型配列に変換する
            byte[] data = Encoding.UTF8.GetBytes(text);

            // ハッシュアルゴリズム生成
            var algorithm = new T();

            // ハッシュ値を計算する
            byte[] bs = algorithm.ComputeHash(data);

            // リソースを解放する
            algorithm.Clear();

            // バイト型配列を16進数文字列に変換
            var result = new StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("X2"));
            }

            return result.ToString();
        }

        public static string NullToBlank(object value)
        {
            // NULL、DBNullのときは空文字に変換する
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }
            return Convert.ToString(value);
        }

        //////////////////////////////////////////////////////////

        public static string NullToBlank_Ex(string value)
        {
            // NULL、DBNullのときは空文字に変換する
            if (string.IsNullOrWhiteSpace(value) == true)
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToString(value).Trim();
            }

        }
        public static int NullToBlank_Ex(object value)
        {
            // NULL、DBNullのときは空文字に変換する
            if (string.IsNullOrWhiteSpace(value.ToString()) == true)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(value.ToString().Trim());
            }
        }

        /// <summary>
        /// nullの場合は空白、そうでない場合はTrimした文字列を返す
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ObjectToString(object value)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return Convert.ToString(value).Trim();
            }
        }
        /// <summary>
        /// nullの場合は0、そうでない場合は数値変換して返す。変換失敗はExceptionエラーを返す。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ObjectToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else
            {
                if (int.TryParse(value.ToString(), out int val))
                {
                    return val;
                }
                else 
                {
                    throw new CustomExtention("数値変換に失敗しました。");
                }
            }
        }
        /// <summary>
        /// nullの場合は1900/01/01、そうでない場合は日付変換して返す。変換失敗はExceptionエラーを返す。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ObjectToDateTimeExNull(object value)
        {
            try
            {
                var datetimeValue = ObjectToDateTime(value);
                return datetimeValue;
            }
            catch(Exception ex) 
            {
                return Convert.ToDateTime("1900/01/01");
            }
        }

        /// <summary>
        /// nullの場合は1900/01/01、そうでない場合は日付変換して返す。変換失敗はExceptionエラーを返す。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ObjectToDateTime(object value)
        {
            var datetimeValue = new DateTime();
            try
            {
                if (value == null || value.ToString() == "")
                {
                    return Convert.ToDateTime("1900/01/01");
                }
                else
                {
                    var valueString = value.ToString();
                    if (DateTime.TryParse(valueString, out datetimeValue))
                    {
                        return datetimeValue;
                    }
                    else if (valueString.All(char.IsDigit))
                    {
                        if (valueString.Length == 6)
                        {
                            var year = "20" + valueString.Substring(0, 2);
                            var month = valueString.Substring(2, 2);
                            var day = valueString.Substring(4, 2);

                            if (DateTime.TryParse((year + "/" + month + "/" + day), out datetimeValue))
                            {

                            }
                            else
                            {
                                // シリアル値に変換
                                var doubleMoji = double.Parse(valueString);
                                datetimeValue = DateTime.FromOADate(doubleMoji);
                            }
                        }
                        else
                        {
                            // シリアル値に変換
                            var doubleMoji = double.Parse(valueString);
                            datetimeValue = DateTime.FromOADate(doubleMoji);
                        }

                        var now = DateTime.Now;
                        if ((datetimeValue.AddDays(120)) > now && (datetimeValue.AddDays(-120) < now))
                        {
                            return datetimeValue;
                        }
                        else
                        {
                            throw new CustomExtention("日付変換に失敗しました。");
                        }
                    }
                    else
                    {
                        throw new CustomExtention("日付変換に失敗しました。");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomExtention("日付変換に失敗しました。");
            }
        }

        /// <summary>
        /// nullの場合は1900/01/01、そうでない場合は日付変換して返す。変換失敗はExceptionエラーを返す。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ObjectToDateTimeByTimeEx(DateTime date, string value)
        {
            try
            {
                if (value.All(char.IsDigit))
                {
                    if (value.Length == 6)
                    {
                        var hh = value.Substring(0, 2);
                        var mm = value.Substring(2, 2);
                        var ss = value.Substring(4, 2);

                        date = new DateTime(date.Year, date.Month, date.Day, Convert.ToInt32(hh), Convert.ToInt32(mm), Convert.ToInt32(ss));
                    }
                    else if (value.Length == 4)
                    {
                        var hh = value.Substring(0, 2);
                        var mm = value.Substring(2, 2);

                        date = new DateTime(date.Year, date.Month, date.Day, Convert.ToInt32(hh), Convert.ToInt32(mm), 0);
                    }
                }

                return date;

            }
            catch (Exception ex)
            {
                return date;
            }
        }

        public static Page ComPageNoGet(int pageNo, int pageSize, int dataCount)
        {
            var page = new Page();
            var endNo = pageNo * pageSize;
            var startNo = endNo - (pageSize - 1);
            if (endNo > dataCount)
            {
                endNo = dataCount;
            }
            page.PageRowStartNo = startNo;
            page.PageRowEndNo = endNo;

            return page;
        }

        public static string ToHankakuKatakana(string value)
        {
            var HankakuKana = Microsoft.VisualBasic.Strings.StrConv(value, Microsoft.VisualBasic.VbStrConv.Katakana | Microsoft.VisualBasic.VbStrConv.Narrow, 0x411);
            return HankakuKana;
        }

        public static object ChangeJsonResultObject(string result, string message)
        {
            var jsonResult = new
            {
                result,
                message
            };

            return jsonResult;
        }

        public static object ChangeJsonResultObjectAndNewId(string result, string message, long newid)
        {
            var jsonResult = new
            {
                result,
                message,
                newid
            };

            return jsonResult;
        }

        //public static bool Equals_Two_Object(this object thisObj, object another)
        //{
        //    if (ReferenceEquals(thisObj, another)) return true;
        //    if ((thisObj == null) || (another == null)) return false;
        //    if (thisObj.GetType() != another.GetType()) return false;

        //    var objJson = JsonConvert.SerializeObject(thisObj);
        //    var anotherJson = JsonConvert.SerializeObject(another);

        //    return objJson == anotherJson;
        //}

        //public static string DatetimeToFFF(DateTime dateTime)
        //{
        //    var dateTimeFFF = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
        //    return dateTimeFFF;
        //}

        //public static bool ExportExcel(DataTable dt, string exportfileFullPath, List<string> titleRows, List<int> dateTimeColumns, string dateTimeKu,string sheetName)
        //{
        //    // データがない時は中断
        //    if (dt == null || dt.Rows.Count == 0)
        //    {
        //        return false;
        //    }
        //    // 出力ファイルパスが未指定の場合は中断する
        //    if (String.IsNullOrWhiteSpace(exportfileFullPath))
        //    {
        //        return false;
        //    }
        //    // 出力フォルダが存在しない場合は中断する
        //    if (!Directory.Exists(Path.GetDirectoryName(exportfileFullPath)))
        //    {
        //        return false;
        //    }
        //    // 既にファイルが存在している場合は削除する
        //    if (File.Exists(exportfileFullPath))
        //    {
        //        File.Delete(exportfileFullPath);
        //    }

        //    try
        //    {
        //        // 出力用ファイルを生成する
        //        FileInfo fileInfo = new FileInfo(exportfileFullPath);
        //        var startIndex = 1;
        //        var printHeader = true;

        //        using (var package = new ExcelPackage(fileInfo))
        //        {
        //            // シート追加
        //            package.Workbook.Worksheets.Add(sheetName);
        //            // シート取得
        //            using (ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName])
        //            {
        //                // タイトル行が指定されているときは、タイトル行をセットする
        //                if (titleRows != null && titleRows.Count > 0)
        //                {
        //                    for (int i = 0; i < titleRows.Count; i++)
        //                    {
        //                        sheet.Cells[1, i + 1].Value = titleRows[i];
        //                    }
        //                    // 開始行番号をセット
        //                    startIndex = 2;
        //                    // タイトル出力済なので、列名は出力しない
        //                    printHeader = false;
        //                }

        //                // データセット
        //                sheet.Cells[startIndex, 1].LoadFromDataTable(dt, printHeader);

        //                // 日付指定をする列番号がある場合
        //                if (dateTimeColumns != null && dateTimeColumns.Count > 0)
        //                {
        //                    for (int i = 0; i < dateTimeColumns.Count; i++)
        //                    {
        //                        if (dateTimeKu == "Time")
        //                        {
        //                            sheet.Cells[startIndex, dateTimeColumns[i], sheet.Dimension.Rows, dateTimeColumns[i]].Style.Numberformat.Format = "yyyy/MM/dd hh:mm:ss";
        //                        }
        //                        else
        //                        {
        //                            sheet.Cells[startIndex, dateTimeColumns[i], sheet.Dimension.Rows, dateTimeColumns[i]].Style.Numberformat.Format = "yyyy/MM/dd";
        //                        }

        //                    }
        //                }

        //                // Upload画面のとき（ファイル名最初の2文字が数字）だけ、最後の行に"END"を追加
        //                var fileHeadName = fileInfo.Name.Substring(0, 2);
        //                if (int.TryParse(fileHeadName, out int number))
        //                {
        //                    var lastRowNo = sheet.Dimension.End.Row;
        //                    sheet.InsertRow(lastRowNo + 1, 1);
        //                    sheet.Cells[lastRowNo + 1, 1].Value = "END";
        //                };

        //                // 保管
        //                package.Save();
        //            }
        //        }

        //    }
        //    catch
        //    {
        //        // 失敗したときは出力用ファイルを削除する
        //        if (File.Exists(exportfileFullPath))
        //        {
        //            File.Delete(exportfileFullPath);
        //        }

        //        throw;
        //    }

        //    return true;
        //}
        ////Excel出力ここまで==========================================================

        //public static bool ExportCsv(DataTable dt, string exportfileFullPath, List<string> titleRows , List<int> quoteColumns)
        //{
        //    // データがない時は中断
        //    if (dt == null || dt.Rows.Count == 0)
        //    {
        //        return false;
        //    }
        //    // 出力ファイルパスが未指定の場合は中断する
        //    if (String.IsNullOrWhiteSpace(exportfileFullPath))
        //    {
        //        return false;
        //    }
        //    // 出力フォルダが存在しない場合は中断する
        //    if (!Directory.Exists(Path.GetDirectoryName(exportfileFullPath)))
        //    {
        //        return false;
        //    }
        //    // 既にファイルが存在している場合は削除する
        //    if (File.Exists(exportfileFullPath))
        //    {
        //        File.Delete(exportfileFullPath);
        //    }
        //    try
        //    {
        //        var stb = new StringBuilder();
        //        var quoteFormat = string.Concat("\"", "{0}", "\"");

        //        Encoding enc = Encoding.GetEncoding("shift_jis");
        //        // 出力用ファイルを生成する
        //        using (StreamWriter srw = new StreamWriter(exportfileFullPath, false, enc))
        //        {
        //            // タイトル行が指定されているときは、タイトル行をセットする
        //            if (titleRows != null && titleRows.Count > 0)
        //            {
        //                for (int i = 0; i < titleRows.Count; i++)
        //                {
        //                    // タイトル行は全列クォートで括る
        //                    stb.Append(",");
        //                    stb.Append(string.Format(quoteFormat, titleRows[i]));
        //                }
        //                // 書き込み
        //                srw.WriteLine(stb.ToString().Substring(1));
        //            }

        //            for (int r = 0; r < dt.Rows.Count; r++)
        //            {
        //                stb.Length = 0;

        //                for (int c = 0; c < dt.Columns.Count; c++)
        //                {
        //                    stb.Append(",");
        //                    // クォートで括る指定のされている列は、クォートで括る
        //                    if (quoteColumns != null && quoteColumns.Contains(c))
        //                    {
        //                        stb.Append(string.Format(quoteFormat, NullToBlank(dt.Rows[r][c])));
        //                    }
        //                    else
        //                    {
        //                        stb.Append(NullToBlank(dt.Rows[r][c]));
        //                    }
        //                }
        //                // 書き込み
        //                srw.WriteLine(stb.ToString().Substring(1));
        //            }
        //            srw.Flush();
        //        }
        //    }
        //    catch
        //    {
        //        // 失敗したときは出力用ファイルを削除する
        //        if (File.Exists(exportfileFullPath))
        //        {
        //            File.Delete(exportfileFullPath);
        //        }

        //        throw;
        //    }

        //    return true;
        //}
        ////CSV出力ここまで==========================================================
        //public static List<string> HederListCreate(string kubun)
        //{
        //    List<string> hederList = new List<string>();

        //    var model = new CommonModel();

        //    switch (kubun)
        //    {
        //        case "DeliveryingData":
        //            hederList.Add("納入指示日");
        //            hederList.Add("便");
        //            hederList.Add("伝票番号1");
        //            hederList.Add("伝票番号2");
        //            hederList.Add("伝票番号3");
        //            hederList.Add("伝票行番号");
        //            hederList.Add("取引先コード");
        //            hederList.Add("取引先名");
        //            hederList.Add("商品コード");
        //            hederList.Add("取引先商品コード");
        //            hederList.Add("商品名");
        //            hederList.Add("納入先コード");
        //            hederList.Add("納入先名");
        //            hederList.Add("納入先詳細");
        //            hederList.Add("取消");
        //            hederList.Add("箱種");
        //            hederList.Add("指示箱数");
        //            hederList.Add("指示ロット数");
        //            hederList.Add("指示数量");
        //            hederList.Add("入庫数");
        //            hederList.Add("出庫数");
        //            hederList.Add("倉庫コード");
        //            break;//switch文を抜ける
        //        case "DeliveryingDetailsData":
        //            hederList.Add("伝票番号1");
        //            hederList.Add("伝票番号2");
        //            hederList.Add("伝票番号3");
        //            hederList.Add("伝票行番号");
        //            hederList.Add("SEQ");
        //            hederList.Add("入庫日時");
        //            hederList.Add("入庫数");
        //            hederList.Add("入庫ユーザ");
        //            hederList.Add("代表キー");
        //            hederList.Add("出庫日時");
        //            hederList.Add("出庫数");
        //            hederList.Add("車両No.");
        //            hederList.Add("出庫ユーザ");
        //            hederList.Add("出庫伝票番号1");
        //            hederList.Add("出庫伝票番号2");
        //            break;//switch文を抜ける
        //        case "DeliveryingDetailsInputData":
        //            hederList.Add("伝票番号1");
        //            hederList.Add("伝票番号2");
        //            hederList.Add("伝票番号3");
        //            hederList.Add("伝票行番号");
        //            hederList.Add("SEQ");
        //            hederList.Add("入庫日時");
        //            hederList.Add("入庫数");
        //            hederList.Add("入庫ハンディユーザー");
        //            break;//switch文を抜ける
        //        case "HandyData":
        //            hederList.Add("ハンディユーザ");
        //            hederList.Add("読取日時");
        //            hederList.Add("送信日時");
        //            hederList.Add("読取画面");
        //            hederList.Add("バーコード情報");
        //            break;//switch文を抜ける
        //        case "StockManegement":
        //            hederList.Add("取引先コード");
        //            hederList.Add("取引先名");
        //            hederList.Add("商品コード");
        //            hederList.Add("取引先商品コード");
        //            hederList.Add("商品名");
        //            hederList.Add("全倉庫在庫数");
        //            hederList.Add("倉庫名");
        //            break;//switch文を抜ける
        //        case "StockDetailsData":
        //            hederList.Add("日付");
        //            //hederList.Add("指示数");
        //            hederList.Add("在庫数");
        //            hederList.Add("入庫数");
        //            hederList.Add("出庫数");
        //            hederList.Add("不良数");
        //            hederList.Add("有効在庫数");
        //            break;//switch文を抜ける
        //                  //default://上記のどれにも当てはまらない
        //                  //        //SQLSERVERへの更新
        //                  //    Other_UPDATE(csvdt, Delete_Flg, Table_name, Delete_Field_Keyname, ref msg);
        //                  //    //更にASへ更新する場合
        //                  //    if (exportno2 == 9000)
        //                  //    {//仕訳をASにも取り込む
        //                  //        SHIWAKE_UPDATE(csvdt);
        //                  //    }
        //                  //    break;//switch文を抜ける
        //    }
        //    return hederList;
        //}
        ////ヘッダーリスト作成ここまで===================================================
        //public static string HidukeHenkan(string kubun, string kubun2, DateTime ima)
        //{
        //    //CultureInfoを日本語 - 日本で作成
        //    CultureInfo ci = new CultureInfo("ja-JP", false);
        //    ci.DateTimeFormat.Calendar = new System.Globalization.JapaneseCalendar();

        //    string hiduke = "";

        //    switch (kubun)
        //    {
        //        case "1": //yyyyy/MM/dd形式の日付を返す
        //            switch (kubun2)
        //            {
        //                case "1":
        //                    //スラッシュを年月日に変更する
        //                    hiduke = ima.ToString("yyyy年MM月dd日");
        //                    break;//switch文を抜ける

        //                case "2":
        //                    hiduke = ima.ToString("yyyy/MM/dd");
        //                    break;//switch文を抜ける

        //                case "3":
        //                    //スラッシュなし
        //                    hiduke = ima.ToString("yyyyMMdd");
        //                    break;//switch文を抜ける
        //            }

        //            break;
        //        case "2": //yyyyy/MM/dd hh:mm:ss形式の日付を返す
        //            if (kubun2 == "1")
        //            {
        //                //スラッシュを年月日に変更する
        //                hiduke = ima.ToString("yyyy年MM月dd日 HH時mm分ss秒");
        //            }
        //            else
        //            {

        //                hiduke = ima.ToString("yyyy/MM/dd HH:mm:ss");
        //            }

        //            break;
        //        case "3": //和暦ggyy/MM/dd形式の日付を返す
        //            if (kubun2 == "1")
        //            {
        //                //スラッシュを年月日に変更する
        //                hiduke = ima.ToString("ggyy年MM月dd日", ci);
        //            }
        //            else
        //            {

        //                hiduke = ima.ToString("ggyy/MM/dd", ci);
        //            }

        //            break;
        //        case "4": //和暦RやH形式の日付を返す
        //            if (kubun2 == "1")
        //            {
        //                //スラッシュを年月日に変更する
        //                hiduke = ima.ToString("ggyy年MM月dd日", ci);
        //            }
        //            else
        //            {

        //                hiduke = ima.ToString("ggyy/MM/dd", ci);
        //            }

        //            hiduke = hiduke.Replace("令和", "R");
        //            hiduke = hiduke.Replace("平成", "H");
        //            hiduke = hiduke.Replace("昭和", "S");
        //            hiduke = hiduke.Replace("大正", "T");
        //            hiduke = hiduke.Replace("明治", "M");
        //            break;
        //    }
        //    return hiduke;
        //}
        ////日付の変換ここまで====================================================

        ///// <summary>
        ///// 日付に対して曜日を作成します
        ///// <param name="hiduke">Datetime型の日付を指定</param>
        ///// <param name="kubun">曜日を短縮するかしないか1＝月曜日、2＝月</param>
        ///// </summary>
        //public static string Youbi(DateTime hiduke, string kubun)
        //{ //曜日を返す
        //    string stryoubi = "";
        //    if (kubun == "1")
        //    {
        //        stryoubi = hiduke.ToString("dddd");
        //    }
        //    else
        //    {
        //        stryoubi = hiduke.ToString("ddd");
        //    }
        //    return stryoubi;
        //}
        ////曜日取得ここまで=======================================================

        ///// <summary>
        ///// 月末日付を作成します
        ///// <param name="hiduke">Datetime型の日付を指定</param>
        ///// </summary>
        //public static DateTime DayLast(DateTime hiduke)
        //{
        //    DateTime getsumatsu;

        //    //月初日をセット
        //    if (hiduke == null)
        //    {//現在日付で取得する場合
        //        hiduke = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    }

        //    //月末日の取得
        //    getsumatsu = hiduke.AddMonths(1).AddDays(-1);

        //    return getsumatsu;
        //}
        ////月末日付を取得するここまで=================================================

        ///// <summary>
        ///// 月初日付を作成します
        ///// <param name="hiduke">Datetime型の日付を指定</param>
        ///// </summary>
        //public static DateTime DayFirst(DateTime hiduke)
        //{
        //    DateTime fairstday;

        //    if (hiduke == null)
        //    {//現在日付で取得する場合
        //        fairstday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    }
        //    else
        //    {
        //        //月初日をセット
        //        fairstday = new DateTime(hiduke.Year, hiduke.Month, 1);
        //    }

        //    return fairstday;
        //}
        ////月初日付を取得するここまで================================================
        //public static string Escape(string data)
        //{
        //    if (!string.IsNullOrEmpty(data))
        //    {
        //        data = data.Replace("'", "''");
        //    }
        //    else
        //    {
        //        data = "";
        //    }
        //    return data;
        //}
        //public static void sendGmail(String mailTo, String Contents)
        //{
        //    try
        //    {
        //        try
        //        {
        //            ServicePointManager.ServerCertificateValidationCallback = delegate (object objectSender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //            { return true; };
        //            ServicePointManager.SecurityProtocol =
        //                 SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        //            SmtpClient client = new SmtpClient();
        //            client.Port = Convert.ToInt32("587");
        //            client.Host = "smtp.gmail.com";
        //            if ("true".Equals(false))
        //            {
        //                client.EnableSsl = true;
        //                //client.UseDefaultCredentials = true;
        //                client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //            }
        //            else
        //            {
        //                client.EnableSsl = true;
        //                client.UseDefaultCredentials = false;
        //                client.Credentials = new NetworkCredential("khacdungboss95@gmail.com", "Dung0964946216");
        //                client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //            }
        //            //var j = i;
        //            //Thread oThreadone = new Thread(() => SendMail(item, cauhinh, client));
        //            SendMail(mailTo, Contents, client);
        //            //oThreadone.Start();
        //            //threads.Add(oThreadone);
        //            //if (i != 0 && i % (numOfThreads - 1) == 0)
        //            //{
        //            //    threads.WaitAll();
        //            //}
        //            //if (i == (ltsNhatKy.Count - 1))
        //            //{
        //            //    threads.WaitAll();
        //            //}
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            // Logging(ex);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Logging(ex);
        //    }
        //    Console.WriteLine("OK");

        //}
        //public static void SendMail(String mailTo, String noiDung, SmtpClient client)
        //{
        //    try
        //    {
        //        MailMessage mail = new MailMessage();
        //        client.EnableSsl = true;
        //        mail.From = new MailAddress("khacdungboss95@gmail.com", "お知らせ");
        //        mail.Subject = "東山株式会社お知らせ。";
        //        mail.IsBodyHtml = true;
        //        mail.To.Add(new MailAddress(mailTo));
        //        mail.Body = noiDung;
        //        //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        //        client.Send(mail);
        //    }
        //    catch (SmtpFailedRecipientsException ex)
        //    {
        //        string dsMaiErrow = "";
        //    }
        //    catch (SmtpFailedRecipientException ex)
        //    {
        //        string dsMaiErrow = ex.FailedRecipient;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message, ex);
        //    }
        //}
        //public static string GetDataDB(SqlDataReader reader, string column)
        //{
        //    if (reader[column] != null)
        //    {
        //        return reader[column].ToString();
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// ModelからDictionaryを生成
        /// </summary>
        public static Dictionary<string, string> ToDic<T>(this T model, bool includeNULL = false)
        {
            // 参考：https://papemk2.hateblo.jp/entry/2018/09/17/104848
            
            if (includeNULL)
            {
                var dictionary = model.GetType()
                    .GetProperties()
                    //.Where(t => t.GetValue(model, null) != null)
                    .Select(i => (i.Name, i.GetValue(model, null)?.ToString()))
                    .ToDictionary(x => x.Item1, x => x.Item2);
                return dictionary;
            }
            else
            {
                var dictionary = model.GetType()
                    .GetProperties()
                    .Where(t => t.GetValue(model, null) != null) // 値がnullの場合は、Dictionaryに含めない 
                    .Select(i => (i.Name, i.GetValue(model, null)?.ToString()))
                    .ToDictionary(x => x.Item1, x => x.Item2);
                return dictionary;
            }

        }

        /// <summary>
        /// ディープコピー（参照先のオブジェクトも複製）
        /// </summary>
        public static T DeepClone<T>(this T src)
        {
            // 参考：https://atmarkit.itmedia.co.jp/ait/articles/1705/24/news040.html
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var formatter = MsgPack.Serialization.MessagePackSerializer.Get<T>();
                formatter.Pack(memoryStream, src); // シリアライズ
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

                return formatter.Unpack(memoryStream); // デシリアライズ
            }
        }
    }

}