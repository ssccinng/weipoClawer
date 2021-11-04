// See https://aka.ms/new-console-template for more information
using System.Text.Json;


Console.WriteLine("Hello, World!");

// long uid = 5156340591;
long uid = 6306512172;
// string baseURL = "https://weibo.com/ajax/statuses/mymblog?uid={0}&page={1}&feature=0&since_id=4647690358490849kp3";
string baseURL = "https://weibo.com/ajax/statuses/mymblog?uid={0}&page={1}&feature=0";



HttpClient httpClient = new();

// string root = "longkui";
string root = "214";

if (!Directory.Exists(root))
{
    Directory.CreateDirectory(root);
}
httpClient.DefaultRequestHeaders.Add("Cookie", "加入你的cookie");

httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
// httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
// httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
var a = File.AppendText("帖子jilu.txt");
for (int i = 1; i < 29; ++i)
{
    Console.WriteLine($"正在访问第{i}页");
    var res = await httpClient.GetStringAsync(string.Format(baseURL, uid, i));
    JsonDocument data = JsonDocument.Parse(res);
    Console.WriteLine(string.Format(baseURL, uid, i));
    // File.WriteAllText($"页面{i}.txt", res);
    // continue;
    var postlist = data.RootElement.GetProperty("data").GetProperty("list");
    
    for (int j = 0; j < postlist.GetArrayLength(); ++j)
    {
        var post = postlist[j];
        var text = post.GetProperty("text_raw").GetString();
        // a.WriteLine(text);
        // continue;
        var folder = $"{root}/" + text![..Math.Min(text.Length, 20)].Replace(":", "").Replace("：", "").Replace("\n", "").Replace("\r", "").Replace("*", "").Replace("?", "").Replace("|", "").Replace(" ", "").Replace("/", "") + " " + post.GetProperty("mblogid").GetString();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        File.WriteAllText($"{folder}/content.txt", $"weibo.com/{uid}/{post.GetProperty("mblogid").GetString()}\n{text}");
        var haspostimg = post.TryGetProperty("pic_ids", out JsonElement postimg);
        List<Task<byte[]>> tasks = new();
        if (haspostimg)
        {
            for (int k = 0; k < postimg.GetArrayLength(); ++k)
            {
                tasks.Add(httpClient.GetByteArrayAsync($"https://wx4.sinaimg.cn/mw2000/{postimg[k].GetString()}"));
            }
        }

        var hasrepost = post.TryGetProperty("retweeted_status", out JsonElement repost);

        if (hasrepost)
        {
            var reposthasimg = repost.TryGetProperty("pic_ids", out JsonElement repostimg);
            if (reposthasimg)
            {
                for (int k = 0; k < repostimg.GetArrayLength(); ++k)
                {
                    tasks.Add(httpClient.GetByteArrayAsync($"https://wx4.sinaimg.cn/mw2000/{repostimg[k].GetString()}"));
                }
            }

        }
        else
        {
            File.WriteAllText($"{folder}/log.txt", repost.ToString());
        }
        Console.WriteLine($"获取到{tasks.Count}张图片");
        for (int k = 0; k < tasks.Count; ++k)
        {
            File.WriteAllBytes($"{folder}/{k}.jpg", await tasks[k]);
        }
    }
}