﻿using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace WikiBrowser.Requests {
    internal class CraftingRequest : StatRequest {
        private const string Title = "How to craft";

        protected override Task<string> Get(Item item) {
            return System.Threading.Tasks.Task.Run(() => GetCrafting(item));
        }

        private static string GetCrafting(Item item) {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}&\n", Title, item.Name);

            var finder = new RecipeFinder();
            finder.SetResult(item.type);
            foreach (var recipe in finder.SearchRecipes()) {
                sb.AppendRecipe(recipe);
            }

            if (sb.Length > (Title.Length + 43)) {
                sb.Remove(sb.Length - 43, 43);
            }

            sb.Append("\n‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾[END]‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾\n");
            return sb.ToString();
        }
    }
}