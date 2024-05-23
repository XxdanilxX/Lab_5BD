using MySql.Data.MySqlClient;

namespace Lb_5BD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Додаємо цю лінію для підтримки українського тексту
            Console.WriteLine("Getting Connection ...");
            MySqlConnection conn = DBUtils.GetDBConnection();

            try
            {
                Console.WriteLine("Opening Connection ...");
                conn.Open();
                Console.WriteLine("Connection successful!");

                // Виклик збережених процедур
                CallStoredProcedure(conn, "GetAllOrders");
                CallStoredProcedure(conn, "GetAllFertilizers");
                CallStoredProcedure(conn, "GetAllClients");

                // Перевірка та створення тригерів
                CreateTriggerIfNotExists(conn, "log_order_insert", LogOrderInsertTrigger);
                CreateTriggerIfNotExists(conn, "update_fertilizer_stock", UpdateFertilizerStockTrigger);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            Console.Read();
        }


        private static void CreateTriggerIfNotExists(MySqlConnection conn, string triggerName, string triggerDefinition)
        {
            string checkTrigger = $"SELECT COUNT(*) FROM information_schema.triggers WHERE trigger_schema = '{conn.Database}' AND trigger_name = '{triggerName}'";
            MySqlCommand cmdCheck = new MySqlCommand(checkTrigger, conn);
            int triggerCount = Convert.ToInt32(cmdCheck.ExecuteScalar());

            if (triggerCount == 0)
            {
                MySqlCommand cmdCreate = new MySqlCommand(triggerDefinition, conn);
                cmdCreate.ExecuteNonQuery();
                Console.WriteLine($"Trigger '{triggerName}' created.");
            }
            else
            {
                Console.WriteLine($"Trigger '{triggerName}' already exists.");
            }
        }

        private static void CallStoredProcedure(MySqlConnection conn, string procedureName)
        {
            MySqlCommand cmd = new MySqlCommand(procedureName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write(reader.GetName(i) + ": " + reader.GetValue(i) + "\t");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine(new string('-', 80));
            }
        }

        
        // Визначення тригерів
        private const string LogOrderInsertTrigger = @"
            CREATE TRIGGER `log_order_insert` AFTER INSERT ON `Замовлення`
            FOR EACH ROW
            BEGIN
                INSERT INTO `Замовлення_Лог` (`код_замовлення`, `дата_заповнення`, `код_замовника`, `код_добрива`)
                VALUES (NEW.`код_замовлення`, NEW.`дата_заповнення`, NEW.`код_замовника`, NEW.`код_добрива`);
            END";

        private const string UpdateFertilizerStockTrigger = @"
            CREATE TRIGGER `update_fertilizer_stock` AFTER INSERT ON `Замовлення`
            FOR EACH ROW
            BEGIN
                UPDATE `Добрива` 
                SET `залишок` = `залишок` - NEW.`площа_для_обробки` 
                WHERE `код_добрива` = NEW.`код_добрива`;
            END";
    }
}
