using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Tls;
using PCLStorage;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace Mobile_FJ.Objects_and_Classes
{
    public static class Global
    {
        public static bool isOnWifi()
        {
            //CrossConnectivity.Current.ConnectionTypes. Contains(ConnectionType.WiFi);
            return true; 
        }

        public static CodeVerification VerificationPage = new CodeVerification();

        public static readonly string AESGCMKey = "MobilePASSWORD";
        public static readonly string ConfigFileName = "Mobile_FJ.config"; // internal configuration name
        public static readonly string SyncFileName = "_sync.pbf"; // sync file
        public static readonly string FullSyncFileName = "_fsync.pbf"; // all information sync file
        public static readonly string ShoppingListFileName = "_shop.pbf"; // shopping list file

        public static bool isLoggedIn { get; set; }
        public static bool hasSyncedItems { get; set; }

        public static List<Item> MasterItemList { get; set; }

        // List view items
        public static ObservableCollection<Item> ItemList { get; set; }
        public static ObservableCollection<Order> OrderList { get; set; }
        public static ObservableCollection<Item> GlobalItemList { get; set; }
        public static ObservableCollection<Order> GlobalOrderList { get; set; }
        public static ObservableCollection<string> LocationList { get; set; }
        public static ObservableCollection<string> PaymentList { get; set; }
        public static ObservableCollection<string> CategoryList { get; set; }
        public static ObservableCollection<ShopItem> ShoppingList { get; set; }

        // Exclusively for application to handle sync process
        public static List<string> SyncList { get; set; }

        // Parameters for checking ifEditing
        public static string CurrentOrderId { get; set; }
        public static int itemEditingIndex { get; set; }

        public static Color TextColor = Color.White;
        public static Color SyncColor = Color.Gray;

        public static string CurrentLocation = "";
        public static string CurrentPayment = "";

        public static string CurrentCategory = "";
        public static string CurrentItemName = "";
        public static int CurrentItemQuantity = 0;
        public static string CurrentItemPrice = "";

        private static Dictionary<string, string> _settingsDictionary;

        static Global()
        {
            isLoggedIn = false;
            hasSyncedItems = false;
            _settingsDictionary = new Dictionary<string, string>();
            PaymentList = new ObservableCollection<string>();
            ResetParameters();
        }

        /// <summary>
        /// Reset parameters
        /// </summary>
        public static void ResetParameters()
        {
            ItemList = new ObservableCollection<Item>();
            OrderList = new ObservableCollection<Order>();
            GlobalItemList = new ObservableCollection<Item>();
            GlobalOrderList = new ObservableCollection<Order>();
            LocationList = new ObservableCollection<string>();
            PaymentList = new ObservableCollection<string>();
            CategoryList = new ObservableCollection<string>();
            ShoppingList = new ObservableCollection<ShopItem>();
            SyncList = new List<string>();
            MasterItemList = new List<Item>();
            PaymentList.Add("Cash");
            PaymentList.Add("Other"); 

            ResetVolatileParameters();
        }

        /// <summary>
        /// One time use variables
        /// </summary>
        public static void ResetVolatileParameters()
        {
            //CurrentCategory = ""; set as persistent
            itemEditingIndex = -1;
            CurrentOrderId = "";
            CurrentLocation = "";
            CurrentPayment = "";
            CurrentItemName = "";
            CurrentItemQuantity = 0;    
            CurrentItemPrice = "";
        }

        //======================== DICTIONARY ACCESS ==========================

        /// <summary>
        /// Set dictionary value from settings dictionary
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        public static void SetSettingsValue(string settingName, string value)
        {
            try
            {
                if (_settingsDictionary.ContainsKey(settingName))
                {
                    _settingsDictionary[settingName] = value;
                }
                else
                {
                    _settingsDictionary.Add(settingName, value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Invalid dictionary set procedure found for " + settingName + ", " + value);
            }
        }

        /// <summary>
        /// Get dictionary value from settings dictionary
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static string GetSettingsValue(string settingName)
        {
            if (_settingsDictionary.ContainsKey(settingName))
                return _settingsDictionary[settingName];
            else
            {
                Debug.WriteLine("Error, key not found!");
                return "";
            }
        }
        
        //=========================== LOAD FILES ==============================

        /// <summary>
        /// Load all local information from configuration file
        /// </summary>
        public static string LoadLocalInformation()
        {
            ResetParameters();

            try
            {
                string[] lines = GetFileContent(ConfigFileName)
                    .Split(new [] {Environment.NewLine}, StringSplitOptions.None);

                List<string> tempLocation = new List<string>();
                List<string> tempPayment = new List<string>();
                List<string> tempCategories = new List<string>();

                foreach (string line in lines)
                {
                    #region Load Local Settings

                    if (line.Contains("[PE_SE]"))
                    {
                        SetSettingsValue("SYNC_EMAIL", Parse_Line_Information(line, "SYNC_EMAIL"));
                        SetSettingsValue("EMAIL_VALIDATED", Parse_Line_Information(line, "EMAIL_VALIDATED"));
                    }

                    #endregion

                    #region Load Locations

                    if (line.Contains("[LO_NA_]="))
                    {
                        tempLocation.Add(Parse_Line_Information(line, "LO_NA_"));
                    }

                    #endregion

                    #region Load Payments

                    if (line.Contains("[PA_NA_]="))
                    {
                        tempPayment.Add(Parse_Line_Information(line, "PA_NA_"));
                    }

                    #endregion

                    #region Load Categories

                    if (line.Contains("[CA_NA_]="))
                    {
                        tempCategories.Add(Parse_Line_Information(line, "CA_NA_"));
                    }

                    #endregion

                    #region Load Items

                    if (line.Contains("[LT_NA_]="))
                    {
                        MasterItemList.Add(new Item(
                            Parse_Line_Information(line, "LT_NA_"),
                            Convert.ToDouble(Parse_Line_Information(line, "LT_PR_")),
                            Convert.ToInt32(Parse_Line_Information(line, "LT_QU_")),
                            Parse_Line_Information(line, "LT_CA_"),
                            Parse_Line_Information(line, "LT_ID_")
                        ));
                    }

                    #endregion

                    #region Load Orders

                    if (line.Contains("[LR_LO_]="))
                    {
                        OrderList.Add(new Order(
                            Parse_Line_Information(line, "LR_LO_"),
                            //Convert.ToDouble(Parse_Line_Information(line, "PRETAX_PRICE")),
                            Parse_Line_Information(line, "LR_PA_"),
                            Convert.ToDateTime(Parse_Line_Information(line, "LR_DA_")),
                            Parse_Line_Information(line, "LR_SY_") == "1",
                            Parse_Line_Information(line, "LR_ID_")
                        ));
                    }

                    #endregion

                    #region Load ShopItem List

                    if (line.Contains("[SI_NA_]="))
                    {
                        ShoppingList.Add(new ShopItem(Parse_Line_Information(line, "SI_NA_"), Parse_Line_Information(line, "SI_CA_")
                        ));
                    }

                    #endregion

                    #region Load Global Items
                    if (line.Contains("||[IT_LO]="))
                    {
                        Item New_Item = new Item();
                        New_Item.Name = Parse_Line_Information(line, "IT_DE_");
                        New_Item.Status = Parse_Line_Information(line, "IT_ST_") == "" ? "0" : Parse_Line_Information(line, "IT_ST_");
                        New_Item.RefundAlert = Parse_Line_Information(line, "IT_RE_") == "1" ? true : false;
                        New_Item.consumedStatus = Convert.ToInt32(Parse_Line_Information(line, "IT_CO_", "||", "2"));
                        New_Item.Location = Parse_Line_Information(line, "IT_LO");
                        New_Item.Payment_Type = Parse_Line_Information(line, "IT_PA_");
                        New_Item.Category = Parse_Line_Information(line, "IT_CA_");
                        New_Item.Discount_Amt = Convert.ToDouble(Parse_Line_Information(line, "IT_DI_", "||", "0"));
                        New_Item.Price = Convert.ToDouble(Parse_Line_Information(line, "IT_PR_"));
                        New_Item.Quantity = Convert.ToInt32(Parse_Line_Information(line, "IT_QU_"));
                        New_Item.Date = Convert.ToDateTime(Parse_Line_Information(line, "IT_DA_"));
                        New_Item.Refund_Date = Parse_Line_Information(line, "IT_RD_").Length > 0 ? Convert.ToDateTime(Parse_Line_Information(line, "IT_RD_")) : DateTime.Now;
                        New_Item.Memo = Parse_Line_Information(line, "IT_ME_");
                        New_Item.OrderID = Parse_Line_Information(line, "IT_ID_");

                        GlobalItemList.Add(New_Item);
                    }
                    #endregion

                    #region Load Global Orders
                    // Load orders
                    else if (line.Contains("||[OR_QU_]="))
                    {
                        Order New_Order = new Order();
                        New_Order.Location = Parse_Line_Information(line, "OR_LO_");
                        New_Order.OrderMemo = Parse_Line_Information(line, "OR_ME_");
                        New_Order.Payment = Parse_Line_Information(line, "OR_PA_");
                        New_Order.Tax_Overridden = (Parse_Line_Information(line, "OR_TO_") == "1");
                        New_Order.Order_Total_Pre_Tax = Convert.ToDouble(Parse_Line_Information(line, "OR_PP_"));
                        New_Order.GC_Amount = Convert.ToDouble(Parse_Line_Information(line, "OR_GC_", "||", "0"));
                        New_Order.Order_Taxes = Convert.ToDouble(Parse_Line_Information(line, "OR_TA_"));
                        New_Order.Order_Discount_Amt = Convert.ToDouble(Parse_Line_Information(line, "OR_DI_", "||", "0"));
                        New_Order.Order_Quantity = Convert.ToInt32(Parse_Line_Information(line, "OR_QU_"));
                        New_Order.Date = Convert.ToDateTime(Parse_Line_Information(line, "OR_DA_"));
                        New_Order.OrderID = Parse_Line_Information(line, "OR_ID_");

                        GlobalOrderList.Add(New_Order);
                    }
                    #endregion

                }

                // Load all synced orders
                LoadSyncFile();

                // Sort locations
                LocationList = new ObservableCollection<string>(tempLocation.OrderBy(x => x));
                PaymentList = new ObservableCollection<string>(tempPayment.OrderBy(x => x));
                CategoryList = new ObservableCollection<string>(tempCategories.OrderBy(x => x));

                // Remove synced orders from current orders
                return RemoveSyncedOrders();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error with loading file: " + ex);
            }
            return "";
        }

        //=========================== SAVE FILES ==============================

        /// <summary>
        /// Save all local information into configuration file
        /// </summary>
        public static void SaveLocalInformation()
        {

            // Clean up!
            CleanUpUnmappedItems();

            string saveText = "[PE_SE]";

            #region Save Local Settings

            foreach (KeyValuePair<string, string> Key in _settingsDictionary)
            {
                saveText += "||[" + Key.Key + "]=" + Key.Value;
            }
            saveText += Environment.NewLine;

            #endregion

            #region Save Locations

            foreach (string location in LocationList)
            {
                saveText += "[LO_NA_]=" + location + Environment.NewLine;
            }

            #endregion

            #region Save Payments

            foreach (string payment in PaymentList)
            {
                saveText += "[PA_NA_]=" + payment + Environment.NewLine;
            }

            #endregion

            #region Save Category

            foreach (string category in CategoryList)
            {
                saveText += "[CA_NA_]=" + category + Environment.NewLine;
            }

            #endregion

            #region Save Items

            foreach (Item item in MasterItemList)
            {
                saveText += "[LT_NA_]=" + item.Name +
                            "||[LT_CA_]=" + item.Category +
                            "||[LT_QU_]=" + item.Quantity +
                            "||[LT_PR_]=" + item.Price +
                            "||[LT_ID_]=" + item.OrderID + Environment.NewLine;
            }

            #endregion

            #region Save Orders

            foreach (Order order in OrderList)
            {
                saveText += "[LR_LO_]=" + order.Location +
                            //"||[PRETAX_PRICE]=" + order.OrderTotalPreTax +
                            "||[LR_DA_]=" + order.Date.ToString() +
                            "||[LR_SY_]=" + (order.IsSyncing ? "1" : "0") +
                            "||[LR_PA_]=" + order.Payment +
                            "||[LR_ID_]=" + order.OrderID + Environment.NewLine;
            }

            #endregion

            #region Save ShopItemList
            string temp = ""; // using temp reduces complexity
            foreach (ShopItem SI in ShoppingList)
            {
                temp += "[SI_NA_]=" + SI.Name +
                        "||[SI_CA_]=" + SI.Category + Environment.NewLine;
            }
            saveText += temp;
            #endregion

            #region Save Global Items
            foreach (Item item in GlobalItemList)
            {
                saveText += "[IT_DE_]=" + item.Name +
                            "||[IT_LO]=" + item.Location +
                            "||[IT_ST_]=" + item.Status +
                            "||[IT_CA_]=" + item.Category +
                            "||[IT_QU_]=" + item.Quantity +
                            "||[IT_PR_]=" + item.Price +
                            "||[IT_DI_]=" + item.Discount_Amt +
                            "||[IT_DA_]=" + item.Date.ToString() +
                            "||[IT_RD_]=" + item.Refund_Date.ToString() +
                            "||[IT_PA_]=" + item.Payment_Type +
                            "||[IT_ID_]=" + item.OrderID +
                            "||[IT_RE_]=" + (item.RefundAlert ? "1" : "0") +
                            "||[IT_CO_]=" + item.consumedStatus +
                            "||[IT_ME_]=" + item.Memo + Environment.NewLine;
            }

            #endregion

            #region Save Global Orders
            foreach (Order order in GlobalOrderList)
            {
                saveText += "[OR_LO_]=" + order.Location +
                            "||[OR_QU_]=" + order.Order_Quantity +
                            "||[OR_PP_]=" + order.Order_Total_Pre_Tax +
                            "||[OR_DI_]=" + order.Order_Discount_Amt +
                            "||[OR_GC_]=" + order.GC_Amount +
                            "||[OR_TA_]=" + order.Order_Taxes +
                            "||[OR_ME_]=" + order.OrderMemo +
                            "||[OR_TO_]=" + (order.Tax_Overridden ? "1" : "0") +
                            "||[OR_DA_]=" + order.Date.ToString() +
                            "||[OR_PA_]=" + order.Payment +
                            "||[OR_ID_]=" + order.OrderID + Environment.NewLine;
            }
            #endregion

            WriteFileContent(ConfigFileName, saveText);

            if (hasSyncedItems)
            {
                SaveSyncFile();
                hasSyncedItems = false;
            }

        }

        //=========================== SYNC SYSTEM =============================

        /// <summary>
        /// Upload entire information item/order list from FTP/Cloud_Sync/FullSync directory
        /// </summary>
        public static void DownloadAllInformation()
        {
            string syncText = "";
            string emailAddress = GetSettingsValue("SYNC_EMAIL");


            try
            {
                if (Global.isOnWifi()) // only try on WiFi (most data will block this)
                {
                    List<string> lines = new List<string>();
                    // Copy local repository to FTP Server
                    if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
                    {
                        // Read from FTP File
                        //string[] lines = GetFileContent(emailAddress + SyncFileName)
                        lines = AESGCM
                            .SimpleDecryptWithPassword(
                                DependencyService.Get<IFtpWebRequest>().FTPRead(emailAddress + FullSyncFileName), AESGCMKey)
                            .Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

                    }

                    // Load items and orders from downloaded file
                    foreach (string line in lines)
                    {
                        // Load items
                        if (line.Contains("||[IT_LO]="))
                        {
                            Item New_Item = new Item();
                            New_Item.Name = Parse_Line_Information(line, "IT_DE_");
                            New_Item.Status = Parse_Line_Information(line, "IT_ST_") == "" ? "0" : Parse_Line_Information(line, "IT_ST_");
                            New_Item.RefundAlert = Parse_Line_Information(line, "IT_RE_") == "1" ? true : false;
                            New_Item.consumedStatus = Convert.ToInt32(Parse_Line_Information(line, "IT_CO_", "||", "2"));
                            New_Item.Location = Parse_Line_Information(line, "IT_LO");
                            New_Item.Payment_Type = Parse_Line_Information(line, "IT_PA_");
                            New_Item.Category = Parse_Line_Information(line, "IT_CA_");
                            New_Item.Discount_Amt = Convert.ToDouble(Parse_Line_Information(line, "IT_DI_", "||", "0"));
                            New_Item.Price = Convert.ToDouble(Parse_Line_Information(line, "IT_PR_"));
                            New_Item.Quantity = Convert.ToInt32(Parse_Line_Information(line, "IT_QU_"));
                            New_Item.Date = Convert.ToDateTime(Parse_Line_Information(line, "IT_DA_"));
                            New_Item.Refund_Date = Parse_Line_Information(line, "IT_RD_").Length > 0 ? Convert.ToDateTime(Parse_Line_Information(line, "IT_RD_")) : DateTime.Now;
                            New_Item.Memo = Parse_Line_Information(line, "IT_ME_");
                            New_Item.OrderID = Parse_Line_Information(line, "IT_ID_");

                            GlobalItemList.Add(New_Item);
                        }

                        // Load orders
                        else if (line.Contains("||[OR_QU_]="))
                        {
                            Order New_Order = new Order();
                            New_Order.Location = Parse_Line_Information(line, "OR_LO_");
                            New_Order.OrderMemo = Parse_Line_Information(line, "OR_ME_");
                            New_Order.Payment = Parse_Line_Information(line, "OR_PA_");
                            New_Order.Tax_Overridden = (Parse_Line_Information(line, "OR_TO_") == "1");
                            New_Order.Order_Total_Pre_Tax = Convert.ToDouble(Parse_Line_Information(line, "OR_PP_"));
                            New_Order.GC_Amount = Convert.ToDouble(Parse_Line_Information(line, "OR_GC_", "||", "0"));
                            New_Order.Order_Taxes = Convert.ToDouble(Parse_Line_Information(line, "OR_TA_"));
                            New_Order.Order_Discount_Amt = Convert.ToDouble(Parse_Line_Information(line, "OR_DI_", "||", "0"));
                            New_Order.Order_Quantity = Convert.ToInt32(Parse_Line_Information(line, "OR_QU_"));
                            New_Order.Date = Convert.ToDateTime(Parse_Line_Information(line, "OR_DA_"));
                            New_Order.OrderID = Parse_Line_Information(line, "OR_ID_");

                            GlobalOrderList.Add(New_Order);

                        }
                    }

                    foreach (Item item in GlobalItemList)
                    {
                        Debug.WriteLine(item.ToString());
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("No file found: " + Ex);
            }
        }

        /// <summary>
        /// Save all local information into configuration file
        /// </summary>
        public static async void SaveSyncFile()
        {
            SyncList = SyncList.Distinct().ToList();


            string emailAddress = GetSettingsValue("SYNC_EMAIL");
            string saveText = "";

            #region Save Synced Files
            
            //foreach (var orderID in SyncList)
            foreach (Order order in OrderList.Where(x => x.IsSyncing))
            {
                //saveText += orderID + Environment.NewLine;

                #region Save corresponding items with orderID

               // Order order = OrderList.First(x => x.OrderID == orderID);
                saveText += "[OR_LO_]=" + order.Location +
                            //"||[PRETAX_PRICE]=" + order.OrderTotalPreTax +
                            "||[OR_DA_]=" + order.Date.ToString() +
                            "||[OR_PA_]=" + order.Payment +
                            "||[OR_ID_]=" + order.OrderID + Environment.NewLine;

                #endregion

                #region Save order with corresponding orderID

                foreach (Item item in MasterItemList.Where(x => x.OrderID == order.OrderID).ToList())
                {
                    saveText += "[IT_NA_]=" + item.Name +
                                "||[IT_CA_]=" + item.Category +
                                "||[IT_QU_]=" + item.Quantity +
                                "||[IT_PR_]=" + item.Price +
                                "||[IT_ID_]=" + item.OrderID + Environment.NewLine;
                }

                #endregion

            }

            #endregion

            if (saveText.Length < 4) saveText = " ";
                
            // Create local repository
            await WriteFileContent("localSyncFTP", saveText);
             
            // Copy local repository to FTP Server
            if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
            {
                DependencyService.Get<IFtpWebRequest>().FTPWrite(emailAddress + SyncFileName,
                    Path.Combine(_fileSystem.LocalStorage.Path, "localSyncFTP"));
            }

            // Delete local copy
            //DeleteFileContent("localSyncFTP");
        }

        /// <summary>
        /// Load the current sync file if available
        /// </summary>
        public static async Task<bool> LoadSyncFile()
        {
            string syncText = "";
            string emailAddress = GetSettingsValue("SYNC_EMAIL");

            try
            {
                if (Global.isOnWifi()) // only try on WiFi (most data will block this)
                {
                    if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
                    {
                        // Copy local repository to FTP Server
                            // Read from FTP File
                            //string[] lines = GetFileContent(emailAddress + SyncFileName)
                        Task<string> ftpLine = Task.Run(() => DependencyService.Get<IFtpWebRequest>()
                                .FTPRead(emailAddress + SyncFileName));

                    
                        List<string> lines = AESGCM.SimpleDecryptWithPassword(await ftpLine, AESGCMKey).Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

                        // Only load orders
                        foreach (string line in lines)
                        {
                            // if is already synced, append "[ISSYNCED]=1" if contains this in sync file (the desktop application will append this AFTER it syncs)
                            if (line.Length > 5 && line.Contains("[OR_LO_]="))
                            {
                                SyncList.Add(Parse_Line_Information(line, "OR_ID_") +
                                             (line.Contains("[OR_SY_]=1")
                                                 ? "[OR_SY_]=1"
                                                 : "")); // append only order id to list
                            }
                        }

                        // Remove duplication
                        SyncList.Distinct();

                    }
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine("No file found: " + Ex);
            }


            return true; //complete
        }

        /// <summary>
        /// Remove synced orders from current OrderList and SyncList simultaneously
        /// </summary>
        public static string RemoveSyncedOrders()
        {
            string removedItems = "";
            // Remove synced items from orders
            for (int i = OrderList.Count - 1; i >= 0; i--)
            {
                if (SyncList.Count(x => x.Contains(OrderList[i].OrderID) && x.Contains("[OR_SY_]=1")) > 0)
                {
                    removedItems += OrderList[i].OrderID + ", ";
                    OrderList.RemoveAt(i);
                }
            }

            // Remove synced items from SyncList
            SyncList = SyncList.Where(x => !x.Contains("[OR_SY_]=1")).ToList();

            return removedItems;
        }

        /// <summary>
        /// Load the current sync file if available
        /// </summary>
        public static void ReadShoppingSyncFile()
        {
            string syncText = "";
            string emailAddress = GetSettingsValue("SYNC_EMAIL");

            Debug.WriteLine("Reading Cloud Shopping file....");

            try
            {
                if (Debugger.IsAttached || Global.isOnWifi()) // only try on WiFi (most data will block this)
                {
                    Debug.WriteLine("WIFI DETECTED!");
                    List<string> lines = new List<string>();
                    // Copy local repository to FTP Server
                    if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
                    {
                        // Read from FTP File
                        //string[] lines = GetFileContent(emailAddress + SyncFileName)
                        lines = AESGCM
                            .SimpleDecryptWithPassword(
                                DependencyService.Get<IFtpWebRequest>().FTPRead(emailAddress + ShoppingListFileName), AESGCMKey)
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                    }

                    // Only load orders
                    foreach (string line in lines)
                    {
                        Debug.WriteLine("line: " + line);
                        // if is already synced, append "[ISSYNCED]=1" if contains this in sync file (the desktop application will append this AFTER it syncs)
                        if (line.Length > 5 && line.Contains("[SI_NA_]="))
                        {
                            string name = Parse_Line_Information(line, "SI_NA_");
                            string category = Parse_Line_Information(line, "SI_CA_");

                            if (!ShoppingList.Any(x => x.Name == name && x.Category == category)) // only add items that are not already there
                                ShoppingList.Add(new ShopItem(name, category));
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("No file found: " + Ex);
            }
            Debug.WriteLine("Done reading....");
        }

        //=========================== FILE SYSTEM =============================

        public static readonly IFileSystem _fileSystem = FileSystem.Current; // Master file system

        // Get the root directory of the file system for our application.
        private static IFolder rootFolder = _fileSystem.LocalStorage;

        public static string fileText = "";

        /// <summary>
        /// Create root folder (will check if it already exists)
        /// </summary>
        public static async void CreateRootFolder()
        {
            // Create PersonalBanker root folder, if one doesn't already exist.
            IFolder photosFolder =
                await rootFolder.CreateFolderAsync("PersonalBankerRoot", CreationCollisionOption.OpenIfExists);

        }

        /// <summary>
        /// Create file. Do nothing if it exists already
        /// </summary>
        /// <param name="fileName"></param>
        public static async void CreateFileAtRoot(string fileName)
        {
            string filePath = Path.Combine(_fileSystem.LocalStorage.Path, fileName);
            Debug.WriteLine(filePath);
            try
            {
                await _fileSystem.LocalStorage.CreateFileAsync(filePath, CreationCollisionOption.FailIfExists);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Get the file content in a parseable string
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>filetext</returns>
        public static string GetFileContent(string fileName)
        {
            ReadFileContent(fileName);
            return fileText;
        }

        /// <summary>
        ///  read files and sets fileText to file content
        /// </summary>
        /// <param name="fileName"></param>
        public static async void ReadFileContent(string fileName)
        {
            string filePath = Path.Combine(_fileSystem.LocalStorage.Path, fileName);
            //IFile file = await rootFolder.GetFileAsync(filePath).ConfigureAwait(false);
           // fileText = await file.ReadAllTextAsync();

            try
            {
                IFile file = rootFolder.GetFileAsync(filePath).Result;
                fileText = AESGCM.SimpleDecryptWithPassword(file.ReadAllTextAsync().Result, AESGCMKey); //decrypt
            }
            catch (Exception e)
            {
                Debug.WriteLine( e.ToString()); 
            }
        }

        /// <summary>
        /// write text into file named fileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        public static async Task<bool> WriteFileContent(string fileName, string text)
        {

            // Create new file; do nothing if exists
            CreateFileAtRoot(fileName);

            string filePath = Path.Combine(_fileSystem.LocalStorage.Path, fileName);
            IFile file = await rootFolder.GetFileAsync(filePath);
            await file.WriteAllTextAsync(AESGCM.SimpleEncryptWithPassword(text, AESGCMKey)); //encrypt

            return true;
        }

        /// <summary>
        /// Delte existing file. Assumes exists; else throws
        /// </summary>
        /// <param name="fileName"></param>
        public static async void DeleteFileContent(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_fileSystem.LocalStorage.Path, fileName);
                IFile file = await rootFolder.GetFileAsync(filePath);
                await file.DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot delete file; file does not exist or another error: " + ex);
            }
        }

        /// <summary>
        /// Return the output line after [output].
        /// 
        /// For example, in line = [INFO_TYPE]=ITEM||[ITEM_NAME]=CLOTHING||[ITEM_PRICE]=49.22||....
        ///     Calling this program:
        /// 
        ///     
        ///     Parse_Line_Information(line, "ITEM_PRICE", parse_token = "||") returns "49.22"
        ///     
        /// </summary>
        private static string Parse_Line_Information(string input, string output, string parse_token = "||", string default_string = "")
        {
            string[] Split_Layer_1 = input.Split(new string[] { parse_token }, StringSplitOptions.None);

            foreach (string Info_Pair in Split_Layer_1)
            {
                if (Info_Pair.Contains("[" + output + "]"))
                {
                    return Info_Pair.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                }
            }

            return default_string;
        }


        //============================== UNSYNCED ORDERS ===============================

        /// <summary>
        /// Return an order given orderID
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static Order GetOrderById(string orderID)
        {
            return OrderList.FirstOrDefault(x => x.OrderID == orderID);
        }

        /// <summary>
        /// Add Order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="index">Optional, insert index</param>
        public static void AddOrder(Order order, int index=-1)
        {
            if (index < 0)
            {
                if (OrderList.Count == 0)
                {
                    OrderList.Add(order);
                    return;
                }
                // Add in appropriate date slot
                for (int i = 0; i < OrderList.Count; i++)
                {
                    OrderList.Insert(i, order);
                    return;
                }
            }
            else
            {
                OrderList.Insert(index, order);
            }
            // Sort by date
        }

        /// <summary>
        /// Delete Order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="index">Optional, insert index</param>
        public static void DeleteOrder(Order order, int index = -1)
        {
            if (index < 0)
            {
                OrderList.Remove(order);
            }
            else
            {
                OrderList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Delete specific order by orderID
        /// </summary>
        /// <param name="orderID"></param>
        public static void DeleteOrderById(string orderID)
        {
            foreach (Order order in OrderList)
            {
                if (order.OrderID == orderID)
                {
                    OrderList.Remove(order);
                    break; //terminate for collection
                }
            }
        }

        /// <summary>
        /// Generate an unused orderID
        /// </summary>
        /// <returns></returns>
        public static string GenerateNewOrderId()
        {
            Random ranGen = new Random();
            string randID = ranGen.Next(100000000, 999999998).ToString();
            while (OrderList.Count(x => x.OrderID == randID) > 0) //while clashing with existing ID hash
            {
                randID = ranGen.Next(100000000, 999999998).ToString();
            }
            return randID;
        }

        //============================== UNSYNCED ITEMS ===============================

        /// <summary>
        /// Return an item given orderID
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static Item GetItemById(string orderID)
        {
            return ItemList.FirstOrDefault(x => x.OrderID == orderID);
        }

        /// <summary>
        /// Add Order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="index">Optional, insert index</param>
        public static void AddItem(Item item, int index = -1)
        {
            if (index < 0)
            {
                MasterItemList.Add(item);
                ItemList.Add(item);
            }
            else
            {
                MasterItemList.Add(item); //doesnt care about the index
                ItemList.Insert(index, item);
            }
        }

        /// <summary>
        /// Delete item (index is where index in ITEMLIST and NOT MASTER)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="index">Optional, insert index</param>
        public static void DeleteItem(Item item, int index = -1)
        {
            if (index < 0)
            {
                MasterItemList.Remove(item);
                ItemList.Remove(item);
            }
            else
            {
                MasterItemList.Remove(ItemList[index]); 
                ItemList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Delete specific order by orderID
        /// </summary>
        /// <param name="orderID"></param>
        public static void DeleteItemsById(string orderID, string itemName = "")
        {
            for (int i = MasterItemList.Count - 1; i >= 0; i--)
            {
                // If item matches order ID and if (optional) itemName == Item.Name
                if (MasterItemList[i].OrderID == orderID && (itemName == "" || itemName == MasterItemList[i].Name))
                {
                    MasterItemList.RemoveAt(i);
                    break; //terminate for collection
                }
            }
            Debug.WriteLine("Items left ============");
            foreach (Item item in MasterItemList)
            {
                Debug.WriteLine(item.ToString());
            }

        }

        /// <summary>
        /// Filter the ObservableCollection down to an item list only containing orderID
        /// </summary>
        public static void FilterItemListById(string orderID)
        {
            ItemList.Clear();

            foreach (Item item in MasterItemList)
            {
                if (item.OrderID == orderID)
                    ItemList.Add(item);
            }
        }

        /// <summary>
        /// Remove items that are not mapped a.k.a items generated for orders that are not saved
        /// </summary>
        private static void CleanUpUnmappedItems()
        {
            List<string> OrderIdList = OrderList.Select(x => x.OrderID).ToList();
            MasterItemList = MasterItemList.Where(x => OrderIdList.Contains(x.OrderID)).ToList(); // remove unmapped items
        }

        /// <summary>
        /// Set all temporary jobs into current ones
        /// </summary>
        public static void SaveOrderItems()
        {
            foreach (Item item in MasterItemList.Where(x => x.OrderID == "999999999").ToList())
            {
                item.OrderID = CurrentOrderId;
            }
        }

        /// <summary>
        /// Removes all temporary items from the item list
        /// </summary>
        public static void ClearTempItems()
        {
            MasterItemList = MasterItemList.Where(x => x.OrderID != "999999999").ToList();
        }
    }

    public class YesNoSyncColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "true":
                    return Global.SyncColor;
                case "false":
                    return Global.TextColor;
            }

            return Color.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // You probably don't need this, this is used to convert the other way around
            // so from color to yes no or maybe
            throw new NotImplementedException();
        }
    }
}
