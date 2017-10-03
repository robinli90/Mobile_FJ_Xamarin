/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;
using System.IO;
using SQLite;

namespace Mobile_FJ
{
    public class Databases
    {
        // Access the file system for the current platform.
        public static readonly IFileSystem _fileSystem = FileSystem.Current;

        // Database name for orders
        private static string _OrderDBName = "Order.db";

        /// <summary>
        /// returns orderdatabasepath
        /// </summary>
        /// <returns></returns>
        private static string GetOrderDBPath()
        {
            return Path.Combine(_fileSystem.LocalStorage.Path, _OrderDBName);
        }

        /// <summary>
        /// Create order.db database
        /// </summary>
        /// <returns></returns>
        public static string CreateOrderDatabase()
        {
            CreateRootFolder();

            try
            {
                var connection = new SQLiteAsyncConnection(GetOrderDBPath());
                connection.CreateTableAsync<Order>();
                return "Database created";

            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }


        }

        /// <summary>
        /// Allow inserting and updating orders in order.db
        /// </summary>
        /// <param name="data"></param>
        public static async void insertUpdateData(Order data)
        {
            try
            {
                var db = new SQLiteAsyncConnection(GetOrderDBPath());
                if (await db.InsertAsync(data) != 0)
                    await db.UpdateAsync(data);
                Debug.WriteLine("SQL Updated/Inserted");
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Create root folder
        /// </summary>
        public static async void CreateRootFolder()
        {

            // Get the root directory of the file system for our application.
            IFolder rootFolder = _fileSystem.LocalStorage;

            // Create PersonalBanker root folder, if one doesn't already exist.
            IFolder photosFolder =
                await rootFolder.CreateFolderAsync("PersonalBankerRoot", CreationCollisionOption.OpenIfExists);



        }
    }
}*/
