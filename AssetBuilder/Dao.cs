using System.Data.SQLite;

namespace AssetBuilder
{
    public class Dao
    {
        public SQLiteConnection Connection { get; }

        protected Dao(SQLiteConnection Connection)
        {
            this.Connection = Connection;
        }

        public void Query(string QueryString)
        {
            var command = Connection.CreateCommand();
            command.CommandText = QueryString;
            command.ExecuteNonQuery();
        }

        public static Dao From(string DatabasePath)
        {
            var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            connection.Open();
            return new Dao(connection);
        }

        public static void BuildDatabase(string DatabasePath)
        {
            SQLiteConnection.CreateFile(DatabasePath);
        }
    }
}
