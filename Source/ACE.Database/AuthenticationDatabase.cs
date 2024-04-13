using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;

using log4net;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using ACE.Database.Models.Auth;
using ACE.Entity.Enum;
using System.Collections.Generic;
using System;
using System.Net;

namespace ACE.Database
{
    public class AuthenticationDatabase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(bool retryUntilFound)
        {
            var config = Common.ConfigManager.Config.MySql.Authentication;

            for (; ; )
            {
                using (var context = new AuthDbContext())
                {
                    if (((RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>()).Exists())
                    {
                        log.Debug($"Successfully connected to {config.Database} database on {config.Host}:{config.Port}.");
                        return true;
                    }
                }

                log.Error($"Attempting to reconnect to {config.Database} database on {config.Host}:{config.Port} in 5 seconds...");

                if (retryUntilFound)
                    Thread.Sleep(5000);
                else
                    return false;
            }
        }


        public int GetAccountCount()
        {
            using (var context = new AuthDbContext())
                return context.Account.Count();
        }

        /// <exception cref="MySqlException">Account with name already exists.</exception>
        public Account CreateAccount(string name, string password, AccessLevel accessLevel, IPAddress address)
        {
            var account = new Account();

            account.AccountName = name;
            account.SetPassword(password);
            account.SetSaltForBCrypt();
            account.AccessLevel = (uint)accessLevel;

            account.CreateTime = DateTime.UtcNow;
            account.CreateIP = address.GetAddressBytes();

            using (var context = new AuthDbContext())
            {
                context.Account.Add(account);

                context.SaveChanges();
            }

            return account;
        }

        /// <summary>
        /// Will return null if the accountId was not found.
        /// </summary>
        public Account GetAccountById(uint accountId)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountId == accountId);
            }
        }

        /// <summary>
        /// Will return null if the accountName was not found.
        /// </summary>
        public Account GetAccountByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);
            }
        }

        /// <summary>
        /// id will be 0 if the accountName was not found.
        /// </summary>
        public uint GetAccountIdByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                var result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);

                return (result != null) ? result.AccountId : 0;
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var context = new AuthDbContext())
            {
                context.Entry(account).State = EntityState.Modified;

                context.SaveChanges();
            }
        }

        public bool UpdateAccountAccessLevel(uint accountId, AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var account = context.Account
                    .First(r => r.AccountId == accountId);

                if (account == null)
                    return false;

                account.AccessLevel = (uint)accessLevel;

                context.SaveChanges();
            }

            return true;
        }

        public List<string> GetListofAccountsByAccessLevel(AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.AccessLevel == Convert.ToUInt32(accessLevel)).ToList();

                var result = new List<string>();
                foreach (var account in results)
                    result.Add(account.AccountName);

                return result;
            }
        }

        public List<string> GetListofBannedAccounts()
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.BanExpireTime > DateTime.UtcNow).ToList();

                var result = new List<string>();
                foreach (var account in results)
                {
                    var bannedbyAccount = account.BannedByAccountId.Value > 0 ? $"account {GetAccountById(account.BannedByAccountId.Value).AccountName}" : "CONSOLE";
                    result.Add($"{account.AccountName} -- banned by {bannedbyAccount} until server time {account.BanExpireTime.Value.ToLocalTime():MMM dd yyyy  h:mmtt}{(!string.IsNullOrWhiteSpace(account.BanReason) ? $" -- Reason: {account.BanReason}" : "")}");
                }

                return result;
            }
        }

        public (DateTime Daily, DateTime Weekly, uint Week) GetXpCapTimestamps()
        {
            using (var context = new AuthDbContext())
            {
                var timestamps = context.XpCap.FirstOrDefault();

                if (timestamps == null)
                {
                    timestamps = new XpCap
                    {
                        DailyTimestamp = DateTime.UtcNow.AddDays(1),
                        WeeklyTimestamp = DateTime.UtcNow.AddDays(7),
                        Week = 1
                    };

                    context.XpCap.Add(timestamps);
                }
                else
                {
                    DateTime currentDateTime = DateTime.UtcNow;


                    if (currentDateTime > timestamps.DailyTimestamp)
                    {
                        timestamps.DailyTimestamp = currentDateTime.AddDays(1);
                    }

                    if (currentDateTime > timestamps.WeeklyTimestamp)
                    {
                        timestamps.WeeklyTimestamp = currentDateTime.AddDays(7);

                        timestamps.Week++;

                    }
                }

                context.SaveChanges();

                return (timestamps.DailyTimestamp, timestamps.WeeklyTimestamp, timestamps.Week);
            }
        }

        public void LogCharacterLogin(uint accountId, string accountName, string sessionIP, uint characterId, string characterName)
        {
            var logEntry = new CharacterLogin();

            try
            {
                logEntry.AccountId = accountId;
                logEntry.AccountName = accountName;
                logEntry.SessionIP = sessionIP;
                logEntry.CharacterId = characterId;
                logEntry.CharacterName = characterName;
                logEntry.LoginDateTime = DateTime.Now;

                using (var context = new AuthDbContext())
                {
                    context.CharacterLogin.Add(logEntry);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception in LogCharacterLogin saving character login info to DB. Ex: {ex}");
            }
        }
        public List<string> GetCharactersAssociatedWithIp(string sessionIp)
        {
            using (var context = new AuthDbContext())
            {
                var logins = context.CharacterLogin.Where(login => login.SessionIP == sessionIp);
                return logins.Select(login => login.CharacterName).ToList();
            }
        }
    }
}
