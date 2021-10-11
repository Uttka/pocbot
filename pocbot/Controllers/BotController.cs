using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.Data.SqlClient;

namespace pocbot.Controllers
{


    [ApiController]
    [Route("api/bot")]
    public class BotController : ControllerBase
    {
        string pp;
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update) 
        {
            string connStr = "Data Source=DEL_E7440\\MSSQLSERVER01;Initial Catalog=TestDB;Integrated Security=True";
            TelegramBotClient client = new TelegramBotClient("2072510364:AAGqJsvmQDpPWL4sQoUlrC0_2dmiucFugXc");
           
            if (update.Type==Telegram.Bot.Types.Enums.UpdateType.Message)
            {

                pp =Convert.ToString(update.Message.Text);
                double bonusov = 0,
                  tovar = 0,
                  chekov = 0,
                  time = 0;
                string timeword;

                if (pp.EndsWith("M"))
                {
                    time = -30;
                    pp = pp.Remove(pp.Length - 1);
                    timeword = "месяц";

                }
                else if (pp.EndsWith("W"))
                {
                    time = -7;
                    pp = pp.Remove(pp.Length - 1);
                    timeword = "неделю";
                }
                else
                {

                    pp = "0";
                    timeword = "?";
                }
                string sqlexpression = sqlcd(time, "AddBonusesDate", "UserBonuses", "tblCheckBonusesUser", pp);
                string sqlexpression2 = sqlcd(time, "check_date", "check_count_sku", "tblTransactions", pp);

                SqlConnection connection = null;
                // подключение к бд
                connection = new SqlConnection(connStr);
                SqlCommand command = new SqlCommand(sqlexpression, connection);
                try
                {

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        bonusov += Convert.ToDouble(reader["UserBonuses"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw;

                }
                finally
                {
                    connection.Close();
                }
                //перебор таблицы транзакций 
                SqlCommand command2 = new SqlCommand(sqlexpression2, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader2 = command2.ExecuteReader();

                    while (reader2.Read())
                    {
                        tovar += Convert.ToDouble(reader2["check_count_sku"].ToString());
                        chekov += 1;
                    }


                }
                catch (Exception ex)
                {
                    throw;

                }
                finally
                {

                    connection.Close();
                }
                //вывод сообщения пользователю
                if (pp != "0")
                {
                    var itog =
                         $"Клиент: " + pp + "\n" +
                         $"Отправлено чеков за {timeword}: " + Convert.ToString(chekov) + "\n" +
                         $"Куплено товаров за {timeword}: " + Convert.ToString(tovar) + "\n" +
                         $"Заработано за {timeword} БОНУСОВ: " + Convert.ToString(bonusov);

                    await client.SendTextMessageAsync(update.Message.From.Id, itog);
                }
                else
                {
                    await client.SendTextMessageAsync(update.Message.From.Id, "вы забыли символ, или ввели некорректный номер");
                 
                }


                
            }
            return Ok();
        
        }
        //функция вывода sql 
        string sqlcd(double wt, string ft, string st, string tabl, string phn)
        {

            string sqlp = $"declare @Lastday as datetime set @Lastday = (select max({ft}) " +
                          $"from {tabl} where UserPhoneNumber = {Convert.ToDouble(phn)}); " +
                          $"select {ft}, {st} from {tabl} " +
                          $"where UserPhoneNumber = {Convert.ToDouble(phn)} and {ft}> " +
                          $"DATEADD(day,{wt}, @Lastday) and {st}!=0";
            return sqlp;

        }
    } 

}