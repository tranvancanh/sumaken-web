using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using stock_management_system.common;
using stock_management_system.Models;
using stock_management_system.Models.common;

namespace stock_management_system.Controllers
{
    public class D_ShipmentImportController : BaseController
    {
        public string[] SupportedTypes = new[] { ".xls", ".xlsx", ".csv" };

        //public int DuplicationCheckType = 0;

        // GET: D_ShipmentImport
        public IActionResult Index(bool isUpdate = false)
        {
            var viewModel = new D_ShipmentImportViewModel();

            if (isUpdate && TempData["Message"] != null)
            {
                var viewData = TempData["Message"].ToString();
                ViewData["Message"] = viewData;
            }

            string db = UserDataList().DatabaseName;
            viewModel.ImportedDatas = GetImportedDataList(db);

            return View(viewModel);
        }

        private List<ImportedData> GetImportedDataList(string db)
        {
            var importedDatas = new List<ImportedData>();

            // 直近1か月間の取込履歴を取得
            DateTime searchStartShipmentDate = DateTime.Now.AddMonths(-1);

            try
            {
                // データベースから取得
                using (var connection = new SqlConnection(new GetConnectString(db).ConnectionString))
                {
                    connection.Open();
                    try
                    {
                        string selectString = string.Empty;
                        selectString = $@"
                                            SELECT
                                                 M_Depo.DepoName
                                                ,ImportFileName
                                                ,COUNT(*) AS DataCount
                                                ,(SELECT
	                                                TOP(1) ImportLogHandyScanDate
                                                  FROM D_Shipment AS B
                                                  WHERE (1=1)
                                                    AND A.ImportFileName = B.ImportFileName
                                                    AND B.ShipmentDate >= @SearchStartShipmentDate
                                                    AND B.DeleteFlag = @DeleteFlag
                                                  ORDER BY ShipmentID DESC, ImportLogHandyScanDate ASC
                                                  ) AS FirstImportLogHandyScanDate
                                                ,(SELECT
	                                                TOP(1) ImportLogHandyScanDate
                                                  FROM D_Shipment AS B
                                                  WHERE (1=1)
                                                    AND A.ImportFileName = B.ImportFileName
                                                    AND B.ShipmentDate >= @SearchStartShipmentDate
                                                    AND B.DeleteFlag = @DeleteFlag
                                                  ORDER BY ShipmentID DESC, ImportLogHandyScanDate DESC
                                                  ) AS LastImportLogHandyScanDate
                                            FROM D_Shipment AS A
                                            LEFT OUTER JOIN M_Depo ON A.DepoID = M_Depo.DepoID
                                            WHERE (1=1)
	                                            AND A.ShipmentDate >= @SearchStartShipmentDate
	                                            AND A.DeleteFlag = @DeleteFlag
                                            GROUP BY M_Depo.DepoName, ImportFileName
                                            ORDER BY FirstImportLogHandyScanDate DESC
                                        ";
                        var param = new
                        {
                            SearchStartShipmentDate = searchStartShipmentDate,
                            DeleteFlag = false
                        };
                        importedDatas = connection.Query<ImportedData>(selectString, param).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw new CustomExtention("取込済データの取得に失敗しました。");
                    }
                }
                return importedDatas;

            }
            catch (Exception ex)
            {
                throw new CustomExtention("取込済データの取得に失敗しました。");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(D_ShipmentImportViewModel model)
        {
            var userData = UserDataList();
            bool importingFlag = false;

            try
            {

                if (model.PostedFile == null)
                {
                    throw new CustomExtention("ファイルが選択されていません。");
                }

                var file = model.PostedFile;
                var fileName = file.FileName;

                string db = userData.DatabaseName;

                var commonReturnLabel = " ファイル名 『" + fileName + "』";

                try
                {
                    // 二重処理を防ぐための処理フラグ

                    // 処理開始フラグON
                    var processGet = ProcessModel.ProcessGet(userData, (int)Enums.ProcessID.ShipmentImport);

                    // 《処理フラグがONではない》or《前回の処理から３０分以上経過》していたらOK
                    if (processGet.processFlag == false || processGet.startDate.AddMinutes(30) < DateTime.Now)
                    {
                        // 処理開始フラグON
                        var processSet = ProcessModel.ProcessSet(userData, (int)Enums.ProcessID.ShipmentImport);
                    }
                    else
                    {
                        importingFlag = true;
                        throw new CustomExtention("現在取込処理中のため二重実行できません。");
                    }

                    // ファイル拡張子チェック
                    var checkFileExtension = CheckFileExtension(file);
                    if (!checkFileExtension.result)
                    {
                        throw new CustomExtention(checkFileExtension.message);
                    }

                    // ファイルをWebサーバーにコピー

                    var saveFileName = String.Concat(
                        DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        "_",
                        fileName
                        );
                    var rootPath = Directory.GetCurrentDirectory();
                    var folderPath = Path.Combine(rootPath, @"wwwroot\uploadedfiles\shipment");
                    var filePath = Path.Combine(folderPath, saveFileName);

                    // WebサーバーのUpLoadフォルダーがない場合は作成
                    if (Directory.Exists(folderPath) == false)
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // ファイルコピー
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // 出荷実績取込パターンを取得
                    var shipmentImportPattern = M_ShipmentImportPattern.GetShipmentImportPattern(db, model.PatternID);
                    if (shipmentImportPattern == null)
                    {
                        throw new CustomExtention("取込パターンマスタの取得に失敗しました。");
                    }
                    else if (shipmentImportPattern.ColumnIndexProductCode == 0)
                    {
                        throw new CustomExtention("取込パターンマスタの商品コード列が設定されていません。");
                    }
                    else if (shipmentImportPattern.ColumnIndexQuantity == 0)
                    {
                        throw new CustomExtention("取込パターンマスタの数量列が設定されていません。");
                    }

                    // ファイルからデータ取得
                    var getListresult = GetList(model.PostedFile, shipmentImportPattern);
                    if (!getListresult.result)
                    {
                        throw new CustomExtention(getListresult.message);
                    }

                    // 重複チェック
                    var insertShipmentList = new List<D_ShipmentImportModel>();
                    if (shipmentImportPattern.DuplicationCheckType == Enums.ShipmentImportDuplicationCheckType.None)
                    {
                        insertShipmentList = getListresult.list;
                    }
                    else
                    {
                        insertShipmentList = DuplicationCheck(getListresult.list, shipmentImportPattern.DuplicationCheckType);
                    }

                    // 登録
                    var insertResult = Insert(fileName, insertShipmentList, shipmentImportPattern.IsStockOut, shipmentImportPattern.DepoID);
                    if (!insertResult)
                    {
                        throw new CustomExtention("取込に失敗しました。");
                    }

                    ModelState.Clear();
                    model = new D_ShipmentImportViewModel();

                    TempData["Message"] = "取込がすべて完了しました。" + commonReturnLabel;
                    return RedirectToAction("Index", new { isUpdate = true });

                }
                catch (CustomExtention ex)
                {
                    ViewData["Error"] = ex.Message + commonReturnLabel;
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = "取込に失敗しました。" + commonReturnLabel;
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            finally
            {
                if (!importingFlag)
                {
                    // 処理開始フラグOFF
                    var processEnd = ProcessModel.ProcessEnd(userData, (int)Enums.ProcessID.ShipmentImport);
                }
            }

            // 更新したデータを取込履歴一覧に反映させるため再取得
            model.ImportedDatas = GetImportedDataList(userData.DatabaseName);

            return View(model);
        }

        private (bool result, string message) CheckFileExtension(IFormFile file)
        {
            bool result = false;
            string message = "";

            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (SupportedTypes.Contains(extension))
            {
                // OK
                result = true;
            }
            else
            {
                message = "拡張子 " + extension + " は取り込みの対象外です。";
            }

            return (result, message);

        }

        private (bool result, string message, List<D_ShipmentImportModel> list) GetList(IFormFile file, M_ShipmentImportPattern shipmentImportPattern)
        {
            var shipmentImportModels = new List<D_ShipmentImportModel>();

            bool result = false;
            string message = "";

            try
            {
                var fileName = file.FileName;
                var companyId = UserDataList().CompanyID;

                // ファイルの読み取り開始
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                IExcelDataReader reader;

                //ファイルの拡張子を確認
                if (fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx"))
                {
                    // デフォルトのエンコードは西ヨーロッパ言語の為、日本語が文字化けする
                    // オプション設定でエンコードをシフトJISに変更する
                    using (reader = ExcelReaderFactory.CreateReader(file.OpenReadStream(), new ExcelReaderConfiguration() { FallbackEncoding = Encoding.GetEncoding("Shift_JIS") }))
                    {
                        shipmentImportModels = GetReaderDataList(companyId, reader, shipmentImportPattern);
                    }
                }
                else if (fileName.EndsWith(".csv"))
                {
                    // デフォルトのエンコードは西ヨーロッパ言語の為、日本語が文字化けする
                    // オプション設定でエンコードをシフトJISに変更する
                    using (reader = ExcelReaderFactory.CreateCsvReader(file.OpenReadStream(), new ExcelReaderConfiguration() { FallbackEncoding = Encoding.GetEncoding("Shift_JIS") }))
                    {
                        shipmentImportModels = GetReaderDataList(companyId, reader, shipmentImportPattern);
                    }
                }
                else
                {
                    message = "サポート対象外の拡張子です。";

                    return (result, message, shipmentImportModels);
                }

                if (reader != null) { reader.Close(); }

                if (shipmentImportModels != null && shipmentImportModels.Count > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    message = "データが０件です。";
                }

                return (result, message, shipmentImportModels);

            }
            catch (Exception ex)
            {
                throw;
            }

        }


        private List<D_ShipmentImportModel> GetReaderDataList(int companyId, IExcelDataReader reader, M_ShipmentImportPattern pattern)
        {
            var productLabelList = new List<D_ShipmentImportModel>();

            try
            {

                int rowCount = 0;

                // 1行毎に情報を取得
                while (reader.Read())
                {
                    rowCount++;

                    if (rowCount < pattern.RowStartNumber)
                    {
                        continue;
                    }

                    var shipmentImportModel = new D_ShipmentImportModel();

                    // 読取終了合図
                    //bool endFlg = false;

                    if (pattern.MatchString1ColumnNumber > 0)
                    {
                        var checkString = Util.ObjectToString(reader.GetValue(pattern.MatchString1ColumnNumber - 1));
                        var matchStringErrorFlag1 = pattern.MatchString1ErrorFlag;

                        bool notMatching = true;
                        if ((!String.IsNullOrEmpty(pattern.MatchString1_1) && pattern.MatchString1_1 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString1_2) && pattern.MatchString1_2 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString1_3) && pattern.MatchString1_3 == checkString))
                        {
                            notMatching = false;
                        }

                        if (notMatching)
                        {
                            if (matchStringErrorFlag1)
                            {
                                // Errorを出し、取込中止する
                                throw new CustomExtention("チェック文字列の対象ではありません。");
                            }
                            else
                            {
                                // Errorを出さず、スキップするだけ
                                continue;
                            }
                        }
                    }

                    if (pattern.MatchString2ColumnNumber > 0)
                    {
                        var checkString = Util.ObjectToString(reader.GetValue(pattern.MatchString2ColumnNumber - 1));
                        var matchStringErrorFlag2 = pattern.MatchString2ErrorFlag;

                        bool notMatching = true;
                        if ((!String.IsNullOrEmpty(pattern.MatchString2_1) && pattern.MatchString2_1 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString2_2) && pattern.MatchString2_2 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString2_3) && pattern.MatchString2_3 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString2_4) && pattern.MatchString2_4 == checkString))
                        {
                            notMatching = false;
                        }
                        else if ((!String.IsNullOrEmpty(pattern.MatchString2_5) && pattern.MatchString2_5 == checkString))
                        {
                            notMatching = false;
                        }

                        if (notMatching)
                        {
                            if (matchStringErrorFlag2)
                            {
                                // Errorを出し、取込中止する
                                throw new CustomExtention("チェック文字列の対象ではありません。");
                            }
                            else
                            {
                                // Errorを出さず、スキップするだけ
                                continue;
                            }
                        }
                    }

                    for (int colCount = 1; colCount <= reader.FieldCount; colCount++)
                    {

                        var obj = reader.GetValue(colCount - 1);

                        var row = (rowCount).ToString();
                        var col = (colCount).ToString();
                        var position = row + "行目" + col + "列目：";

                        var errorHeadLabel = position;

                        // 1行目だけの処理
                        if (colCount == 1)
                        {
                            // 固定箱数の設定があれば挿入
                            if (pattern.FixedValuePackingCount > 0)
                            {
                                shipmentImportModel.PackingCount = pattern.FixedValuePackingCount;
                            }

                            if (pattern.RowEndSignFlag)
                            {
                                // 読取終了合図有りの場合
                                // 1行目の値に指定文字が含まれていたら終了
                                if (Util.ObjectToString(obj).Contains(pattern.RowEndSignString))
                                {
                                    //endFlg = true;
                                    break;
                                }
                            }
                            else
                            {
                                // 読取終了合図無しの場合
                                // 1行目の値が空白なら終了
                                if (Util.ObjectToString(obj) == "")
                                {
                                    //endFlg = true;
                                    break;
                                }
                            }
                        }

                        try
                        {
                            // colCount=列番号

                            if (colCount == pattern.ColumnIndexHandyMatchClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.HandyMatchClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexHandyMatchResult)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.HandyMatchResult = value;
                            }
                            else if (colCount == pattern.ColumnIndexShipmentDate)
                            {
                                var value = Util.ObjectToDateTime(obj);
                                shipmentImportModel.ShipmentDate = value;
                            }
                            else if (colCount == pattern.ColumnIndexDeliveryDate)
                            {
                                var value = Util.ObjectToDateTimeExNull(obj);
                                shipmentImportModel.DeliveryDate = value;
                            }
                            else if (colCount == pattern.ColumnIndexDeliveryTimeClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.DeliveryTimeClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexDeliverySlipNumber)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.DeliverySlipNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexDeliverySlipRowNumber)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.DeliverySlipRowNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexSupplierCode)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.SupplierCode = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerCode)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerCode = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerName)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerName = value;
                            }
                            else if (colCount == pattern.ColumnIndexProductCode)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ProductCode = value;
                            }
                            else if (colCount == pattern.ColumnIndexProductAbbreviation)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ProductAbbreviation = value;
                            }
                            else if (colCount == pattern.ColumnIndexProductManagementClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ProductManagementClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexProductLabelBranchNumber)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.ProductLabelBranchNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexNextProcess1)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.NextProcess1 = value;
                            }
                            else if (colCount == pattern.ColumnIndexLocation1)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.Location1 = value;
                            }
                            else if (colCount == pattern.ColumnIndexNextProcess2)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.NextProcess2 = value;
                            }
                            else if (colCount == pattern.ColumnIndexLocation2)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.Location2 = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerProductCode)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerProductCode = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerProductAbbreviation)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerProductAbbreviation = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerProductManagementClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerProductManagementClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerProductLabelBranchNumber)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.CustomerProductLabelBranchNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerNextProcess1)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerNextProcess1 = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerLocation1)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerLocation1 = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerNextProcess2)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerNextProcess2 = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerLocation2)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerLocation2 = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerDeliveryDate)
                            {
                                var value = Util.ObjectToDateTimeExNull(obj);
                                shipmentImportModel.CustomerDeliveryDate = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerDeliveryTimeClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerDeliveryTimeClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerDeliverySlipNumber)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerDeliverySlipNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerDeliverySlipRowNumber)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.CustomerDeliverySlipRowNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerOrderNumber)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerOrderNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexCustomerOrderClass)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.CustomerOrderClass = value;
                            }
                            else if (colCount == pattern.ColumnIndexLotQuantity)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.LotQuantity = value;
                            }
                            else if (colCount == pattern.ColumnIndexFractionQuantity)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.FractionQuantity = value;
                            }
                            else if (colCount == pattern.ColumnIndexQuantity)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.Quantity = value;
                            }
                            else if (colCount == pattern.ColumnIndexPacking)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.Packing = value;
                            }
                            else if (colCount == pattern.ColumnIndexPackingCount)
                            {
                                var value = Util.ObjectToInt(obj);
                                shipmentImportModel.PackingCount = value;
                            }
                            else if (colCount == pattern.ColumnIndexLotNumber)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.LotNumber = value;
                            }
                            else if (colCount == pattern.ColumnIndexRemark)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.Remark = value;
                            }
                            else if (colCount == pattern.ColumnIndexHandyUserCode)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ImportLogHandyUserCode = value;
                            }
                            else if (colCount == pattern.ColumnIndexHandyUserName)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ImportLogHandyUserName = value;
                            }
                            else if (colCount == pattern.ColumnIndexHandyScanDate)
                            {
                                var value = Util.ObjectToDateTimeExNull(obj);
                                shipmentImportModel.ImportLogHandyScanDate = value;
                            }
                            else if (colCount == pattern.ColumnIndexHandyScanTime)
                            {
                                var value = Util.ObjectToString(obj);
                                shipmentImportModel.ImportLogHandyScanTime = value;
                            }

                        }
                        catch (CustomExtention ex)
                        {
                            if (!String.IsNullOrEmpty(errorHeadLabel))
                            {
                                throw new CustomExtention(errorHeadLabel + ex.Message);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }

                    }

                    // 企業ごとのカスタマイズ
                    #region テクノエイト（酒倉デポ・桐井デポ）
                    if (companyId == (int)Enums.UserCompanyId.test || companyId == (int)Enums.UserCompanyId.technoleight)
                    {
                        #region 集荷実績の場合
                        if (pattern.ShipmentImportPatternID == 4) // 集荷実績取込なら
                        {
                            // 「数量」と同じ値を「ロット数」にもセット
                            shipmentImportModel.LotQuantity = shipmentImportModel.Quantity;

                            // 商品コードが空白であれば、102列目の品番を12文字で切り取ってセット
                            // 社内管理品番が空白の場合の対応
                            if (shipmentImportModel.ProductCode.Length == 0)
                            {
                                var subObj = reader.GetValue(102 - 1);
                                var subValue = Util.ObjectToString(subObj);
                                subValue = subValue.Substring(0, 12);
                                shipmentImportModel.ProductCode = Util.ObjectToString(subValue);
                            }

                            // 集荷日を、ハンディスキャン日付にセット
                            shipmentImportModel.ImportLogHandyScanDate = shipmentImportModel.ShipmentDate;
                        }
                        #endregion
                        #region 出荷実績の場合
                        else if (pattern.ShipmentImportPatternID == 5) // 出荷実績取込なら
                        {
                            // 「出荷日」と同じ値を「納入日」にもセット
                            shipmentImportModel.DeliveryDate = shipmentImportModel.ShipmentDate;

                            // 商品コードが空白であれば、3列目の品番を12文字で切り取ってセット
                            // 社内管理品番が空白の場合の対応
                            if (shipmentImportModel.ProductCode.Length == 0)
                            {
                                var subObj = reader.GetValue(3 - 1);
                                var subValue = Util.ObjectToString(subObj);
                                subValue = subValue.Substring(0, 12);
                                shipmentImportModel.ProductCode = Util.ObjectToString(subValue);
                            }
                        }
                        #endregion
                    }
                    #endregion

                    // スキャン日付と時間から、スキャン日時を作成
                    if (shipmentImportModel.ImportLogHandyScanDate != Convert.ToDateTime("1900/01/01")) // スキャン日付に値が入っていたら
                    {
                        if (shipmentImportModel.ImportLogHandyScanTime.Length > 0) // スキャン時間に値が入っていたら
                        {
                            // スキャン日付と時間を合体
                            var importLogHandyScanDateTime = Util.ObjectToDateTimeByTimeEx(shipmentImportModel.ImportLogHandyScanDate, shipmentImportModel.ImportLogHandyScanTime);
                            shipmentImportModel.ImportLogHandyScanDateAndTime = importLogHandyScanDateTime;
                        }
                        else
                        {
                            shipmentImportModel.ImportLogHandyScanDateAndTime = shipmentImportModel.ImportLogHandyScanDate;
                        }
                    }

                    // 必須項目が取得できているかのチェック
                    CheckStringRequired("商品コード", shipmentImportModel.ProductCode);
                    CheckStringRequired("数量", shipmentImportModel.Quantity.ToString());

                    // OK！格納
                    productLabelList.Add(shipmentImportModel);

                }

            }
            catch (Exception ex)
            {
                throw;
            }

            return productLabelList;

        }

        private List<D_ShipmentImportModel> DuplicationCheck(List<D_ShipmentImportModel> shipmentImportModels, Enums.ShipmentImportDuplicationCheckType checkType)
        {
            List<D_ShipmentImportModel> list = new List<D_ShipmentImportModel>(shipmentImportModels);

            string db = UserDataList().DatabaseName;

            try
            {
                var shipmentDateGroupData = shipmentImportModels.GroupBy(x => x.ShipmentDate).ToList();

                for (int i = 0; i < shipmentDateGroupData.Count; ++i)
                {
                    var shipmentDate = shipmentDateGroupData[i].Key;

                    var connectionString = new GetConnectString(db).ConnectionString;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string selectShipmentString = $@"
                                                    SELECT
                                                        CustomerDeliveryDate,
                                                        CustomerDeliverySlipNumber,
                                                        ProductCode,
                                                        Quantity,
                                                        ProductLabelBranchNumber,
                                                        CustomerProductLabelBranchNumber
                                                    FROM D_Shipment
                                                    WHERE (1=1)
                                                        AND ShipmentDate = @ShipmentDate
                                                    ;";

                        var selectParam = new
                        {
                            ShipmentDate = shipmentDate
                        };

                        var selectShipment = connection.Query<ShipmentDuplicationCheckData>(selectShipmentString, selectParam);

                        if (selectShipment.Count() == 0)
                        {
                            continue;
                        }

                        foreach (var item1 in selectShipment)
                        {
                            foreach (var item2 in shipmentImportModels)
                            {
                                if (item1.CustomerDeliveryDate == item2.CustomerDeliveryDate
                                    && item1.CustomerDeliverySlipNumber == item2.CustomerDeliverySlipNumber
                                    && item1.CustomerProductLabelBranchNumber == item2.CustomerProductLabelBranchNumber
                                    && item1.ProductCode == item2.ProductCode
                                    && item1.Quantity == item2.Quantity
                                    && item1.ProductLabelBranchNumber == item2.ProductLabelBranchNumber)
                                {

                                    // 既に出荷登録がある場合の動作
                                    if (checkType == Enums.ShipmentImportDuplicationCheckType.Error) // エラーを出して取り込みを中止する
                                    {
                                        var errorDataDetail = item2.CustomerDeliveryDate.ToString("yyyy/MM/dd")
                                            + "," + item2.CustomerDeliverySlipNumber
                                            + "," + item2.ProductCode
                                            + "," + item2.Quantity
                                            + "," + item2.ProductLabelBranchNumber
                                            + "," + item2.CustomerProductLabelBranchNumber;
                                        var message = "重複データが存在するため、取込みを中止します。" + errorDataDetail;
                                        throw new CustomExtention(message);
                                    }
                                    else if (checkType == Enums.ShipmentImportDuplicationCheckType.Skip) // スキップして取り込みを継続する
                                    {
                                        list.Remove(item2);
                                    }
                                }
                            }
                        }

                    }
                }

                //for (int i = 0; i < shipmentImportModels.Count; ++i)
                //{
                //    string selectString = $@"
                //                                SELECT
                //                                    COUNT(*)
                //                                FROM D_Shipment
                //                                WHERE (1=1)
                //                                    AND CustomerDeliveryDate = @CustomerDeliveryDate
                //                                    AND CustomerDeliverySlipNumber = @CustomerDeliverySlipNumber
                //                                    AND ProductCode = @ProductCode
                //                                    AND Quantity = @Quantity
                //                                    AND ProductLabelBranchNumber = @ProductLabelBranchNumber
                //                                    AND CustomerProductLabelBranchNumber = @CustomerProductLabelBranchNumber
                //                                ;";

                //    var selectParam = new
                //    {
                //        shipmentImportModels[i].CustomerDeliveryDate,
                //        shipmentImportModels[i].CustomerDeliverySlipNumber,
                //        shipmentImportModels[i].ProductCode,
                //        shipmentImportModels[i].Quantity,
                //        shipmentImportModels[i].ProductLabelBranchNumber,
                //        shipmentImportModels[i].CustomerProductLabelBranchNumber
                //    };

                //    var selectShipmentCount = connection.QueryFirstOrDefault<int>(selectString, selectParam);

                //    if (selectShipmentCount > 0)
                //    {
                //        // 既に出荷登録がある場合の動作
                //        if (checkType == Enums.ShipmentImportDuplicationCheckType.Error) // エラーを出して取り込みを中止する
                //        {
                //            var errorDataDetail = shipmentImportModels[i].CustomerDeliveryDate.ToString("yyyy/MM/dd")
                //                + "," + shipmentImportModels[i].CustomerDeliverySlipNumber
                //                + "," + shipmentImportModels[i].ProductCode
                //                + "," + shipmentImportModels[i].Quantity
                //                + "," + shipmentImportModels[i].ProductLabelBranchNumber
                //                + "," + shipmentImportModels[i].CustomerProductLabelBranchNumber;
                //            var message = "重複データが存在するため、取込みを中止します。" + errorDataDetail;
                //            throw new CustomExtention(message);
                //        }
                //        else if (checkType == Enums.ShipmentImportDuplicationCheckType.Skip) // スキップして取り込みを継続する
                //        {
                //            list.Remove(shipmentImportModels[i]);
                //        }
                //    }

                //}

                if (list.Count == 0)
                {
                    throw new CustomExtention("取込対象のデータは０件です。");
                }

                return list;
                
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private bool Insert(string fileName, List<D_ShipmentImportModel> shipmentImportModels, bool isStockOut, int depoID)
        {
            var now = DateTime.Now;

            var user = UserDataList();

            long insertStoreOutID = 0;

            // 商品マスタ情報を取得
            var productMasterList = M_ProductModel.GetProduct(user.DatabaseName, depoID);
            var productMaster = new M_ProductModel.M_Product();

            // 正常に１：１で消し込みが行われた、入庫データIDリスト
            var storeInIdList = new List<long>();

            var connectionString = new GetConnectString(user.DatabaseName).ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    #region roop process start !
                    for (int i = 0; i < shipmentImportModels.Count; ++i)
                    {
                        #region tran start !
                        using (var tran = connection.BeginTransaction())
                        {
                            try
                            {
                                /////////////////////////////////////////////////////
                                // 処理の流れ：出庫データ作成→出荷データ作成 //
                                ////////////////////////////////////////////////////

                                // 在庫の出庫データ作成（D_StoreOut）
                                if (isStockOut)
                                {
                                    int targetStoreInID = 0;
                                    string insertStoreOutString = $@"
                                                INSERT D_StoreOut
                                                (
                                                       ShipmentInstructionDetailID
                                                      ,ScanRecordID
                                                      ,StoreInID
                                                      ,DepoID
                                                      ,StoreOutDate
                                                      ,ProductCode
                                                      ,Quantity
                                                      ,Packing
                                                      ,PackingCount
                                                      ,StockLocation1
                                                      ,StockLocation2
                                                      ,AdjustmentFlag
                                                      ,Remark
                                                      ,DeleteFlag
                                                      ,DeleteStoreOutID
                                                      ,CreateDate
                                                      ,CreateUserID
                                                      ,UpdateDate
                                                      ,UpdateUserID
                                                )
                                                OUTPUT 
                                                    INSERTED.StoreOutID
                                                VALUES
                                                (
                                                       @ShipmentInstructionDetailID
                                                      ,@ScanRecordID
                                                      ,@StoreInID
                                                      ,@DepoID
                                                      ,@StoreOutDate
                                                      ,@ProductCode
                                                      ,@Quantity
                                                      ,@Packing
                                                      ,@PackingCount
                                                      ,@StockLocation1
                                                      ,@StockLocation2
                                                      ,@AdjustmentFlag
                                                      ,@Remark
                                                      ,@DeleteFlag
                                                      ,@DeleteStoreOutID
                                                      ,@CreateDate
                                                      ,@CreateUserID
                                                      ,@UpdateDate
                                                      ,@UpdateUserID
                                                 );";

                                    var insertStoreOutParam = new D_StoreOutModel()
                                    {
                                        ScanRecordID = 0,
                                        ShipmentInstructionDetailID = 0,
                                        StoreInID = targetStoreInID,
                                        DepoID = depoID,
                                        StoreOutDate = shipmentImportModels[i].ShipmentDate,
                                        ProductCode = shipmentImportModels[i].ProductCode,
                                        Quantity = shipmentImportModels[i].Quantity,
                                        Packing = shipmentImportModels[i].Packing,
                                        PackingCount = shipmentImportModels[i].PackingCount,
                                        StockLocation1 = shipmentImportModels[i].NextProcess1,
                                        StockLocation2 = shipmentImportModels[i].Location1,
                                        AdjustmentFlag = false,
                                        Remark = shipmentImportModels[i].Remark,
                                        DeleteFlag = shipmentImportModels[i].DeleteFlag,
                                        DeleteStoreOutID = 0,
                                        CreateDate = now,
                                        CreateUserID = user.UserID,
                                        UpdateDate = now,
                                        UpdateUserID = user.UserID
                                    };
                                    insertStoreOutID = connection.QuerySingle<long>(insertStoreOutString, insertStoreOutParam, tran);

                                }

                                // 出荷データ作成（D_Shipment）
                                string insertShipmentString = $@"
                                                INSERT D_Shipment
                                                (
                                                       ScanRecordID
                                                      ,ShipmentInstructionDetailID
                                                      ,StoreOutID
                                                      ,HandyMatchClass
                                                      ,HandyMatchResult
                                                      ,ShipmentDate
                                                      ,DepoID
                                                      ,DeliveryDate
                                                      ,DeliveryTimeClass
                                                      ,DeliverySlipNumber
                                                      ,DeliverySlipRowNumber
                                                      ,SupplierCode
                                                      ,SupplierClass
                                                      ,ProductCode
                                                      ,ProductAbbreviation
                                                      ,ProductManagementClass
                                                      ,ProductLabelBranchNumber
                                                      ,NextProcess1
                                                      ,Location1
                                                      ,NextProcess2
                                                      ,Location2
                                                      ,CustomerCode
                                                      ,CustomerClass
                                                      ,CustomerName
                                                      ,CustomerProductCode
                                                      ,CustomerProductAbbreviation
                                                      ,CustomerProductManagementClass
                                                      ,CustomerProductLabelBranchNumber
                                                      ,CustomerNextProcess1
                                                      ,CustomerLocation1
                                                      ,CustomerNextProcess2
                                                      ,CustomerLocation2
                                                      ,CustomerDeliveryDate
                                                      ,CustomerDeliveryTimeClass
                                                      ,CustomerDeliverySlipNumber
                                                      ,CustomerDeliverySlipRowNumber
                                                      ,CustomerOrderNumber
                                                      ,CustomerOrderClass
                                                      ,LotQuantity
                                                      ,FractionQuantity
                                                      ,Quantity
                                                      ,Packing
                                                      ,PackingCount
                                                      ,LotNumber
                                                      ,InvoiceNumber
                                                      ,ExpirationDate
                                                      ,DeleteFlag
                                                      ,DeleteShipmentID
                                                      ,Remark
                                                      ,ImportFileName
                                                      ,ImportLogHandyUserCode
                                                      ,ImportLogHandyUserName
                                                      ,ImportLogHandyScanDate
                                                      ,CreateDate
                                                      ,CreateUserID
                                                      ,CreateHandyUserID
                                                      ,UpdateDate
                                                      ,UpdateUserID
                                                )
                                                OUTPUT 
                                                   INSERTED.ShipmentID
                                                VALUES
                                                (
                                                       @ScanRecordID
                                                      ,@ShipmentInstructionDetailID
                                                      ,@StoreOutID
                                                      ,@HandyMatchClass
                                                      ,@HandyMatchResult
                                                      ,@ShipmentDate
                                                      ,@DepoID
                                                      ,@DeliveryDate
                                                      ,@DeliveryTimeClass
                                                      ,@DeliverySlipNumber
                                                      ,@DeliverySlipRowNumber
                                                      ,@SupplierCode
                                                      ,@SupplierClass
                                                      ,@ProductCode
                                                      ,@ProductAbbreviation
                                                      ,@ProductManagementClass
                                                      ,@ProductLabelBranchNumber
                                                      ,@NextProcess1
                                                      ,@Location1
                                                      ,@NextProcess2
                                                      ,@Location2
                                                      ,@CustomerCode
                                                      ,@CustomerClass
                                                      ,@CustomerName
                                                      ,@CustomerProductCode
                                                      ,@CustomerProductAbbreviation
                                                      ,@CustomerProductManagementClass
                                                      ,@CustomerProductLabelBranchNumber
                                                      ,@CustomerNextProcess1
                                                      ,@CustomerLocation1
                                                      ,@CustomerNextProcess2
                                                      ,@CustomerLocation2
                                                      ,@CustomerDeliveryDate
                                                      ,@CustomerDeliveryTimeClass
                                                      ,@CustomerDeliverySlipNumber
                                                      ,@CustomerDeliverySlipRowNumber
                                                      ,@CustomerOrderNumber
                                                      ,@CustomerOrderClass
                                                      ,@LotQuantity
                                                      ,@FractionQuantity
                                                      ,@Quantity
                                                      ,@Packing
                                                      ,@PackingCount
                                                      ,@LotNumber
                                                      ,@InvoiceNumber
                                                      ,@ExpirationDate
                                                      ,@DeleteFlag
                                                      ,@DeleteShipmentID
                                                      ,@Remark
                                                      ,@ImportFileName
                                                      ,@ImportLogHandyUserCode
                                                      ,@ImportLogHandyUserName
                                                      ,@ImportLogHandyScanDate
                                                      ,@CreateDate
                                                      ,@CreateUserID
                                                      ,@CreateHandyUserID
                                                      ,@UpdateDate
                                                      ,@UpdateUserID
                                                );";

                                var insertShipmentParam = new D_ShipmentImportModel()
                                {
                                    ScanRecordID = 0,
                                    ShipmentInstructionDetailID = 0,
                                    StoreOutID = insertStoreOutID,
                                    HandyMatchClass = shipmentImportModels[i].HandyMatchClass,
                                    HandyMatchResult = shipmentImportModels[i].HandyMatchResult,
                                    ShipmentDate = shipmentImportModels[i].ShipmentDate,
                                    DepoID = depoID,
                                    DeliveryDate = shipmentImportModels[i].DeliveryDate,
                                    DeliveryTimeClass = shipmentImportModels[i].DeliveryTimeClass,
                                    DeliverySlipNumber = shipmentImportModels[i].DeliverySlipNumber,
                                    DeliverySlipRowNumber = shipmentImportModels[i].DeliverySlipRowNumber,
                                    SupplierCode = shipmentImportModels[i].SupplierCode,
                                    SupplierClass = shipmentImportModels[i].SupplierClass,
                                    ProductCode = shipmentImportModels[i].ProductCode,
                                    ProductAbbreviation = shipmentImportModels[i].ProductAbbreviation,
                                    ProductManagementClass = shipmentImportModels[i].ProductManagementClass,
                                    ProductLabelBranchNumber = shipmentImportModels[i].ProductLabelBranchNumber,
                                    NextProcess1 = shipmentImportModels[i].NextProcess1,
                                    Location1 = shipmentImportModels[i].Location1,
                                    NextProcess2 = shipmentImportModels[i].NextProcess2,
                                    Location2 = shipmentImportModels[i].Location2,
                                    CustomerCode = shipmentImportModels[i].CustomerCode,
                                    CustomerClass = shipmentImportModels[i].CustomerClass,
                                    CustomerName = shipmentImportModels[i].CustomerName,
                                    CustomerProductCode = shipmentImportModels[i].CustomerProductCode,
                                    CustomerProductAbbreviation = shipmentImportModels[i].CustomerProductAbbreviation,
                                    CustomerProductManagementClass = shipmentImportModels[i].CustomerProductManagementClass,
                                    CustomerProductLabelBranchNumber = shipmentImportModels[i].CustomerProductLabelBranchNumber,
                                    CustomerNextProcess1 = shipmentImportModels[i].CustomerNextProcess1,
                                    CustomerLocation1 = shipmentImportModels[i].CustomerLocation1,
                                    CustomerNextProcess2 = shipmentImportModels[i].CustomerNextProcess2,
                                    CustomerLocation2 = shipmentImportModels[i].CustomerLocation2,
                                    CustomerDeliveryDate = shipmentImportModels[i].CustomerDeliveryDate,
                                    CustomerDeliveryTimeClass = shipmentImportModels[i].CustomerDeliveryTimeClass,
                                    CustomerDeliverySlipNumber = shipmentImportModels[i].CustomerDeliverySlipNumber,
                                    CustomerDeliverySlipRowNumber = shipmentImportModels[i].CustomerDeliverySlipRowNumber,
                                    CustomerOrderNumber = shipmentImportModels[i].CustomerOrderNumber,
                                    CustomerOrderClass = shipmentImportModels[i].CustomerOrderClass,
                                    LotQuantity = shipmentImportModels[i].LotQuantity,
                                    FractionQuantity = shipmentImportModels[i].FractionQuantity,
                                    Quantity = shipmentImportModels[i].Quantity,
                                    Packing = shipmentImportModels[i].Packing,
                                    PackingCount = shipmentImportModels[i].PackingCount,
                                    LotNumber = shipmentImportModels[i].LotNumber,
                                    InvoiceNumber = "",
                                    ExpirationDate = "",
                                    DeleteFlag = false,
                                    DeleteShipmentID = 0,
                                    Remark = shipmentImportModels[i].Remark,
                                    ImportFileName = fileName,
                                    ImportLogHandyUserCode = shipmentImportModels[i].ImportLogHandyUserCode,
                                    ImportLogHandyUserName = shipmentImportModels[i].ImportLogHandyUserName,
                                    ImportLogHandyScanDate = shipmentImportModels[i].ImportLogHandyScanDateAndTime,
                                    CreateDate = now,
                                    CreateUserID = user.UserID,
                                    CreateHandyUserID = 0,
                                    UpdateDate = now,
                                    UpdateUserID = user.UserID
                                };
                                var insertShipmentResult = connection.Execute(insertShipmentString, insertShipmentParam, tran);

                                tran.Commit();

                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                throw;
                            }
                        }
                        #endregion tran end !
                    }
                    #endregion roop process end !

                    //try
                    //{
                    //    // 入庫調整テーブルの完了処理
                    //    // 例）強制入庫など、なんらかの理由で同じ枝番のかんばんが存在しており、正常に消し込みが行われた場合
                    //    string updateStoreInAdjustment = $@"
                    //                        MERGE D_StoreInAdjustment AS TargetTable
                    //                        USING (
                    //                            SELECT
                    //                                StoreInID
                    //                            FROM D_StoreOut
                    //                            WHERE DeleteFlag = @DeleteFlag
                    //                        ) AS SourceTable
                    //                            ON TargetTable.DuplicationStoreInID = SourceTable.StoreInID
                    //                            AND TargetTable.EndFlag = 0
                    //                        WHEN MATCHED
                    //                               THEN UPDATE 
                    //                                  SET
                    //                                     TargetTable.EndFlag = @TargetEndFlag,
                    //                                     TargetTable.UpdateDate = @UpdateDate,
                    //                                     TargetTable.UpdateUserID = @UpdateUserID
                    //                            ;";

                    //    var updateStoreInAdjustmentParam = new
                    //    {
                    //        DeleteFlag = 0,
                    //        SourceEndFlag = 0, // 未完了
                    //        TargetEndFlag = 1, // 完了
                    //        UpdateDate = now,
                    //        UpdateUserID = user.UserID
                    //    };

                    //    var updateStoreInAdjustmentResult = connection.Execute(updateStoreInAdjustment, updateStoreInAdjustmentParam);
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new CustomExtention("入庫調整テーブルの完了更新に失敗しました。");
                    //}

                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {

                }

            }

            return true;
        }

        private void CheckStringRequiredErrorMessage(string errorMsgHead)
        {
            throw new CustomExtention(errorMsgHead + "が" + "空白です。");
        }
        private void CheckStringRequired(string errorMsgHead, string value)
        {
            if (value.Length < 1)
            {
                throw new CustomExtention(errorMsgHead + "が" + "空白です。");
            }
        }

    }


}
