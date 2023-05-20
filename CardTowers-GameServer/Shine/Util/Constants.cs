using System;
namespace CardTowers_GameServer.Shine.Util
{
    public class Constants
    {
        public static int MAX_PLAYERS_STANDARD_MULTIPLAYER = 2;
        public static int MAX_AMOUNT_BUILDINGS = 12;
        public static string COGNITO_POOL_ID = "us-east-1_yTMdNWLqc";
        public static string COGNITO_REGION = "us-east-1";
        public static string PGSQL_RDS_CONNECTION_STRING = "Host=gb-cardtowers-dev.cerkerng2ucs.us-east-1.rds.amazonaws.com;Username=gbadmin;Password=sv(75>Mon&4[2{JxAoeKn3y&5td%;Database=cardtowers-dev;Port=5432;IncludeErrorDetail=true";


        public static int INITIAL_ELIXIR = 0;
        public static int MAX_ELIXIR = 10;
        public static long ELIXIR_GENERATION_INTERVAL_MS = 2000;
        public static int ELIXIR_PER_INTERVAL = 1;
    }
}

