using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using System;
using Lab1.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Lab1.DB
{
    public class FirebaseDB : IDataBase
    {
        public FirebaseDB(IConfiguration configuration)
        {
            _configuration = configuration;
            _firebaseConfig = new FirebaseConfig()
            {
                AuthSecret = _configuration["FireBase:AuthSecret"],
                BasePath = _configuration["FireBase:BasePath"]
            };
            dbClient = new FireSharp.FirebaseClient(_firebaseConfig);
        }

        private static IConfiguration _configuration;
        private static IFirebaseConfig _firebaseConfig;

        private static IFirebaseClient dbClient;

        public Scedule GetSceduleData()
        {
            try
            {
                FirebaseResponse response = dbClient.Get("Scedule");
                Scedule data = response.ResultAs<Scedule>();
                for(int i = 0; i < data.rounds.Length; i++)
                {
                    data.rounds[i].comments.RemoveAll(c => c == null);
                }
                Console.WriteLine(response.StatusCode);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void UpdateMatchData(Match match, int roundId, int matchId)
        {
            try
            {
                FirebaseResponse response = dbClient.Update("Scedule/rounds/" + roundId + "/matches/" + matchId, match);
                Console.WriteLine(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateRoundCommentsData(List<Comment> comments, int roundId)
        {
            try
            {
                FirebaseResponse response = dbClient.Set("Scedule/rounds/" + roundId + "/comments/", comments);
                Console.WriteLine(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DeleteComment(int roundId, int commentId)
        {
            try
            {
                FirebaseResponse response = dbClient.Delete("Scedule/rounds/" + roundId + "/comments/"+commentId);
                Console.WriteLine(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendScedule(Scedule scedule)
        {
            try
            {
                SetResponse response = dbClient.Set("Scedule", scedule);
                Console.WriteLine(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}



//public static void SendCredentialsToDB(PlayerCredentials playerCredentials)
//{
//    try
//    {
//        SetResponse response = dbClient.Set("UserCredentials/" + playerCredentials.username, playerCredentials);
//        Console.WriteLine(response.StatusCode);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//    }
//}

//public static void SendDataToDB(PlayerData playerData)
//{
//    try
//    {
//        SetResponse response = dbClient.Set("UserData/" + playerData.username, playerData);
//        Console.WriteLine(response.StatusCode);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//    }
//}

//public static PlayerCredentials GetCredentialsFromDB(string username)
//{
//    try
//    {
//        FirebaseResponse response = dbClient.Get("UserCredentials/" + username);
//        PlayerCredentials data = response.ResultAs<PlayerCredentials>();
//        Console.WriteLine(response.StatusCode);
//        return data;
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//        return null;
//    }
//}

//public static PlayerData GetDataFromDB(string username)
//{
//    try
//    {
//        FirebaseResponse response = dbClient.Get("UserData/" + username);
//        PlayerData data = response.ResultAs<PlayerData>();
//        Console.WriteLine(response.StatusCode);
//        return data;
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//        return null;
//    }
//}

//public static void UpdateDataOnDb(PlayerCredentials playerData)
//{
//    try
//    {
//        FirebaseResponse response = dbClient.Update("UserData/" + playerData.username, playerData);
//        Console.WriteLine(response.StatusCode);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//    }
//}

//public static void DeleteData(PlayerCredentials playerData)
//{
//    try
//    {
//        FirebaseResponse response = dbClient.Delete("UserData/" + playerData.username);
//        Console.WriteLine(response.StatusCode);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//    }
//}
