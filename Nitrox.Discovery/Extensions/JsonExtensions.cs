using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Nitrox.Discovery.Extensions;

internal static class JsonExtensions
{
    extension(JsonNode self)
    {
        public IEnumerable<JsonNode> Children()
        {
            IEnumerable<JsonNode?> nodes = self switch
            {
                JsonArray array => array,
                JsonObject obj => obj.Select(e => e.Value),
                _ => []
            };
            return nodes.Where(n => n != null)!;
        }
    }
}