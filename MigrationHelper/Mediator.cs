using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationHelper
{
    public static class Mediator
    {
        /// <summary>
        /// Contains the dictionary with the actions
        /// </summary>
        private static readonly Dictionary<string, List<Action>> ActionDictionary = new Dictionary<string, List<Action>>();

        /// <summary>
        /// Register a new token with its callback
        /// </summary>
        /// <param name="token">The token (case insensitive)</param>
        /// <param name="callback">The callback</param>
        public static void Register(string token, Action callback)
        {
            if (!IsTokenValid(ref token))
                return;

            // Check if the token is already existing
            if (ActionDictionary.ContainsKey(token))
            {
                if (ActionDictionary.TryGetValue(token, out var actionList))
                {
                    if (!actionList.Any(a => a.Method.ToString().Equals(callback.Method.ToString())))
                        actionList.Add(callback);
                }
            }
            else
            {
                ActionDictionary.Add(token, new List<Action> { callback });
            }
        }

        /// <summary>
        /// Removes a callback action 
        /// </summary>
        /// <param name="token">The name of the callback</param>
        public static void Remove(string token)
        {
            if (!IsTokenValid(ref token))
                return;

            if (ActionDictionary.ContainsKey(token))
                ActionDictionary.Remove(token);
        }

        /// <summary>
        /// Removes all entries from the mediator
        /// </summary>
        public static void Clear()
        {
            ActionDictionary.Clear();
        }

        /// <summary>
        /// Executes a token
        /// </summary>
        /// <param name="token">The token (case insensitive)</param>
        public static void Execute(string token)
        {
            if (!IsTokenValid(ref token))
                return;

            if (ActionDictionary.ContainsKey(token))
            {
                foreach (var callback in ActionDictionary[token])
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Checks if the given token is valid
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>true when the token is valid, otherwise false</returns>
        private static bool IsTokenValid(ref string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            token = token.ToLower();

            return true;
        }
    }
}
