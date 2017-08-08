using ServiceStack;
using System;
using System.Collections.Generic;
using ServiceStack.Redis;
using ServiceStack.Configuration;
using ServiceStack.Templates;

namespace TemplateWebsites
{
    public class SearchCursorResult
    {
        public int Cursor { get; set; }
        public List<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long Ttl { get; set; }
        public long Size { get; set; }
    }

    public class RedisTemplateFilters
    {
        public IRedisClientsManager RedisManager { get; set; }
        public IAppSettings AppSettings { get; set; }
        T exec<T>(Func<IRedisClient,T> fn)
        {
            using (var db = RedisManager.GetClient())
            {
                return fn(db);
            }
        }
        List<string> parseCommandString(string cmd)
        {
            var args = new List<string>();
            var lastPos = 0;
            for (var i = 0; i < cmd.Length; i++) {
                var c = cmd[i];
                if (c == '{' || c == '[') { 
                    break; //stop splitting args if value is complex type
                }
                if (c == ' ') {
                    var arg = cmd.Substring(lastPos, i);
                    args.Add(arg);
                    lastPos = i + 1;
                }
            }
            args.Add(cmd.Substring(lastPos));
            return args;
        }

        object toObject(RedisText r)
        {
            if (r == null)
                return null;

            if (r.Children != null && r.Children.Count > 0) 
            {
                var to = new List<object>();
                for (var i = 0; i < r.Children.Count; i++) {
                    var child = r.Children[i];
                    var value = child.Text ?? toObject(child);
                    to.Add(value);
                }
                return to;
            }
            return r.Text;
        }

        public object redisCall(string cmd)
        {
            var args = parseCommandString(cmd);
            var redisText = exec(r => r.Custom(args));
            var result = toObject(redisText);
            return result;
        }

        public List<SearchResult> redisSearchKeys(TemplateScopeContext scope, string query) => redisSearchKeys(scope, query, null);
        public List<SearchResult> redisSearchKeys(TemplateScopeContext scope, string query, object options)
        {
            var args = scope.AssertOptions(nameof(redisSearchKeys), options);
            var limit = args.TryGetValue("limit", out object value)
                ? value.ConvertTo<int>()
                : AppSettings.Get("redis.search.limit", 100);

            const string LuaScript = @"
local limit = tonumber(ARGV[2])
local pattern = ARGV[1]
local cursor = tonumber(ARGV[3])
local len = 0
local keys = {}
repeat
    local r = redis.call('scan', cursor, 'MATCH', pattern, 'COUNT', limit)
    cursor = tonumber(r[1])
    for k,v in ipairs(r[2]) do
        table.insert(keys, v)
        len = len + 1
        if len == limit then break end
    end
until cursor == 0 or len == limit
local cursorAttrs = {['cursor'] = cursor, ['results'] = {}}
if len == 0 then
    return cursorAttrs
end
local keyAttrs = {}
for i,key in ipairs(keys) do
    local type = redis.call('type', key)['ok']
    local pttl = redis.call('pttl', key)
    local size = 0
    if type == 'string' then
        size = redis.call('strlen', key)
    elseif type == 'list' then
        size = redis.call('llen', key)
    elseif type == 'set' then
        size = redis.call('scard', key)
    elseif type == 'zset' then
        size = redis.call('zcard', key)
    elseif type == 'hash' then
        size = redis.call('hlen', key)
    end
    local attrs = {['id'] = key, ['type'] = type, ['ttl'] = pttl, ['size'] = size}
    table.insert(keyAttrs, attrs)    
end
cursorAttrs['results'] = keyAttrs
return cjson.encode(cursorAttrs)";

            var json = exec(r => r.ExecCachedLua(LuaScript, sha1 => 
                r.ExecLuaShaAsString(sha1, query, limit.ToString(), "0")));

            var searchResults = json.FromJson<SearchCursorResult>();
            return searchResults.Results;
        }
    }
}
